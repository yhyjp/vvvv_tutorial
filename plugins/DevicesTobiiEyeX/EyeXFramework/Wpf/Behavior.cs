//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework.Wpf
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using Tobii.EyeX.Framework;

    /// <summary>
    /// Class that contains all the EyeX Behavior related attached
    /// properties and events.
    /// <para>
    /// The class implementation is split into one file per EyeX 
    /// Behavior, with the Behavior.cs file containing generic
    /// implementation.
    /// </para>
    /// <para>
    /// File naming convention: Behavior.Xyz.cs contains all
    /// implementation related explicitly to the EyeX Xyz behavior.
    /// </para>
    /// </summary>
    public static partial class Behavior
    {
        /// <summary>
        /// Indicates whether the FrameworkElement represents an EyeX 
        /// interactor or not. That is, if it has any EyeX Behaviors
        /// set on it. 
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/></param>
        /// <returns>
        /// A value indicating whether the FrameworkElement represents 
        /// an EyeX interactor.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Reviewed.")]
        public static bool IsInteractor(this FrameworkElement element)
        {
            var wpfInteractor = element.GetWpfInteractor();
            return (null != wpfInteractor) && wpfInteractor.IsInteractor;
        }

        #region WpfInteractor property

        /// <summary>
        /// Holds the WpfInteractor object associated with a FrameworkElement. 
        /// <para>
        /// The WpfInteractor class stores information about EyeX behaviors
        /// set on the element, and other information needed to create an EyeX
        /// interactor based on the element, and also delegates handling of
        /// events from the EyeX Engine to the attached EyeX behaviors.
        /// </para>
        /// </summary>
        public static readonly DependencyProperty WpfInteractorProperty = DependencyProperty.RegisterAttached(
            "WpfInteractor",
            typeof(WpfInteractor),
            typeof(Behavior),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Sets the WpfInteractor property for a given FrameworkElement.
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to set the property for.</param>
        /// <param name="value">The <see cref="WpfInteractor"/> value of the property.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "FrameworkElement required.")]
        public static void SetWpfInteractor(this FrameworkElement element, WpfInteractor value)
        {
            element.SetValue(WpfInteractorProperty, value);
        }

        /// <summary>
        /// Gets the WpfInteractor of the FrameworkElement. 
        /// <para>
        /// This method returns null for a FrameworkElement with no set EyeX 
        /// Behaviors. Code that need a reference to the WpfInteractor, to be 
        /// able to add an EyeX Behavior to it, should use the 
        /// GetWpfInteractorOrDefault method instead.
        /// </para>
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to get the interactor from.</param>
        /// <returns>The <see cref="WpfInteractor"/> attached, or null.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "FrameworkElement required.")]
        public static WpfInteractor GetWpfInteractor(this FrameworkElement element)
        {
            return (WpfInteractor)element.GetValue(WpfInteractorProperty);
        }

        /// <summary>
        /// Gets the WpfInteractor for the FrameworkElement, or if the
        /// WpfInteractor is null, creates, sets and returns an empty
        /// WpfInteractor. 
        /// <para>
        /// This method should be used for code that need a reference to the 
        /// WpfInteractor to add an EyeX Behavior to it.
        /// </para>
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to get the object from.</param>
        /// <returns>The WpfInteractor for the FrameworkElement, or a default instance.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "FrameworkElement required.")]
        public static WpfInteractor GetWpfInteractorOrDefault(this FrameworkElement element)
        {
            var wpfInteractor = element.GetWpfInteractor();
            if (null == wpfInteractor)
            {
                wpfInteractor = WpfInteractor.CreateEmpty();
                element.SetWpfInteractor(wpfInteractor);
            }

            return wpfInteractor;
        }

        #endregion

        #region Utility methods for adding and removing behaviors to/from a WpfInteractor

        private static void AddBehavior(FrameworkElement element, IEyeXBehavior behavior)
        {
            var wpfInteractor = element.GetWpfInteractorOrDefault();
            wpfInteractor.AddBehavior(element, behavior);
        }

        private static void RemoveBehavior(FrameworkElement element, BehaviorType behaviorType)
        {
            var wpfInteractor = element.GetWpfInteractorOrDefault();
            wpfInteractor.RemoveBehavior(behaviorType);
        }

        #endregion
    }
}
