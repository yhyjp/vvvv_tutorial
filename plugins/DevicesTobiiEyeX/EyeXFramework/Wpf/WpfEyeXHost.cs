//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework.Wpf
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using System.Windows.Interop;
    using Tobii.EyeX.Client;
    using Tobii.EyeX.Framework;

    /// <summary>
    /// Provides the main point of contact with the EyeX Engine.
    /// Hosts an EyeX context and responds to engine queries using a repository of WpfInteractors.
    /// </summary>
    public class WpfEyeXHost : EyeXHost
    {
        private readonly object _lock = new object();

        // Collection of wpf interactors, which are objects holding a reference to a FrameworkElement
        // and its associated EyeXBehaviors. Weak references are used to avoid memory leaks, we want
        // the garbage collector to be able to collect closed windows and removed elements.
        private readonly IDictionary<string, WeakReference> _interactors = new Dictionary<string, WeakReference>();

        // Flag to keep track of the dirty state of interactors having been added or removed since
        // the last time the interactor properties were updated.
        private bool _interactorsHaveChanged = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfEyeXHost"/> class.
        /// </summary>
        public WpfEyeXHost()
        {
            WpfInteractor.InteractorAdded += OnInteractorAdded;
            WpfInteractor.InteractorRemoved += OnInteractorRemoved;
        }

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        /// <param name="isDisposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool isDisposing)
        {
            WpfInteractor.InteractorAdded -= OnInteractorAdded;
            WpfInteractor.InteractorRemoved -= OnInteractorRemoved;

            base.Dispose(isDisposing);
        }

        /// <summary>
        /// Handles a query from the EyeX Engine.
        /// </summary>
        /// <remarks>
        /// Called from a worker thread. GUI object access has to be dispatched to UI thread.
        /// </remarks>
        /// <param name="query">Query to be handled.</param>
        protected override void HandleQuery(Query query)
        {
            if (!RunOnMainThread(new Action(() => HandleQueryInner(query))))
            {
                query.Dispose();
            }
        }

        /// <summary>
        /// Handles an event from the EyeX Engine.
        /// </summary>
        /// <remarks>
        /// Called from a worker thread. GUI object access has to be dispatched to UI thread.
        /// </remarks>
        /// <param name="event_">Event to be handled.</param>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Event is a reserved word.")]
        protected override void HandleEvent(InteractionEvent event_)
        {
            if (!RunOnMainThread(new Action(() => HandleEventInner(event_))))
            {
                event_.Dispose();
            }
        }

        /// <summary>
        /// Marshals the given operation to the UI thread.
        /// </summary>
        /// <param name="action">The operation to be performed.</param>
        /// <returns>True if the marshaling operation was successful.</returns>
        private static bool RunOnMainThread(Action action)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke(action);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static string GetWindowId(Window window)
        {
            var windowHelper = new WindowInteropHelper(window);
            return windowHelper.Handle.ToString();
        }

        private static bool TryGetQueryRectangle(Query query, out System.Windows.Rect queryRect)
        {
            using (var queryBounds = query.Bounds)
            {
                double x, y, w, h;
                if (queryBounds.TryGetRectangularData(out x, out y, out w, out h))
                {
                    queryRect = new System.Windows.Rect((int)x, (int)y, (int)w, (int)h);
                    return true;
                }
                else
                {
                    queryRect = new System.Windows.Rect();
                    return false;
                }
            }
        }

        private void HandleQueryInner(Query query)
        {
            using (query)
            {
                var updateInteractors = false;
                lock (_lock)
                {
                    if (_interactorsHaveChanged)
                    {
                        updateInteractors = true;
                        _interactorsHaveChanged = false;
                    }
                }

                System.Windows.Rect queryRect;
                if (!TryGetQueryRectangle(query, out queryRect)) { return; }

                foreach (var window in GetWindowsWithInteractors())
                {
                    if (updateInteractors)
                    {
                        WpfCrawler.UpdateInteractorProperties(window, Literals.RootId);
                    }

                    if (query.WindowIds.Contains(GetWindowId(window)))
                    {
                        var interactorElements = WpfCrawler.GetInteractors(window, queryRect);
                        SendQueryReply(query, interactorElements);
                    }
                }
            }
        }

        private void HandleEventInner(InteractionEvent event_)
        {
            using (event_)
            {
                try
                {
                    if (!HandleDataStreamEvent(event_))
                    {
                        var interactorId = event_.InteractorId;
                        var interactor = GetInteractor(interactorId);
                        if (interactor != null)
                        {
                            interactor.HandleEvent(event_);
                        }
                    }
                }
                catch (InteractionApiException ex)
                {
                    Debug.WriteLine("EyeX event handler failed: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Handles an InteractorAdded event, and adds the interactor to the WpfEyeXHost's
        /// repository of interactors.
        /// </summary>
        /// <param name="sender">The sender of the OnInteractorAdded event.</param>
        /// <param name="args">The <see cref="ElementEventArgs"/>.</param>
        private void OnInteractorAdded(object sender, ElementEventArgs args)
        {
            var element = args.Element;
            var interactor = element.GetWpfInteractor();

            lock (_lock)
            {
                _interactors.Add(interactor.Id, new WeakReference(interactor));
                _interactorsHaveChanged = true;
            }
        }

        /// <summary>
        /// Handles an InteractorRemoved event, and removes the interactor from the WpfEyeXHost's
        /// repository of interactors.
        /// </summary>
        /// <param name="sender">The sender of the OnInteractorRemoved event.</param>
        /// <param name="args">The <see cref="ElementEventArgs"/>.</param>
        private void OnInteractorRemoved(object sender, ElementEventArgs args)
        {
            var element = args.Element;
            var interactor = element.GetWpfInteractor();

            lock (_lock)
            {
                _interactors.Remove(interactor.Id);
                _interactorsHaveChanged = true;
            }
        }

        /// <summary>
        /// Returns all windows for which there are child elements that are interactors.
        /// </summary>
        /// <returns>All windows with interactors, or an empty list if there are no interactors.</returns>
        private IEnumerable<Window> GetWindowsWithInteractors()
        {
            return GetInteractors()
                .Where(interactor => interactor.HasValidWindowId)
                .Select(interactor => Window.GetWindow(interactor.Element))
                .Where(window => window != null)
                .Distinct();
        }

        /// <summary>
        /// Gets all interactors.
        /// </summary>
        /// <returns>Collection of interactors.</returns>
        private IEnumerable<WpfInteractor> GetInteractors()
        {
            IEnumerable<WpfInteractor> interactors;
            lock (_lock)
            {
                interactors = _interactors
                    .Values
                    .Select(interactorReference => interactorReference.Target as WpfInteractor)
                    .ToList() // to prevent them from being gc'ed right in front of our eyes
                    .Where(interactor => interactor != null && interactor.IsInteractor);
            }

            return interactors;
        }

        /// <summary>
        /// Gets the interactor with the specified interactor id.
        /// </summary>
        /// <param name="interactorId">The id of the interactor to be returned.</param>
        /// <returns>The interactor with Id interactorId, or null if not found.</returns>
        private WpfInteractor GetInteractor(string interactorId)
        {
            lock (_lock)
            {
                WeakReference interactorReference;
                if (_interactors.TryGetValue(interactorId, out interactorReference))
                {
                    return interactorReference.Target as WpfInteractor;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a commits a snapshot with the given interactors.
        /// </summary>
        /// <param name="query">The query the snapshot is a reply to.</param>
        /// <param name="interactors">A list of interactors intersecting the given query's bounds.</param>
        private void SendQueryReply(Query query, IEnumerable<WpfInteractor> interactors)
        {
            try
            {
                using (var snapshot = CreateSnapshot(query))
                {
                    foreach (var interactor in interactors)
                    {
                        interactor.AddToSnapshot(snapshot);
                    }

                    CommitSnapshot(snapshot);
                }
            }
            catch (InteractionApiException ex)
            {
                Debug.WriteLine("Could not create and commit snapshot: " + ex.Message);
            }
        }
    }
}
