//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework.Wpf
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using Tobii.EyeX.Framework;

    /// <summary>
    /// Partial class with events and properties related to the Activatable behavior.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "More important to keep regions together.")]
    public static partial class Behavior
    {
        #region Activatable property

        /// <summary>
        /// When set to a value other than None, the associated element becomes 
        /// an EyeX interactor with the Activatable behavior. 
        /// <para>
        /// -The routed event Activate will be fired when the user interacts 
        /// with the element by looking at it and pressing the EyeX Direct Click
        /// key or EyeX Button (typically the Right Ctrl key, but this can be 
        /// changed in the EyeX Interaction settings).
        /// </para><para>
        /// -The property ActivationFocus will be set to a value corresponding 
        /// to the current activation focus state of the element, and the routed 
        /// event ActivationFocusChanged will be fired whenever this value is 
        /// changed.
        /// </para>
        /// </summary>
        public static readonly DependencyProperty ActivatableProperty = DependencyProperty.RegisterAttached(
           "Activatable",
           typeof(ActivatableType),
           typeof(Behavior),
           new FrameworkPropertyMetadata(ActivatableType.None, OnActivatablePropertyChanged));

        /// <summary>
        /// Sets the value of the Activatable property on a given FrameworkElement.
        /// <para>
        /// When set to a value other than ActivatableType.None, the associated 
        /// element becomes an EyeX interactor with the Activatable behavior. 
        /// </para><para>
        /// -The routed event Activate will be fired when the user interacts 
        /// with the element by looking at it and pressing the EyeX Direct Click
        /// key (typically the Right Ctrl key, but this can be changed in the 
        /// EyeX Interaction settings).
        /// </para><para>
        /// -The property ActivationFocus will be set to a value corresponding 
        /// to the current activation focus state of the element, and the routed 
        /// event ActivationFocusChanged will be fired whenever this value is 
        /// changed.
        /// </para>
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to set the property for.</param>
        /// <param name="value">The <see cref="ActivatableType"/> value of the property.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "FrameworkElement required.")]
        public static void SetActivatable(this FrameworkElement element, ActivatableType value)
        {
            element.SetValue(ActivatableProperty, value);
        }

        /// <summary>
        /// Gets the value of the Activatable property on a given FrameworkElement.
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to get the property value from.</param>
        /// <returns>The value of the Activatable property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "FrameworkElement required.")]
        public static ActivatableType GetActivatable(this FrameworkElement element)
        {
            return (ActivatableType)element.GetValue(ActivatableProperty);
        }

        #endregion

        #region ActivationFocus property

        /// <summary>
        /// The element can be in one of three activation focus states: 
        /// <para>
        /// -ActivationFocus.None - the element does not have any kind of 
        /// activation focus. 
        /// </para><para>
        /// -ActivationFocus.HasActivationFocus - the user is pressing down 
        /// the activation key, and this element would be activated if the 
        /// user were to release the activation key at this instant. 
        /// </para><para>
        /// -ActivationFocus.HasTentativeActivationFocus - the user is just 
        /// looking around, but this element would be activated if the user 
        /// were to press and release the activation key at this instant.
        /// </para>
        /// </summary>
        public static readonly DependencyProperty ActivationFocusProperty = DependencyProperty.RegisterAttached(
            "ActivationFocus",
            typeof(ActivationFocus),
            typeof(Behavior),
            new FrameworkPropertyMetadata(ActivationFocus.None));

        /// <summary>
        /// Sets the ActivationFocus property.
        /// <para>
        /// Should only be set as part of the event handling of activation
        /// focus changed events from the EyeX Engine.
        /// </para>
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to set the property value for.</param>
        /// <param name="value">The <see cref="ActivationFocus"/> value of the property.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "FrameworkElement required.")]
        public static void SetActivationFocus(this FrameworkElement element, ActivationFocus value)
        {
            element.SetValue(ActivationFocusProperty, value);
        }

        /// <summary>
        /// Gets the value of the ActivationFocus property on a FrameworkElement.
        /// <para>
        /// The element can be in one of three activation focus states: 
        /// </para><para>
        /// -ActivationFocus.None - the element does not have any kind of 
        /// activation focus. 
        /// </para><para>
        /// -ActivationFocus.HasActivationFocus - the user is pressing down 
        /// the activation key, and this element would be activated if the 
        /// user were to release the activation key at this instant. 
        /// </para><para>
        /// -ActivationFocus.HasTentativeActivationFocus - the user is just 
        /// looking around, but this element would be activated if the user 
        /// were to press and release the activation key at this instant.
        /// </para>
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/></param>
        /// <returns>The the value of the ActivationFocus property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "FrameworkElement required.")]
        public static ActivationFocus GetActivationFocus(this FrameworkElement element)
        {
            return (ActivationFocus)element.GetValue(ActivationFocusProperty);
        }

        #endregion

        #region Activate event

        /// <summary>
        /// Event that notifies that the associated element has been activated 
        /// by the user. That is, the user has looked at it while releasing the 
        /// activation (or EyeX Direct Click) key.
        /// </summary>
        public static readonly RoutedEvent ActivateEvent = EventManager.RegisterRoutedEvent(
            "Activate",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(Behavior));

        /// <summary>
        /// Adds a handler for the Activate event to a given DependencyObject.
        /// </summary>
        /// <param name="d">The <see cref="DependencyObject"/> where to add the event handler.</param>
        /// <param name="handler">The <see cref="RoutedEventHandler"/> to add.</param>
        public static void AddActivateHandler(this DependencyObject d, RoutedEventHandler handler)
        {
            var uie = d as UIElement;
            if (uie == null) { return; }

            uie.AddHandler(ActivateEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the Activate event from a given DependencyObject.
        /// </summary>
        /// <param name="d">The <see cref="DependencyObject"/> where to remove the event handler.</param>
        /// <param name="handler">The <see cref="RoutedEventHandler"/> to remove.</param>
        public static void RemoveActivateHandler(this DependencyObject d, RoutedEventHandler handler)
        {
            var uie = d as UIElement;
            if (uie == null) { return; }

            uie.RemoveHandler(ActivateEvent, handler);
        }

        #endregion

        #region ActivationFocusChanged event

        /// <summary>
        /// Event that notifies that the ActivationFocus property has changed.
        /// </summary>
        public static readonly RoutedEvent ActivationFocusChangedEvent = EventManager.RegisterRoutedEvent(
            "ActivationFocusChanged",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(Behavior));

        /// <summary>
        /// Adds a handler for the ActivationFocusChanged event to a given DependencyObject.
        /// </summary>
        /// <param name="d">The <see cref="DependencyObject"/> where to add the event handler.</param>
        /// <param name="handler">The <see cref="RoutedEventHandler"/> to add.</param>
        public static void AddActivationFocusChangedHandler(this DependencyObject d, RoutedEventHandler handler)
        {
            var uie = d as UIElement;
            if (uie == null) { return; }

            uie.AddHandler(ActivationFocusChangedEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the ActivationFocusChanged event from a given DependencyObject.
        /// </summary>
        /// <param name="d">The <see cref="DependencyObject"/> where to remove the event handler.</param>
        /// <param name="handler">The <see cref="RoutedEventHandler"/> to remove.</param>
        public static void RemoveActivationFocusChangedHandler(this DependencyObject d, RoutedEventHandler handler)
        {
            var uie = d as UIElement;
            if (uie == null) { return; }

            uie.RemoveHandler(ActivationFocusChangedEvent, handler);
        }

        #endregion

        #region ActivatableProperty changed handling

        private static void OnActivatablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as FrameworkElement;
            if (null == element) { return; }

            element.Unloaded += OnActivatableElementUnloaded;

            var newPropertyValue = (ActivatableType)e.NewValue;

            switch (newPropertyValue)
            {
                case ActivatableType.Default:
                    AddBehavior(element, CreateActivatableBehavior(false));
                    break;
                case ActivatableType.TentativeFocusEnabled:
                    AddBehavior(element, CreateActivatableBehavior(true));
                    break;
                case ActivatableType.None:
                    RemoveBehavior(element, BehaviorType.Activatable);
                    break;
            }
        }

        private static IEyeXBehavior CreateActivatableBehavior(bool enableTentativeFocus)
        {
            return new ActivatableBehavior(OnActivated, OnActivationFocusChanged)
                {
                    IsTentativeFocusEnabled = enableTentativeFocus
                };
        }

        /// <summary>
        /// Callback method called when an activation event has been received 
        /// from the EyeX Engine for the interactor associated with this 
        /// FrameworkElement.
        /// </summary>
        /// <param name="sender">The <see cref="FrameworkElement"/> that got the event.</param>
        /// <param name="e">The event arguments (not used for this event)</param>
        private static void OnActivated(object sender, EventArgs e)
        {
            var element = sender as FrameworkElement;
            if (null == element) { return; }

            element.RaiseEvent(new RoutedEventArgs(ActivateEvent, element));
        }

        /// <summary>
        /// Callback method called when an activation focus changed event has 
        /// been received from the EyeX Engine for the interactor associated 
        /// with this FrameworkElement.
        /// </summary>
        /// <param name="sender">The <see cref="FrameworkElement"/> that got the event.</param>
        /// <param name="e">The <see cref="ActivationFocusChangedEventArgs"/> for the event.</param>
        private static void OnActivationFocusChanged(object sender, ActivationFocusChangedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (null == element) { return; }

            element.SetActivationFocus(e.Focus);
            element.RaiseEvent(new RoutedEventArgs(ActivationFocusChangedEvent, element));
        }

        private static void OnActivatableElementUnloaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (null == element) { return; }

            element.Unloaded -= OnActivatableElementUnloaded;
            RemoveBehavior(element, BehaviorType.Activatable);
        }

        #endregion
    }

    /// <summary>
    /// Used to specify the type of Activatable events that the EyeX Engine 
    /// should send for the Activatable interactor. The element is only 
    /// considered an interactor for a value other than None.
    /// </summary>
    public enum ActivatableType
    {
        /// <summary>
        /// No interactor.
        /// </summary>
        None,

        /// <summary>
        /// Activatable interactor with tentative focus disabled.
        /// </summary>
        Default,

        /// <summary>
        /// Activatable interactor with tentative focus enabled.
        /// </summary>
        TentativeFocusEnabled
    }
}
