//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework
{
    using System.Collections.Generic;
    using Tobii.EyeX.Client;
    using Tobii.EyeX.Framework;

    /// <summary>
    /// Interface of an EyeX behavior adapter, capable of assigning a certain type 
    /// of behavior to an interactor, and of interpreting the corresponding events.
    /// <para>
    /// Note: this package includes adapters for the most common behaviors, so there is 
    /// usually no need to write your own.
    /// </para>
    /// </summary>
    public interface IEyeXBehavior
    {
        /// <summary>
        /// Gets the type of EyeX behavior.
        /// </summary>
        BehaviorType BehaviorType { get; }

        /// <summary>
        /// Assigns the behavior to an Interactor. 
        /// This method is invoked when a snapshot is being prepared for the EyeX Engine.
        /// </summary>
        /// <param name="interactor">Interactor to be modified.</param>
        void AssignBehavior(Interactor interactor);

        /// <summary>
        /// Handles an event from the EyeX Engine and calls the registered event handlers/callbacks.
        /// </summary>
        /// <param name="sender">Object associated with the interactor.</param>
        /// <param name="behaviors">The <see cref="Behavior"/> instances containing the event data.</param>
        void HandleEvent(object sender, IEnumerable<Behavior> behaviors);
    }
}
