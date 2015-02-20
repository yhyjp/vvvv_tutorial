//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework.Forms
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using Tobii.EyeX.Client;

    /// <summary>
    /// Provides the main point of contact with the EyeX Engine. 
    /// Hosts an EyeX context and responds to engine queries using a repository of BehaviorMaps.
    /// </summary>
    public class FormsEyeXHost : EyeXHost, IFormsEyeXHost
    {
        // Collection of behavior maps, that is, the objects holding the EyeX interactors and behaviors.
        // We use weak references because we don't want to prevent the Forms and Controls associated with 
        // the behavior maps from being reaped by the Grim Garbage Collector.
        private readonly List<WeakReference> _behaviorMaps = new List<WeakReference>();

        /// <summary>
        /// Initializes the EyeX host and enables the connection to the EyeX Engine.
        /// </summary>
        public override void Start()
        {
            DpiAwarenessUtilities.SetProcessDpiAware();

            base.Start();
        }

        /// <summary>
        /// Connects a behavior map so that it may receive queries and events from the EyeX Engine.
        /// </summary>
        /// <param name="behaviorMap">Map to be connected.</param>
        public void Connect(BehaviorMap behaviorMap)
        {
            ((IComponent)behaviorMap).Disposed += OnBehaviorMapDisposed;
            _behaviorMaps.Add(new WeakReference(behaviorMap));
        }

        /// <summary>
        /// Handles a query from the EyeX Engine.
        /// </summary>
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
        /// <param name="event_">Event to be handled.</param>
        protected override void HandleEvent(InteractionEvent event_)
        {
            if (!RunOnMainThread(new Action(() => HandleEventInner(event_))))
            {
                event_.Dispose();
            }
        }

        /// <summary>
        /// Marshals the given operation to the main thread using the first open form that we can find.
        /// </summary>
        /// <param name="action">The operation to be performed.</param>
        /// <returns>True if the marshaling operation was successful.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception types cannot be identified with certainty.")]
        private static bool RunOnMainThread(Action action)
        {
            if (Application.OpenForms.Count > 0)
            {
                try
                {
                    var form = Application.OpenForms[0];
                    form.BeginInvoke(action);
                    return true;
                }
                catch (Exception ex)
                {
                    // the OpenForms collection might change between the call to OpenForms.Count 
                    // and OpenForms[] -- ignore any such errors.
                    Debug.WriteLine("Failed to marshal call to the main thread: " + ex.Message);
                }
            }

            return false;
        }

        private static bool TryGetQueryRectangle(Query query, out Rectangle queryRect)
        {
            using (var queryBounds = query.Bounds)
            {
                double x, y, w, h;
                if (queryBounds.TryGetRectangularData(out x, out y, out w, out h))
                {
                    queryRect = new Rectangle((int)x, (int)y, (int)(w + 0.5), (int)(h + 0.5));
                    return true;
                }
                else
                {
                    queryRect = new Rectangle();
                    return false;
                }
            }
        }

        private static void UpdateInteractorPropertiesIfNecessary(IEnumerable<BehaviorMap> behaviorMaps)
        {
            if (!behaviorMaps.Any(x => x.IsModified))
            {
                return;
            }

            FormsCrawler.UpdateInteractorProperties(behaviorMaps);

            foreach (var connector in behaviorMaps)
            {
                connector.MarkAsUnmodified();
            }
        }

        private void HandleQueryInner(Query query)
        {
            using (query)
            {
                var maps = GetBehaviorMaps();
                UpdateInteractorPropertiesIfNecessary(maps);

                Rectangle queryRect;
                if (TryGetQueryRectangle(query, out queryRect))
                {
                    var interactors = FormsCrawler.GetInteractors(maps, queryRect, query.WindowIds);
                    SendQueryReply(query, interactors);
                }

                RemoveStaleBehaviorMaps();
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
                        foreach (var map in GetBehaviorMaps())
                        {
                            if (map.HandleEvent(event_))
                            {
                                break;
                            }
                        }
                    }
                }
                catch (InteractionApiException ex)
                {
                    Debug.WriteLine("EyeX event handler failed: " + ex.Message);
                }
            }
        }

        private IEnumerable<BehaviorMap> GetBehaviorMaps()
        {
            return _behaviorMaps
                .Select(reference => reference.Target as BehaviorMap)
                .ToList() // to prevent them from being gc'ed right in front of our eyes
                .Where(map => map != null);
        }

        private void OnBehaviorMapDisposed(object sender, EventArgs e)
        {
            _behaviorMaps.RemoveAll(reference => reference.Target == sender);
        }

        private void RemoveStaleBehaviorMaps()
        {
            _behaviorMaps.RemoveAll(reference => reference.Target == null);
        }

        private void SendQueryReply(Query query, IEnumerable<FormsInteractor> interactors)
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
