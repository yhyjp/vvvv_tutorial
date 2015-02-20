//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Tobii.EyeX.Client;
    using Tobii.EyeX.Framework;
    using Environment = Tobii.EyeX.Client.Environment;

    /// <summary>
    /// Base class for EyeX Hosts, independent of GUI frameworks.
    /// Provides the main point of contact with the EyeX Engine.
    /// </summary>
    public partial class EyeXHost : IDisposable
    {
        // Data streams
        private readonly List<IDataStreamObserver> _dataStreams = new List<IDataStreamObserver>();
        private readonly List<string> _deletedDataStreams = new List<string>();

        // Engine state accessors
        private readonly EngineStateAccessor<Rect> _screenBoundsStateAccessor;
        private readonly EngineStateAccessor<Size2> _displaySizeStateAccessor;
        private readonly EngineStateAccessor<EyeTrackingDeviceStatus> _eyeTrackingDeviceStatusStateAccessor;
        private readonly EngineStateAccessor<UserPresence> _userPresenceStateAccessor;
        private readonly EngineStateAccessor<string> _userProfileNameStateAccessor;

        // EyeX client library handle.
        private Environment _environment;

        // EyeX context representing the connection to the EyeX Engine.
        private Context _context;

        // Queue for deferred initialization actions.
        private readonly Queue<Action> _deferredInitialization = new Queue<Action>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EyeXHost"/> class.
        /// </summary>
        public EyeXHost()
        {
            _screenBoundsStateAccessor = new EngineStateAccessor<Rect>(StatePaths.ScreenBounds);
            _displaySizeStateAccessor = new EngineStateAccessor<Size2>(StatePaths.DisplaySize);
            _eyeTrackingDeviceStatusStateAccessor = new EngineStateAccessor<EyeTrackingDeviceStatus>(StatePaths.EyeTrackingState);
            _userPresenceStateAccessor = new EngineStateAccessor<UserPresence>(StatePaths.UserPresence);
            _userProfileNameStateAccessor = new EngineStateAccessor<string>(StatePaths.ProfileName);
        }

        /// <summary>
        /// Gets the availability of the EyeX Engine.
        /// </summary>
        public static EyeXAvailability EyeXAvailability
        {
            get { return Environment.GetEyeXAvailability(); }
        }

        /// <summary>
        /// Event raised when the screen bounds have changed.
        /// </summary>
        public event EventHandler<EngineStateValue<Rect>> ScreenBoundsChanged
        {
            add { _screenBoundsStateAccessor.Changed += value; }
            remove { _screenBoundsStateAccessor.Changed -= value; }
        }

        /// <summary>
        /// Event raised when the display size has changed.
        /// </summary>
        public event EventHandler<EngineStateValue<Size2>> DisplaySizeChanged
        {
            add { _displaySizeStateAccessor.Changed += value; }
            remove { _displaySizeStateAccessor.Changed -= value; }
        }

        /// <summary>
        /// Event raised when the eye tracking device status has changed.
        /// </summary>
        public event EventHandler<EngineStateValue<EyeTrackingDeviceStatus>> EyeTrackingDeviceStatusChanged
        {
            add { _eyeTrackingDeviceStatusStateAccessor.Changed += value; }
            remove { _eyeTrackingDeviceStatusStateAccessor.Changed -= value; }
        }

        /// <summary>
        /// Event raised when the user presence status has changed.
        /// </summary>
        public event EventHandler<EngineStateValue<UserPresence>> UserPresenceChanged
        {
            add { _userPresenceStateAccessor.Changed += value; }
            remove { _userPresenceStateAccessor.Changed -= value; }
        }

        /// <summary>
        /// Event raised when the user profile name has changed.
        /// </summary>
        public event EventHandler<EngineStateValue<string>> UserProfileNameChanged
        {
            add { _userProfileNameStateAccessor.Changed += value; }
            remove { _userProfileNameStateAccessor.Changed -= value; }
        }

        /// <summary>
        /// Gets the engine state: Screen bounds in pixels.
        /// </summary>
        public EngineStateValue<Rect> ScreenBounds
        {
            get { return _screenBoundsStateAccessor.GetCurrentValue(); }
        }

        /// <summary>
        /// Gets the engine state: Display size, width and height, in millimeters.
        /// </summary>
        public EngineStateValue<Size2> DisplaySize
        {
            get { return _displaySizeStateAccessor.GetCurrentValue(); }
        }

        /// <summary>
        /// Gets the engine state: Eye tracking status.
        /// </summary>
        public EngineStateValue<EyeTrackingDeviceStatus> EyeTrackingDeviceStatus
        {
            get { return _eyeTrackingDeviceStatusStateAccessor.GetCurrentValue(); }
        }

        /// <summary>
        /// Gets the engine state: User presence.
        /// </summary>
        public EngineStateValue<UserPresence> UserPresence
        {
            get { return _userPresenceStateAccessor.GetCurrentValue(); }
        }

        /// <summary>
        /// Gets the engine state: User profile name.
        /// </summary>
        public EngineStateValue<string> UserProfileName
        {
            get { return _userProfileNameStateAccessor.GetCurrentValue(); }
        }

        /// <summary>
        /// Gets whether or not the <see cref="EyeXHost"/> has been started.
        /// </summary>
        public bool IsStarted
        {
            get { return _context != null; }
        }

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initializes the EyeX host and enables the connection to the EyeX Engine.
        /// </summary>
        public virtual void Start()
        {
            if (_context != null)
            {
                throw new InvalidOperationException("The EyeX host cannot be re-started.");
            }

            // Initialize the EyeX Engine client library.
            _environment = Environment.Initialize(LogTarget.Trace);

            // Create a context, register event handlers, and enable the connection to the engine.
            _context = new Context(false);
            _context.RegisterQueryHandlerForCurrentProcess(HandleQuery);
            _context.RegisterEventHandler(HandleEvent);
            _context.ConnectionStateChanged += OnConnectionStateChanged;

            _screenBoundsStateAccessor.OnContextCreated(_context);
            _displaySizeStateAccessor.OnContextCreated(_context);
            _eyeTrackingDeviceStatusStateAccessor.OnContextCreated(_context);
            _userPresenceStateAccessor.OnContextCreated(_context);
            _userProfileNameStateAccessor.OnContextCreated(_context);

            _context.EnableConnection();

            // Execute deferred initialization logic.
            while (_deferredInitialization.Count > 0)
            {
                _deferredInitialization.Dequeue().Invoke();
            }
        }

        /// <summary>
        /// Gets a gaze point data stream.
        /// </summary>
        /// <param name="mode">Specifies the kind of data processing to be applied by the EyeX Engine.</param>
        /// <returns>The data stream.</returns>
        public GazePointDataStream CreateGazePointDataStream(GazePointDataMode mode)
        {
            var dataStream = new GazePointDataStream(mode);
            RegisterDataStreamObserver(dataStream);
            return dataStream;
        }

        /// <summary>
        /// Gets a fixation data stream.
        /// </summary>
        /// <param name="mode">Specifies the kind of data processing to be applied by the EyeX Engine.</param>
        /// <returns>The data stream.</returns>
        public FixationDataStream CreateFixationDataStream(FixationDataMode mode)
        {
            var dataStream = new FixationDataStream(mode);
            RegisterDataStreamObserver(dataStream);
            return dataStream;
        }

        /// <summary>
        /// Gets an eye position data stream.
        /// </summary>
        /// <returns>The data stream.</returns>
        public EyePositionDataStream CreateEyePositionDataStream()
        {
            var dataStream = new EyePositionDataStream();
            RegisterDataStreamObserver(dataStream);
            return dataStream;
        }

        /// <summary>
        /// Trigger an activation ("direct click").
        /// Use this method if you want to bind the click command to a key other than the one used 
        /// in the EyeX Interaction settings -- or to something other than a key press event.
        /// </summary>
        public void TriggerActivation()
        {
            EnsureStarted();

            _context.CreateActionCommand(ActionType.Activate)
                .ExecuteAsync(null);
        }

        /// <summary>
        /// Gets the EyeX Engine version.
        /// </summary>
        /// <returns>A <see cref="Task{Version}"/> that retrieves the EyeX Engine version.</returns>
        /// <remarks>The task result will evaluate to <c>null</c> if the EyeX Engine version can't be retrieved.</remarks>
        public Task<Version> GetEngineVersion()
        {
            return Task.Run<Version>(() =>
            {
                // Get the state (synchronously).
                var stateBag = _context.GetState(StatePaths.EngineVersion);
                string value = null;
                if (stateBag.TryGetStateValue<string>(out value, StatePaths.EngineVersion))
                {
                    // Parse the version.
                    Version version = null;
                    if (Version.TryParse(value, out version))
                    {
                        return version;
                    }
                }
                return null;
            });
        }

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        /// <param name="isDisposing">true if managed resources should be disposed; otherwise, false.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (_context != null && !_context.IsInvalid)
                {
                    // The context must be shut down before disposing.
                    try
                    {
                        _context.Shutdown();
                    }
                    catch (InteractionApiException ex)
                    {
                        Debug.WriteLine("EyeX context shutdown failed: " + ex.Message);
                    }

                    _context.Dispose();
                    _context = null;
                }

                if (_environment != null)
                {
                    _environment.Dispose();
                    _environment = null;
                }
            }
        }

        /// <summary>
        /// Handles a query from the EyeX Engine.
        /// </summary>
        /// <param name="query">Query to be handled.</param>
        protected virtual void HandleQuery(Query query)
        {
            // The default implementation does nothing.
            // Deriving classes are expected to override this method if they handle region-bound interactors.
            query.Dispose();
        }

        /// <summary>
        /// Handles an event from the EyeX Engine.
        /// </summary>
        /// <param name="event_">Event to be handled.</param>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Event is a reserved word.")]
        protected virtual void HandleEvent(InteractionEvent event_)
        {
            using (event_)
            {
                // If you derive from this class, make sure to call HandleDataStreamEvent.
                HandleDataStreamEvent(event_);
            }
        }

        /// <summary>
        /// Handles a connection-state-changed notification from the EyeX Engine.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event parameters.</param>
        protected virtual void OnConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "The connection state is now {0}.", e.State));

            if (e.State == ConnectionState.Connected)
            {
                // Send a snapshot including all data streams, now that the connection 
                // to the EyeX Engine is established.
                SendDataStreamSnapshot();

                _screenBoundsStateAccessor.OnConnected(_context);
                _displaySizeStateAccessor.OnConnected(_context);
                _eyeTrackingDeviceStatusStateAccessor.OnConnected(_context);
                _userPresenceStateAccessor.OnConnected(_context);
                _userProfileNameStateAccessor.OnConnected(_context);
            }
            else
            {
                _screenBoundsStateAccessor.OnDisconnected();
                _displaySizeStateAccessor.OnDisconnected();
                _eyeTrackingDeviceStatusStateAccessor.OnDisconnected();
                _userPresenceStateAccessor.OnDisconnected();
                _userProfileNameStateAccessor.OnDisconnected();
            }
        }

        /// <summary>
        /// Check if the event is for one of the data streams bound to this host. 
        /// If it is, then let the data stream handle it.
        /// </summary>
        /// <param name="event_">Event to be handled.</param>
        /// <returns>True if the event was handled.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Event is a reserved word.")]
        protected bool HandleDataStreamEvent(InteractionEvent event_)
        {
            lock (_dataStreams)
            {
                var stream = _dataStreams
                    .FirstOrDefault(x => string.Equals(event_.InteractorId, x.Id));

                if (stream != null)
                {
                    stream.HandleEvent(event_);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Creates a snapshot.
        /// </summary>
        /// <returns>A snapshot.</returns>
        protected Snapshot CreateSnapshot()
        {
            return _context.CreateSnapshot();
        }

        /// <summary>
        /// Creates a snapshot using the bounds and window id's from the given query.
        /// </summary>
        /// <returns>A snapshot.</returns>
        /// <param name="query">The query that spawned the snapshot creation.</param>
        protected Snapshot CreateSnapshot(Query query)
        {
            var snapshot = _context.CreateSnapshotWithQueryBounds(query);

            foreach (var windowId in query.WindowIds)
            {
                snapshot.AddWindowId(windowId);
            }

            return snapshot;
        }

        /// <summary>
        /// Commits the snapshot asynchronously to the EyeX Engine.
        /// </summary>
        /// <param name="snapshot">The snapshot to be committed.</param>
        protected void CommitSnapshot(Snapshot snapshot)
        {
#if DEBUG
            snapshot.CommitAsync(OnSnapshotCommitted);
#else
            snapshot.CommitAsync(null);
#endif
        }

#if DEBUG
        private static string GetErrorMessage(AsyncData asyncData)
        {
            string errorMessage;
            if (asyncData.TryGetPropertyValue<string>(Literals.ErrorMessage, out errorMessage))
            {
                return errorMessage;
            }
            else
            {
                return "Unspecified error.";
            }
        }

        /// <summary>
        /// Callback method useful during application development to find malformed or
        /// incorrectly set up snapshots and interactors.
        /// </summary>
        /// <param name="asyncData">Data packet that contains value code and error messages.</param>
        private static void OnSnapshotCommitted(AsyncData asyncData)
        {
            using (asyncData)
            {
                ResultCode resultCode;
                Debug.Assert(asyncData.TryGetResultCode(out resultCode) != false, "Failed to read value code after committing snapshot.");
                Debug.Assert(resultCode != ResultCode.InvalidSnapshot, "Snapshot validation failed: " + GetErrorMessage(asyncData));
            }
        }
#endif

        private void RegisterDataStreamObserver(IDataStreamObserver observer)
        {
            if (IsStarted)
            {
                // Perform registration of the data stream observer.
                RegisterDataStreamObserverCore(observer);                
            }
            else
            {
                // Add registration of the data stream observer to the deferred intitialization queue.
                // The actions in this queue will be executed when the EyeX host has been started.
                _deferredInitialization.Enqueue(() => RegisterDataStreamObserverCore(observer));
            }
        }

        private void RegisterDataStreamObserverCore(IDataStreamObserver observer)
        {
            EnsureStarted();

            observer.Disposed += OnDataStreamDisposed;

            lock (_dataStreams)
            {
                _dataStreams.Add(observer);
            }

            SendDataStreamSnapshot();            
        }

        private void SendDataStreamSnapshot()
        {
            if (_context == null ||
                _context.IsInvalid ||
                _context.ConnectionState != ConnectionState.Connected ||
                (_dataStreams.Count == 0 && _deletedDataStreams.Count == 0))
            {
                return;
            }

            var snapshot = _context.CreateSnapshot();

            snapshot.CreateBounds(BoundsType.None);
            snapshot.AddWindowId(Literals.GlobalInteractorWindowId);

            lock (_dataStreams)
            {
                foreach (var stream in _dataStreams)
                {
                    stream.CreateInteractor(snapshot);
                }

                foreach (var interactorId in _deletedDataStreams)
                {
                    var interactor = snapshot.CreateInteractor(interactorId, Literals.RootId, Literals.GlobalInteractorWindowId);
                    interactor.CreateBounds(BoundsType.None);
                    interactor.IsDeleted = true;
                }

                _deletedDataStreams.Clear();
            }

            CommitSnapshot(snapshot);
        }

        private void OnDataStreamDisposed(object sender, EventArgs e)
        {
            var dataStream = (IDataStreamObserver)sender;
            dataStream.Disposed -= OnDataStreamDisposed;

            lock (_dataStreams)
            {
                _dataStreams.Remove(dataStream);
                _deletedDataStreams.Add(dataStream.Id);
            }

            SendDataStreamSnapshot();
        }

        private void EnsureStarted()
        {
            if (!IsStarted)
            {
                throw new InvalidOperationException("The EyeX host must be started before this method is called.");
            }
        }
    }
}
