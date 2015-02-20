//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework.Wpf
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using Tobii.EyeX.Client;
    using Tobii.EyeX.Framework;

    /// <summary>
    /// Partial class with events and properties related to occluders (interactors without behavior).
    /// </summary>
    public static partial class Behavior
    {
        #region IsOccluder property

        /// <summary>
        /// When set to true, the associated element becomes an EyeX interactor
        /// with no behavior.
        /// <para>
        /// An EyeX interactor with no behavior is the definition of an occluder 
        /// in the eyes of the EyeX Engine. An occluder will block eye-gaze
        /// interaction from reaching parts of the screen that are behind it.
        /// </para><para>
        /// For the occluder to work correctly, its branch in the interactor tree
        /// has to have a higher Z value than the branch of interactors it is
        /// suppose to occlude.
        /// </para><para>
        /// If a visual element is drawn on top of another visual element, but
        /// is not explicitly declared to be an occluder, it will be treated as
        /// if it were transparent by the EyeX Engine.
        /// </para>
        /// </summary>
        public static readonly DependencyProperty IsOccluderProperty = DependencyProperty.RegisterAttached(
           "IsOccluder",
           typeof(bool),
           typeof(Behavior),
           new FrameworkPropertyMetadata(false, OnIsOccluderPropertyChanged));

        /// <summary>
        /// Sets the IsOccluder property on a FrameworkElement.
        /// <para>
        /// When set to true, the associated element becomes an EyeX interactor
        /// with no behavior.
        /// </para><para>
        /// An EyeX interactor with no behavior is the definition of an occluder 
        /// in the eyes of the EyeX Engine. An occluder will block eye-gaze
        /// interaction from reaching parts of the screen that are behind it.
        /// </para><para>
        /// For the occluder to work correctly, its branch in the interactor tree
        /// has to have a higher Z value than the branch of interactors it is
        /// suppose to occlude.
        /// </para><para>
        /// If a visual element is drawn on top of another visual element, but
        /// is not explicitly declared to be an occluder, it will be treated as
        /// if it were transparent by the EyeX Engine.
        /// </para>
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to set the IsOccluder property for.</param>
        /// <param name="value">The value of the IsOccluder property.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "FrameworkElement required.")]
        public static void SetIsOccluder(this FrameworkElement element, bool value)
        {
            element.SetValue(IsOccluderProperty, value);
        }

        /// <summary>
        /// Gets the IsOccluder property of a FrameworkElement.
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to get the property from.</param>
        /// <returns>A value indicating whether the FrameworkElement is an occluder or not.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "FrameworkElement required.")]
        public static bool GetIsOccluder(this FrameworkElement element)
        {
            return (bool)element.GetValue(IsOccluderProperty);
        }

        #endregion

        #region Property changed handling

        private static void OnIsOccluderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as FrameworkElement;
            if (null == element) { return; }

            // Make sure there is a unique id set on the element
            if (string.IsNullOrEmpty(element.Uid))
            {
                element.Uid = InteractorIDGenerator.CreateUniqueID();
            }

            element.Unloaded += OnOccluderElementUnloaded;

            var occluderBehavior = new OccluderBehavior();
            var wpfInteractor = element.GetWpfInteractorOrDefault();
            wpfInteractor.AddBehavior(element, occluderBehavior);
        }

        private static void OnOccluderElementUnloaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (null == element) { return; }

            element.Unloaded -= OnOccluderElementUnloaded;

            var wpfInteractor = element.GetWpfInteractorOrDefault();
            wpfInteractor.RemoveBehavior((BehaviorType)0);
        }

        #endregion
    }

    /// <summary>
    /// Maps a dummy behavior to an interactor.
    /// </summary>
    /// <remarks>
    /// The OccluderBehavior class is only needed for the WPF framework and is
    /// therefore implemented here.
    /// </remarks>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Belongs here.")]
    public class OccluderBehavior : IEyeXBehavior
    {
        /// <summary>
        /// Gets the behavior type, which in this case is "no behavior".
        /// </summary>
        public BehaviorType BehaviorType
        {
            get { return 0; }
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="interactor">The interactor to do nothing with.</param>
        public void AssignBehavior(Interactor interactor)
        {
            // do nothing
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="sender">The sender to do nothing with.</param>
        /// <param name="behaviors">The <see cref="Behavior"/> instances containing the event data.</param>
        public void HandleEvent(object sender, IEnumerable<Tobii.EyeX.Client.Behavior> behaviors)
        {
            // do nothing
        }
    }
}
