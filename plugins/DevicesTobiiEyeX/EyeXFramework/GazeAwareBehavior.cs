//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Tobii.EyeX.Client;
    using Tobii.EyeX.Framework;

    /// <summary>
    /// Maps the Gaze-aware behavior to an interactor.
    /// Exposes EyeX behavior parameters and events as .NET properties and events.
    /// </summary>
    public class GazeAwareBehavior : IEyeXBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GazeAwareBehavior"/> class.
        /// </summary>
        public GazeAwareBehavior()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeAwareBehavior"/> class.
        /// </summary>
        /// <param name="eventHandler">The event handler for the <see cref="GazeAware"/> event.</param>
        public GazeAwareBehavior(EventHandler<GazeAwareEventArgs> eventHandler)
        {
            GazeAware += eventHandler;
        }

        /// <summary>
        /// The event raised when the gaze moves in or out if the interactor.
        /// </summary>
        public event EventHandler<GazeAwareEventArgs> GazeAware;

        /// <summary>
        /// Gets the gaze-aware behavior type.
        /// </summary>
        public BehaviorType BehaviorType
        {
            get { return BehaviorType.GazeAware; }
        }

        /// <summary>
        /// Gets or sets the delay in milliseconds from when the user looks at the interactor to the event is raised.
        /// </summary>
        public int DelayMilliseconds { get; set; }

        /// <summary>
        /// Assigns the gaze-aware behavior to an interactor. 
        /// </summary>
        /// <param name="interactor">Interactor to assign the gaze-aware interactor to.</param>
        public void AssignBehavior(Interactor interactor)
        {
            using (var behavior = interactor.CreateBehavior(BehaviorType.GazeAware))
            {
                if (DelayMilliseconds > 0)
                {
                    var parameters = new GazeAwareParams { GazeAwareMode = GazeAwareMode.Delayed, DelayTime = DelayMilliseconds };
                    behavior.SetGazeAwareParams(ref parameters);
                }
            }
        }

        /// <summary>
        /// Handles the gaze-aware event.
        /// </summary>
        /// <param name="sender">Object associated with the interactor.</param>
        /// <param name="behaviors">The <see cref="Behavior"/> instances containing the event data.</param>
        public void HandleEvent(object sender, IEnumerable<Behavior> behaviors)
        {
            foreach (var behavior in behaviors
                .Where(b => b.BehaviorType == BehaviorType.GazeAware))
            {
                GazeAwareEventParams parameters;
                if (behavior.TryGetGazeAwareEventParams(out parameters))
                {
                    var handler = GazeAware;
                    if (handler != null)
                    {
                        handler(sender, new GazeAwareEventArgs(parameters.HasGaze != EyeXBoolean.False));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Event arguments for the gaze-aware behavior.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Belongs here.")]
    public sealed class GazeAwareEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GazeAwareEventArgs"/> class.
        /// </summary>
        /// <param name="hasGaze">True if the gaze point falls within the bounds of the interactor.</param>
        public GazeAwareEventArgs(bool hasGaze)
        {
            HasGaze = hasGaze;
        }

        /// <summary>
        /// Gets a value indicating whether the gaze point falls within the bounds of the interactor.
        /// </summary>
        public bool HasGaze { get; private set; }
    }
}
