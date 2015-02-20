//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Tobii.EyeX.Client;
    using Tobii.EyeX.Framework;

    /// <summary>
    /// Abstract base class for data stream observers.
    /// Observes a data stream and raises events when new data points become available.
    /// </summary>
    /// <typeparam name="T">Data type of the events.</typeparam>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Keep interface implementation together.")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "Keep interface implementation together.")]
    public abstract class DataStreamBase<T> : IDisposable, IDataStreamObserver where T : EventArgs
    {
        private readonly string _id;
        private EventHandler _disposedEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataStreamBase{T}"/> class.
        /// </summary>
        protected DataStreamBase()
        {
            _id = InteractorIDGenerator.CreateUniqueID();
        }

        /// <summary>
        /// Event raised when a data point is available.
        /// </summary>
        public event EventHandler<T> Next;

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        /// <param name="disposing">Indicates whether managed resources should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Next = null;

                var handler = _disposedEvent;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Assigns the behavior corresponding to the data stream type to an interactor.
        /// </summary>
        /// <param name="interactor">The global interactor to add the data stream behavior to.</param>
        protected abstract void AssignBehavior(Interactor interactor);

        /// <summary>
        /// Extracts data points from an event from the EyeX Engine.
        /// </summary>
        /// <param name="behaviors">The <see cref="Behavior"/> instances containing the event data.</param>
        /// <returns>The collection of data points.</returns>
        protected abstract IEnumerable<T> ExtractDataPoints(IEnumerable<Behavior> behaviors);

        #region Explicit implementation of the IDataStreamObserver interface

        /// <summary>
        /// Explicit implementation of <see cref="IDataStreamObserver.Disposed"/>.
        /// </summary>
        event EventHandler IDataStreamObserver.Disposed
        {
            add { _disposedEvent += value; }
            remove { _disposedEvent -= value; }
        }

        /// <summary>
        /// Gets the <see cref="IDataStreamObserver.Id"/>.
        /// </summary>
        string IDataStreamObserver.Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Explicit implementation of <see cref="IDataStreamObserver.CreateInteractor"/>.
        /// </summary>
        /// <param name="snapshot">Snapshot to add the interactor to.</param>
        void IDataStreamObserver.CreateInteractor(Snapshot snapshot)
        {
            using (var interactor = snapshot.CreateInteractor(_id, Literals.RootId, Literals.GlobalInteractorWindowId))
            {
                var bounds = interactor.CreateBounds(BoundsType.None);
                bounds.Dispose();

                AssignBehavior(interactor);
            }
        }

        /// <summary>
        /// Explicit implementation of <see cref="IDataStreamObserver.HandleEvent"/>.
        /// </summary>
        /// <param name="sender">Object associated with the interactor.</param>
        /// <param name="event_">Event to be handled.</param>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Event is a reserved word.")]
        void IDataStreamObserver.HandleEvent(InteractionEvent event_)
        {
            var handler = Next;
            if (handler != null)
            {
                var eventBehaviors = event_.Behaviors;

                foreach (var eventArgs in ExtractDataPoints(eventBehaviors))
                {
                    handler(this, eventArgs);
                }

                foreach (var eventBehavior in eventBehaviors)
                {
                    eventBehavior.Dispose();
                }
            }
        }

        #endregion
    }
}
