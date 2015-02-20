//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework.Forms
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using Tobii.EyeX.Client;

    /// <summary>
    /// Component holding the interactors and behaviors used by a Form or Control.
    /// Must be connected to a <see cref="FormsEyeXHost"/> to receive queries and events.
    /// </summary>
    public sealed class BehaviorMap : IComponent
    {
        private readonly List<FormsInteractor> _interactors = new List<FormsInteractor>();
        private EventHandler _disposedEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorMap"/> class.
        /// </summary>
        /// <param name="container">An IContainer that represents the container for the behavior map component.</param>
        public BehaviorMap(IContainer container)
        {
            container.Add(this);
        }

        #region Explicit implementation of the IComponent interface

        event EventHandler IComponent.Disposed
        {
            add { _disposedEvent += value; }
            remove { _disposedEvent -= value; }
        }

        ISite IComponent.Site { get; set; }

        #endregion

        /// <summary>
        /// Gets a value indicating whether any interactors have been added or removed. Used for caching.
        /// </summary>
        [Browsable(false)]
        public bool IsModified { get; private set; }

        /// <summary>
        /// Gets all interactors in the collection.
        /// </summary>
        /// <returns>Enumeration of interactors.</returns>
        [Browsable(false)]
        public IEnumerable<FormsInteractor> Interactors
        {
            get
            {
                return _interactors;
            }
        }

        /// <summary>
        /// Makes the specified control eye-gaze interactive, by assigning an EyeX behavior 
        /// to the interactor associated with the control.
        /// Creates a new interactor for the control if necessary.
        /// </summary>
        /// <param name="control">Control to be made eye-gaze interactive.</param>
        /// <param name="behavior">EyeX behavior to be assigned.</param>
        public void Add(Control control, IEyeXBehavior behavior)
        {
            GetInteractor(control).AddBehavior(behavior);
        }

        /// <summary>
        /// Makes the specified control eye-gaze interactive, but without any EyeX behaviors. 
        /// Use this method to make the control block (occlude) eye-gaze interaction with other 
        /// controls behind it.
        /// </summary>
        /// <param name="control">Control to be marked as an eye-gaze occluder.</param>
        public void AddOccluder(Control control)
        {
            // make sure that there is an interactor associated with the control. No behaviors.
            GetInteractor(control);
        }

        /// <summary>
        /// Gets the interactor associated with a given control. Creates a new interactor if none was found.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns>The interactor.</returns>
        public FormsInteractor GetInteractor(Control control)
        {
            FormsInteractor interactor;
            if (!TryGetInteractor(control, out interactor))
            {
                interactor = new FormsInteractor(control);
                _interactors.Add(interactor);
                IsModified = true;
            }

            return interactor;
        }

        /// <summary>
        /// Gets the interactor associated with a given control, if there is one.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="interactor">The interactor.</param>
        /// <returns>True if an interactor exists.</returns>
        public bool TryGetInteractor(Control control, out FormsInteractor interactor)
        {
            interactor = _interactors
                .FirstOrDefault(x => control.Equals(x.Control));

            return interactor != null;
        }

        /// <summary>
        /// Gets all interactors in the collection that intersect with a given rectangle.
        /// </summary>
        /// <param name="queryRect">The rectangle, in screen coordinates.</param>
        /// <returns>Enumeration of interactors.</returns>
        public IEnumerable<FormsInteractor> GetIntersectingInteractors(Rectangle queryRect)
        {
            return _interactors
                .Where(interactor => interactor.IntersectsWith(queryRect));
        }

        /// <summary>
        /// Handles an event from the EyeX Engine.
        /// </summary>
        /// <param name="event_">Event to be handled.</param>
        /// <returns>True if the event was handled, false otherwise.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Event is a reserved word.")]
        public bool HandleEvent(InteractionEvent event_)
        {
            foreach (var interactor in _interactors)
            {
                if (interactor.HandleEvent(event_))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Clears the IsModified flag.
        /// </summary>
        public void MarkAsUnmodified()
        {
            IsModified = false;
        }

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public void Dispose()
        {
            _interactors.Clear();

            var handler = _disposedEvent;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
