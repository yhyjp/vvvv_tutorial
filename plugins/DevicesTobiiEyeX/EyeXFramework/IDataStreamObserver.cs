//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework
{
    using System;
    using Tobii.EyeX.Client;

    /// <summary>
    /// Interface for a data stream observer.
    /// This is an internal interface used by the EyeXHost.
    /// </summary>
    internal interface IDataStreamObserver
    {
        /// <summary>
        /// Event raised when the observer is disposed.
        /// </summary>
        event EventHandler Disposed;

        /// <summary>
        /// Gets the unique ID of the observer.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Creates an interactor for the data stream.
        /// </summary>
        /// <param name="snapshot">Snapshot to add the interactor to.</param>
        void CreateInteractor(Snapshot snapshot);

        /// <summary>
        /// Handles an event from the EyeX Engine.
        /// </summary>
        /// <param name="event_">Event to be handled.</param>
        void HandleEvent(InteractionEvent event_);
    }
}