//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace EyeXFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tobii.EyeX.Client;
    using Tobii.EyeX.Framework;

    /// <summary>
    /// Maps the Activatable behavior to an interactor.
    /// Exposes EyeX behavior parameters and events as .NET properties and events.
    /// </summary>
    public class ActivatableBehavior : IEyeXBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivatableBehavior"/> class.
        /// </summary>
        public ActivatableBehavior()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivatableBehavior"/> class.
        /// </summary>
        /// <param name="activatedHandler">Event handler for the <see cref="Activated"/> event.</param>
        public ActivatableBehavior(EventHandler activatedHandler)
            : this(activatedHandler, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivatableBehavior"/> class.
        /// </summary>
        /// <param name="activatedHandler">Event handler for the <see cref="Activated"/> event.</param>
        /// <param name="activationFocusChangedHandler">Event handler for the <see cref="ActivationFocusChanged"/> event.</param>
        public ActivatableBehavior(EventHandler activatedHandler, EventHandler<ActivationFocusChangedEventArgs> activationFocusChangedHandler)
        {
            if (activatedHandler != null)
            {
                Activated += activatedHandler;
            }

            if (activationFocusChangedHandler != null)
            {
                ActivationFocusChanged += activationFocusChangedHandler;
            }
        }

        /// <summary>
        /// The activated event.
        /// </summary>
        public event EventHandler Activated;

        /// <summary>
        /// The activation focus event.
        /// </summary>
        public event EventHandler<ActivationFocusChangedEventArgs> ActivationFocusChanged;

        /// <summary>
        /// Gets the type of behavior provided by the adapter.
        /// </summary>
        public BehaviorType BehaviorType
        {
            get { return BehaviorType.Activatable; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to enable tentative focus.
        /// </summary>
        public bool IsTentativeFocusEnabled { get; set; }

        /// <summary>
        /// Assigns the activatable behavior to an interactor.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        public void AssignBehavior(Interactor interactor)
        {
            var parameters = new ActivatableParams { EnableTentativeFocus = new EyeXBoolean(IsTentativeFocusEnabled).integerValue };
            interactor.CreateActivatableBehavior(ref parameters);
        }

        /// <summary>
        /// Handles interaction events.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="behaviors">The <see cref="Behavior"/> instances containing the event data.</param>
        public void HandleEvent(object sender, IEnumerable<Behavior> behaviors)
        {
            foreach (var behavior in behaviors
                .Where(b => b.BehaviorType == BehaviorType.Activatable))
            {
                ActivatableEventType eventType;
                if (behavior.TryGetActivatableEventType(out eventType))
                {
                    switch (eventType)
                    {
                        case ActivatableEventType.Activated:
                            {
                                var handler = Activated;
                                if (handler != null)
                                {
                                    handler(sender, EventArgs.Empty);
                                }

                                break;
                            }

                        case ActivatableEventType.ActivationFocusChanged:
                            {
                                ActivationFocusChangedEventParams parameters;
                                if (behavior.TryGetActivationFocusChangedEventParams(out parameters))
                                {
                                    var handler = ActivationFocusChanged;
                                    if (handler != null)
                                    {
                                        var focus = GetActivationFocus(parameters);
                                        handler(sender, new ActivationFocusChangedEventArgs(focus));
                                    }
                                }

                                break;
                            }
                    }
                }
            }
        }

        private static ActivationFocus GetActivationFocus(ActivationFocusChangedEventParams parameters)
        {
            var focus = ActivationFocus.None;
            if (parameters.HasActivationFocus != EyeXBoolean.False)
            {
                focus = ActivationFocus.HasActivationFocus;
            }
            else if (parameters.HasTentativeActivationFocus != EyeXBoolean.False)
            {
                focus = ActivationFocus.HasTentativeActivationFocus;
            }

            return focus;
        }
    }

    /// <summary>
    /// Event arguments for the <see cref="ActivatableBehavior.ActivationFocusChanged"/> event.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Belongs here.")]
    public sealed class ActivationFocusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationFocusChangedEventArgs"/> class.
        /// </summary>
        /// <param name="focus">The focus type.</param>
        public ActivationFocusChangedEventArgs(ActivationFocus focus)
        {
            Focus = focus;
        }

        /// <summary>
        /// Gets the activation focus type.
        /// </summary>
        public ActivationFocus Focus { get; private set; }
    }

    /// <summary>
    /// Types of activation focus.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Belongs here.")]
    public enum ActivationFocus
    {
        /// <summary>
        /// Represents no activation focus.
        /// </summary>
        None,

        /// <summary>
        /// Represents activation focus.
        /// </summary>
        HasActivationFocus,

        /// <summary>
        /// Represents tentative activation focus.
        /// </summary>
        HasTentativeActivationFocus
    }
}
