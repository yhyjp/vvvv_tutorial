//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework.Wpf
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// Helper class for traversing and hit testing visual trees in Windows 
    /// Presentation Foundation (WPF).
    /// </summary>
    public static class WpfCrawler
    {
        /// <summary>
        /// Gets the set of interactors within a given query rectangle.
        /// <para>
        /// In interactor is defined as a FrameworkElement with an Behavior attached property set on it.
        /// </para>
        /// </summary>
        /// <param name="window">A top level window</param>
        /// <param name="queryRectangle">Query rectangle in screen coordinates</param>
        /// <returns>The set of interactors.</returns>
        public static IEnumerable<WpfInteractor> GetInteractors(Visual window, Rect queryRectangle)
        {
            var intersectingInteractors = new List<WpfInteractor>();
            var hitTestRectangle = new RectangleGeometry(GetBoundsInLocalCoordinates(window, queryRectangle));

            VisualTreeHelper.HitTest(
                window,
                potentialHitTestTarget =>
                {
                    var frameworkElement = potentialHitTestTarget as FrameworkElement;
                    if (frameworkElement == null)
                    {
                        return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
                    }

                    if (frameworkElement.Visibility != Visibility.Visible)
                    {
                        return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
                    }

                    if (!frameworkElement.IsHitTestVisible)
                    {
                        return HitTestFilterBehavior.ContinueSkipSelf;
                    }

                    if (frameworkElement.IsInteractor())
                    {
                        intersectingInteractors.Add(frameworkElement.GetWpfInteractor());
                    }

                    return HitTestFilterBehavior.Continue;
                },
                result => HitTestResultBehavior.Continue,
                new GeometryHitTestParameters(hitTestRectangle));

            return intersectingInteractors;
        }

        /// <summary>
        /// Updates the ParentId and Z properties of all interactors in the visual tree rooted
        /// at the given topElement.
        /// </summary>
        /// <param name="topElement">The root element of the currently traversed visual tree.</param>
        /// <param name="topId">Id of the interactor parent.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "GetWpfInteractor requires FrameworkElement.")]
        public static void UpdateInteractorProperties(FrameworkElement topElement, string topId)
        {
            int nextChildZ = 0;
            foreach (var childElement in GetChildInteractorElements(topElement))
            {
                var wpfInteractor = childElement.GetWpfInteractor();
                wpfInteractor.ParentId = topId;
                wpfInteractor.Z = nextChildZ;
                nextChildZ++;

                UpdateInteractorProperties(childElement, wpfInteractor.Id);
            }
        }

        /// <summary>
        /// Returns all children of the given framework element that are also interactors.
        /// </summary>
        /// <param name="parent">The framework element to get children of.</param>
        /// <returns>The child elements.</returns>
        private static IEnumerable<FrameworkElement> GetChildInteractorElements(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var childAsFrameworkElement = child as FrameworkElement;

                if (null != childAsFrameworkElement && childAsFrameworkElement.IsInteractor())
                {
                    yield return childAsFrameworkElement;
                }
                else
                {
                    foreach (var grandchild in GetChildInteractorElements(child))
                    {
                        yield return grandchild;
                    }
                }
            }
        }

        private static Rect GetBoundsInLocalCoordinates(Visual visual, Rect boundsInScreenCoordinates)
        {
            var localBoundsUpperLeft = visual.PointFromScreen(new Point(boundsInScreenCoordinates.X, boundsInScreenCoordinates.Y));
            var localBoundsLowerRight = visual.PointFromScreen(new Point(
                                                            boundsInScreenCoordinates.X + boundsInScreenCoordinates.Width,
                                                            boundsInScreenCoordinates.Y + boundsInScreenCoordinates.Height));
            return new Rect(localBoundsUpperLeft, localBoundsLowerRight);
        }
    }
}
