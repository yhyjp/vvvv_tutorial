//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Forms;
    using Tobii.EyeX.Client;
    using Tobii.EyeX.Framework;

    /// <summary>
    /// Represents an EyeX interactor. 
    /// Used by <see cref="BehaviorMap"/>.
    /// </summary>
    public class FormsInteractor
    {
        private readonly List<IEyeXBehavior> _behaviors = new List<IEyeXBehavior>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FormsInteractor"/> class.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to represent.</param>
        public FormsInteractor(Control control)
        {
            Control = control;
            Id = InteractorIDGenerator.CreateUniqueID();
        }

        /// <summary>
        /// Gets the <see cref="Control"/> that the interactor represents.
        /// </summary>
        public Control Control { get; private set; }

        /// <summary>
        /// Gets the unique id for the interactor.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets or sets the id of the parent interactor.
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// Gets or sets the Z value.
        /// </summary>
        public int Z { get; set; }

        /// <summary>
        /// Gets or sets the window id.
        /// </summary>
        public string WindowId { get; set; }

        /// <summary>
        /// Returns true if the interactor intersects the given query rectangle.
        /// </summary>
        /// <param name="queryRect">The query rectangle.</param>
        /// <returns>A value indicating whether the interactor intersects the given query rectangle.</returns>
        /// <note>You need to call this method from the UI thread.</note>
        public bool IntersectsWith(Rectangle queryRect)
        {
            var screenTopLeft = Control.PointToScreen(Point.Empty);
            if (queryRect.Right > screenTopLeft.X &&
                queryRect.Bottom > screenTopLeft.Y)
            {
                var screenBottomRight = Control.PointToScreen(new Point(Control.Size.Width, Control.Size.Height));
                if (queryRect.Left < screenBottomRight.X &&
                    queryRect.Top < screenBottomRight.Y)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds a behavior to the interactor.
        /// </summary>
        /// <param name="behavior">The behavior to add.</param>
        public void AddBehavior(IEyeXBehavior behavior)
        {
            if (_behaviors.Any(x => x.BehaviorType == behavior.BehaviorType))
            {
                throw new InvalidOperationException("A behavior with the same type has already been added.");
            }

            _behaviors.Add(behavior);
        }

        /// <summary>
        /// Handles an event from the EyeX Engine. 
        /// </summary>
        /// <param name="event_">The event.</param>
        /// <returns>True if the event could be handled, false if incorrect interactor id.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Event is a reserved word.")]
        public bool HandleEvent(InteractionEvent event_)
        {
            if (string.Equals(Id, event_.InteractorId))
            {
                var eventBehaviors = event_.Behaviors;

                foreach (var behavior in _behaviors)
                {
                    behavior.HandleEvent(Control, eventBehaviors);
                }

                foreach (var eventBehavior in eventBehaviors)
                {
                    eventBehavior.Dispose();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Adds the interactor to a given snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot to add the interactor to.</param>
        public void AddToSnapshot(Snapshot snapshot)
        {
            using (var interactor = snapshot.CreateInteractor(Id, ParentId, WindowId))
            {
                using (var bounds = interactor.CreateBounds(BoundsType.Rectangular))
                {
                    var screenTopLeft = Control.PointToScreen(Point.Empty);
                    var screenBottomRight = Control.PointToScreen(new Point(Control.Size.Width, Control.Size.Height));
                    bounds.SetRectangularData(screenTopLeft.X, screenTopLeft.Y, screenBottomRight.X - screenTopLeft.X, screenBottomRight.Y - screenTopLeft.Y);
                }

                interactor.Z = Z;

                foreach (var behavior in _behaviors)
                {
                    behavior.AssignBehavior(interactor);
                }
            }
        }

        /// <summary>
        /// Returns a string that represents the interactor.
        /// </summary>
        /// <returns>A string that represents the interactor.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} '{1}' P:{2} [{3}]", Id, Control.Name, ParentId, _behaviors.Count);
        }
    }
}
