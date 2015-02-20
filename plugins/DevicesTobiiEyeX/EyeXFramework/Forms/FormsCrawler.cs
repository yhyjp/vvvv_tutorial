//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using Tobii.EyeX.Framework;

    /// <summary>
    /// Helper class for traversing Windows Forms control trees.
    /// </summary>
    public static class FormsCrawler
    {
        /// <summary>
        /// Updates the ParentId, Z, and WindowId properties of all interactors.
        /// </summary>
        /// <param name="behaviorMaps">Behavior maps providing the interactors.</param>
        public static void UpdateInteractorProperties(IEnumerable<BehaviorMap> behaviorMaps)
        {
            foreach (var behaviorsByForm in behaviorMaps
                .ToLookup<BehaviorMap, Form>(map => GetForm(map)))
            {
                var form = behaviorsByForm.Key;
                if (form == null)
                {
                    continue;
                }

                var windowId = form.Handle.ToString();

                try
                {
                    var interactorDictionary = behaviorsByForm
                        .SelectMany(map => map.Interactors)
                        .ToDictionary(x => x.Control);

                    int z = 0;
                    UpdateInteractorProperties(behaviorsByForm.Key, Literals.RootId, ref z, windowId, interactorDictionary);
                }
                catch (ArgumentException) // thrown by the ToDictionary operation
                {
                    throw new InvalidOperationException("A control is referenced by more than one interactor.");
                }
            }
        }

        /// <summary>
        /// Gets the set of interactors within a given query rectangle.
        /// </summary>
        /// <param name="behaviorMaps">Behavior maps providing the interactors.</param>
        /// <param name="queryRect">Query rectangle in screen coordinates.</param>
        /// <param name="queryWindowIds">Window ID's from the query.</param>
        /// <returns>The interactors.</returns>
        public static IEnumerable<FormsInteractor> GetInteractors(IEnumerable<BehaviorMap> behaviorMaps, Rectangle queryRect, IEnumerable<string> queryWindowIds)
        {
            var windowIdArray = queryWindowIds.ToArray(); // avoid multiple traversals of the enumeration

            foreach (var behaviorsByForm in behaviorMaps
                .ToLookup<BehaviorMap, Form>(map => GetForm(map)))
            {
                var form = behaviorsByForm.Key;
                if (form == null)
                {
                    continue;
                }

                var windowId = form.Handle.ToString();
                if (!windowIdArray.Contains(windowId))
                {
                    continue;
                }

                // Find all interactors intersecting the query rectangle.
                // Since controls cannot extend outside the bounds of their parent controls/forms 
                // in Windows Forms, we know that this operation will find all parent interactors 
                // as well.
                foreach (var interactor in behaviorsByForm
                    .SelectMany(map => map.GetIntersectingInteractors(queryRect)))
                {
                    yield return interactor;
                }
            }
        }

        private static Form GetForm(BehaviorMap behaviorMap)
        {
            return behaviorMap.Interactors
                .Select(interactor => interactor.Control.FindForm())
                .Distinct()
                .SingleOrDefault();
        }

        private static void UpdateInteractorProperties(Control control, string parentId, ref int z, string windowId, IDictionary<Control, FormsInteractor> interactors)
        {
            FormsInteractor interactor;
            if (interactors.TryGetValue(control, out interactor))
            {
                interactor.ParentId = parentId;
                interactor.Z = z++;
                interactor.WindowId = windowId;

                foreach (var childControl in control.Controls
                    .Cast<Control>()
                    .Reverse()) // reverse, because the Controls collection is ordered front-to-back, whereas z order goes back-to-front.
                {
                    int childZ = 0;
                    UpdateInteractorProperties(childControl, interactor.Id, ref childZ, windowId, interactors);
                }
            }
            else
            {
                foreach (var childControl in control.Controls
                    .Cast<Control>()
                    .Reverse())
                {
                    UpdateInteractorProperties(childControl, parentId, ref z, windowId, interactors);
                }
            }
        }
    }
}
