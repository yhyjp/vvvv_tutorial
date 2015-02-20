//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework.Wpf
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using Tobii.EyeX.Framework;

    /// <summary>
    /// Partial class with events and properties related to the Gaze-aware behavior.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "More important to keep regions together.")]
    public static partial class Behavior
    {
        #region GazeAware property

        /// <summary>
        /// When set to true, the associated element becomes an EyeX interactor 
        /// with the Gaze-aware behavior: 
        /// <para>
        /// Its HasGaze property will be set to true or false depending on 
        /// whether the user's eye-gaze is within or outside the element's 
        /// bounds, and the routed event HasGazeChanged will be fired whenever 
        /// the HasGaze value is changed.
        /// </para>
        /// </summary>
        public static readonly DependencyProperty GazeAwareProperty = DependencyProperty.RegisterAttached(
            "GazeAware",
            typeof(bool),
            typeof(Behavior),
            new FrameworkPropertyMetadata(false, OnGazeAwarePropertyChanged));

        /// <summary>
        /// Sets the GazeAware behavior on a FrameworkElement. 
        /// When set to true, the associated element becomes an EyeX interactor 
        /// with the Gaze-aware behavior: 
        /// <para>
        /// Its HasGaze property will be set to true or false depending on 
        /// whether the user's eye-gaze is within or outside the element's 
        /// bounds, and the routed event HasGazeChanged will be fired whenever 
        /// the HasGaze value is changed.
        /// </para>
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to set the behavior for.</param>
        /// <param name="value">The value of the HasGaze property.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "FrameworkElement required.")]
        public static void SetGazeAware(this FrameworkElement element, bool value)
        {
            element.SetValue(GazeAwareProperty, value);
        }

        /// <summary>
        /// Gets the GazeAware property for a given FrameworkElement.
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to get the property from.</param>
        /// <returns>A value indicating whether the FrameworkElement is gaze-aware.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "FrameworkElement required.")]
        public static bool GetGazeAware(this FrameworkElement element)
        {
            return (bool)element.GetValue(GazeAwareProperty);
        }

        #endregion

        #region GazeAwareDelay property

        /// <summary>
        /// The number of milliseconds the eye-gaze of the user must have been
        /// within the bounds of the element before the element is considered
        /// to be gazed on.
        /// </summary>
        public static readonly DependencyProperty GazeAwareDelayProperty = DependencyProperty.RegisterAttached(
            "GazeAwareDelay",
            typeof(int),
            typeof(Behavior),
            new FrameworkPropertyMetadata(0, OnGazeAwareDelayPropertyChanged));

        /// <summary>
        /// Sets the value of the GazeAwareDelay property on the FrameworkElement.
        /// The delay is number of milliseconds the eye-gaze of the user must have
        /// been within the bounds of the element before the element is considered
        /// to be gazed on.
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to set the property for.</param>
        /// <param name="value">The delay value, in milliseconds.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "FrameworkElement required.")]
        public static void SetGazeAwareDelay(this FrameworkElement element, int value)
        {
            element.SetValue(GazeAwareDelayProperty, value);
        }

        /// <summary>
        /// Gets the value of the GazeAwareDelay property on the FrameworkElement.
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to set the property from.</param>
        /// <returns>The delay in milliseconds.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "FrameworkElement required.")]
        public static int GetGazeAwareDelay(this FrameworkElement element)
        {
            return (int)element.GetValue(GazeAwareDelayProperty);
        }

        #endregion

        #region HasGaze property

        /// <summary>
        /// Property that is set to true when the user's eye-gaze is within the
        /// bounds of the associated element, and set to false when the eye-gaze
        /// is outside the bounds of the element. It will not be set to true 
        /// until the gaze aware delay number of milliseconds have passed.
        /// </summary>
        public static readonly DependencyProperty HasGazeProperty = DependencyProperty.RegisterAttached(
            "HasGaze",
            typeof(bool),
            typeof(Behavior),
            new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Sets the HasGaze property for a given FrameworkElement.
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to set the property for.</param>
        /// <param name="value">A value indicating whether the user's eye-gaze is within the bounds of the associated element.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "FrameworkElement required.")]
        public static void SetHasGaze(this FrameworkElement element, bool value)
        {
            element.SetValue(HasGazeProperty, value);
        }

        /// <summary>
        /// Gets the value of the HasGaze property on this FrameworkElement.
        /// HasGaze is set to true when the user's eye-gaze is within the bounds
        /// of the associated element, and set to false when the eye-gaze is
        /// outside the bounds of the element. It will not be set to true until
        /// the gaze aware delay number of milliseconds have passed.
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to get the property from.</param>
        /// <returns>A value indicating whether the user's eye-gaze is within the bounds of the element.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "FrameworkElement required.")]
        public static bool GetHasGaze(this FrameworkElement element)
        {
            return (bool)element.GetValue(HasGazeProperty);
        }

        #endregion

        #region HasGazeChanged event

        /// <summary>
        /// Event that notifies that the HasGaze property has changed.
        /// </summary>
        public static readonly RoutedEvent HasGazeChangedEvent = EventManager.RegisterRoutedEvent(
            "HasGazeChanged",
            RoutingStrategy.Bubble, 
            typeof(RoutedEventHandler),
            typeof(Behavior));

        /// <summary>
        /// Adds a handler for the HasGazeChanged event to a given DependencyObject.
        /// </summary>
        /// <param name="d">The <see cref="DependencyObject"/> where to add the event handler.</param>
        /// <param name="handler">The <see cref="RoutedEventHandler"/> to add.</param>
        public static void AddHasGazeChangedHandler(this DependencyObject d, RoutedEventHandler handler)
        {
            var uie = d as UIElement;
            if (uie == null) { return; }

            uie.AddHandler(HasGazeChangedEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the HasGazeChanged event from a given DependencyObject.
        /// </summary>
        /// <param name="d">The <see cref="DependencyObject"/> where to remove the event handler.</param>
        /// <param name="handler">The <see cref="RoutedEventHandler"/> to remove.</param>
        public static void RemoveHasGazeChangedHandler(this DependencyObject d, RoutedEventHandler handler)
        {
            var uie = d as UIElement;
            if (uie == null) { return; }

            uie.RemoveHandler(HasGazeChangedEvent, handler);
        }

        #endregion

        #region Property changed handling

        private static void OnGazeAwarePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as FrameworkElement;
            if (null == element) { return; }

            element.Unloaded += OnGazeAwareElementUnloaded;
            OnGazeAwareBehaviorChanged(element, (bool)e.NewValue);
        }

        private static void OnGazeAwareDelayPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as FrameworkElement;
            if (null == element) { return; }

            OnGazeAwareBehaviorChanged(element, element.GetGazeAware());
        }

        private static void OnGazeAwareBehaviorChanged(FrameworkElement element, bool isGazeAware)
        {
            if (isGazeAware)
            {
                var gazeAwareBehavior = new GazeAwareBehavior(OnHasGazeChanged) { DelayMilliseconds = element.GetGazeAwareDelay() };
                AddBehavior(element, gazeAwareBehavior);
            }
            else
            {
                RemoveBehavior(element, BehaviorType.GazeAware);
            }
        }

        private static void OnHasGazeChanged(object sender, GazeAwareEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (null == element) { return; }

            element.SetHasGaze(e.HasGaze);
            element.RaiseEvent(new RoutedEventArgs(Behavior.HasGazeChangedEvent, element));
        }

        private static void OnGazeAwareElementUnloaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (null == element) { return; }

            element.Unloaded -= OnGazeAwareElementUnloaded;
            RemoveBehavior(element, BehaviorType.GazeAware);
        }

        #endregion
    }
}
