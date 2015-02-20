//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework.Wpf
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using System.Windows.Interop;
    using Tobii.EyeX.Client;
    using Tobii.EyeX.Framework;
    using Rect = System.Windows.Rect;

    /// <summary>
    /// A WpfInteractor is the WPF representation of an EyeX interactor, based
    /// on a FrameworkElement. 
    /// <para>
    /// An EyeX interactor covers a certain region of the screen, and is part
    /// of a logical tree of interactors, with z ordering between siblings, 
    /// and a parent being either the window it belongs to (defined as the root) 
    /// or another interactor.
    /// </para><para>
    /// In this sense, EyeX interactors corresponds very well to FrameworkElements
    /// in the WPF framework. FrameworkElements have an actual width and actual
    /// height and is related to other FrameworkElements by their placement in the
    /// visual tree. The WpfInteractor tree is made up of the subset of elements
    /// in the visual tree that have an WpfInteractor object with at least one
    /// added EyeX Behavior.
    /// </para>
    /// </summary>
    public class WpfInteractor
    {
        private readonly string _id;
        private readonly List<IEyeXBehavior> _behaviors = new List<IEyeXBehavior>();

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfInteractor"/> class.
        /// </summary>
        /// <param name="id">An application-wide unique identifier for this WpfInteractor.</param>
        public WpfInteractor(string id)
        {
            _id = id;
        }

        /// <summary>
        /// Occurs when the first EyeX Behavior have been added to the WpfInteractor, 
        /// so it should now be treated as an interactor in the eyes of the EyeX 
        /// Engine.
        /// </summary>
        public static event EventHandler<ElementEventArgs> InteractorAdded;

        /// <summary>
        /// Occurs when the last EyeX Behavior is removed from this WpfInteractor, 
        /// so it should no longer be treated as an interactor in the eyes of the 
        /// EyeX Engine.
        /// </summary>
        public static event EventHandler<ElementEventArgs> InteractorRemoved;

        /// <summary>
        /// Gets a value indicating whether there is at least one EyeX Behavior attached 
        /// to this WpfInteractor.
        /// </summary>
        public bool IsInteractor
        {
            get { return Element != null && _behaviors.Count > 0; }
        }

        /// <summary>
        /// Gets a value indicating whether the WpfInteractor is in a state so that 
        /// it can be used to create a valid EyeX interactor to be sent to the EyeX Engine.
        /// </summary>
        public bool IsQueryable
        {
            get
            {
                return IsInteractor &&
                       HasValidWindowId &&
                       !string.IsNullOrEmpty(ParentId);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the WindowId is set to a valid window 
        /// handle. Typically does not become true until the associated 
        /// FrameworkElement has been loaded.
        /// </summary>
        public bool HasValidWindowId
        {
            get { return IntPtr.Zero != WindowId; }
        }

        /// <summary>
        /// Gets an application-wide unique identifier for this WpfInteractor. This
        /// id is used by the EyeX Engine to identify interactors, and is the
        /// id set on EyeX events to specify which interactor the event is
        /// raised for.
        /// </summary>
        public string Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets or sets the unique id of the parent WpfInteractor or the root id
        /// (if the WpfInteractor node is logically attached directly to the window
        /// root node).
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// Gets or sets the Z ordering of this WpfInteractor compared to its sibling
        /// interactors (sharing the same parent id). A WpfInteractor with
        /// a higher Z value is considered to be on top of and to cover a
        /// WpfInteractor with a lower Z value.
        /// </summary>
        public int Z { get; set; }

        /// <summary>
        /// Gets the window handle of the top level window the associated
        /// FrameworkElement belongs to.
        /// </summary>
        public IntPtr WindowId { get; private set; }

        /// <summary>
        /// Gets the FrameworkElement associated with this WpfInteractor. It is on this
        /// element the attached properties from the EyeXFramework.Wpf.Behavior 
        /// class have been set, and it is on this element routed EyeX Behavior 
        /// events will be raised. 
        /// </summary>
        public FrameworkElement Element { get; private set; }

        /// <summary>
        /// Creates an empty WpfInteractor with no attached EyeX Behavior.
        /// </summary>
        /// <returns>An empty WpfInteractor.</returns>
        public static WpfInteractor CreateEmpty()
        {
            return new WpfInteractor(InteractorIDGenerator.CreateUniqueID());
        }

        /// <summary>
        /// Adds the supplied IEyeXBehavior to this WpfInteractor and saves
        /// a references to the associated FrameworkElement if this is the
        /// first added behavior. Will replace any already added behavior
        /// with the same BehaviorType.
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> where to add the behavior.</param>
        /// <param name="behavior">The <see cref="IEyeXBehavior"/> to add.</param>
        public void AddBehavior(FrameworkElement element, IEyeXBehavior behavior)
        {
            if (!IsInteractor)
            {
                Initialize(element);
            }

            var wasInteractor = IsInteractor;
            _behaviors.RemoveAll(x => x.BehaviorType == behavior.BehaviorType);
            _behaviors.Add(behavior);

            NotifyIfInteractorStatusChanged(wasInteractor);
        }

        /// <summary>
        /// Removes the behavior with the specified BehaviorType.
        /// Void operation if there is no behavior of that type to remove.
        /// </summary>
        /// <param name="behaviorType">The <see cref="BehaviorType"/>.</param>
        public void RemoveBehavior(BehaviorType behaviorType)
        {
            var wasInteractor = IsInteractor;
            _behaviors.RemoveAll(x => x.BehaviorType == behaviorType);
            NotifyIfInteractorStatusChanged(wasInteractor);
        }

        /// <summary>
        /// Creates an EyeX interactor from the properties and behaviors in
        /// this WpfInteractor, and adds it to the provided snapshot.
        /// </summary>
        /// <param name="snapshot">The <see cref="Snapshot"/> to 
        /// add the interactor to.</param>
        public void AddToSnapshot(Snapshot snapshot)
        {
            if (!IsQueryable)
            {
                return;
            }

            using (var interactor = snapshot.CreateInteractor(Id, ParentId, WindowId.ToString()))
            {
                var boundsRect = GetElementBoundsInScreenCoordinates(Element);
                using (var bounds = interactor.CreateBounds(BoundsType.Rectangular))
                {
                    bounds.SetRectangularData(boundsRect.Left, boundsRect.Top, boundsRect.Width, boundsRect.Height);
                }

                interactor.Z = Z;

                foreach (var behavior in _behaviors)
                {
                    behavior.AssignBehavior(interactor);
                }
            }
        }

        /// <summary>
        /// Delegates to each added behavior to handle events from the EyeX Engine.
        /// </summary>
        /// <remarks>
        /// Must be called on a UI thread.
        /// </remarks>
        /// <param name="event_">Event to be handled.</param>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Event is a reserved word.")]
        public void HandleEvent(InteractionEvent event_)
        {
            var eventBehaviors = event_.Behaviors;

            foreach (var eventBehavior in eventBehaviors)
            {
                foreach (var behavior in _behaviors)
                {
                    behavior.HandleEvent(Element, eventBehaviors);
                }

                eventBehavior.Dispose();
            }

            foreach (var eventBehavior in eventBehaviors)
            {
                eventBehavior.Dispose();
            }
        }

        private static Rect GetElementBoundsInScreenCoordinates(FrameworkElement element)
        {
            var elementUpperLeft = element.PointToScreen(new Point(0, 0));
            var elementBottomRight = element.PointToScreen(new Point(element.ActualWidth, element.ActualHeight));
            return new Rect(elementUpperLeft, elementBottomRight);
        }

        private void Initialize(FrameworkElement element)
        {
            Element = element;
            Element.Loaded += OnElementLoaded;
            Element.Unloaded += OnElementUnloaded;
        }

        /// <summary>
        /// The Clear method should be called when the WpfInteractor no longer
        /// qualifies as an interactor because of missing data, references or
        /// an empty list of behaviors.
        /// </summary>
        private void Clear()
        {
            if (Element != null)
            {
                Element.Loaded -= OnElementLoaded;
                Element.Unloaded -= OnElementUnloaded;
                Element = null;                
            }

            ParentId = string.Empty;
            WindowId = IntPtr.Zero;

            // Do not clear the interactor's Id here! The Id is needed for removal from 
            // the repository in WpfEyeXHost.
        }

        private void OnElementLoaded(object sender, RoutedEventArgs e)
        {
            SetWindowId();
        }

        private void OnElementUnloaded(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void SetWindowId()
        {
            var window = Window.GetWindow(Element);
            if (null == window)
            {
                WindowId = IntPtr.Zero;
            }
            else
            {
                var winHelper = new WindowInteropHelper(window);
                WindowId = winHelper.Handle;
            }
        }

        /// <summary>
        /// The WpfInteractor status as an interactor changes when
        /// the first EyeX Behavior is added to it and when the last
        /// EyeX Behavior is removed from it.
        /// </summary>
        /// <param name="wasInteractor">A value indicating whether there 
        /// was at least one EyeX Behavior attached to this WpfInteractor.</param>
        private void NotifyIfInteractorStatusChanged(bool wasInteractor)
        {
            EventHandler<ElementEventArgs> handler = null;
            if (!wasInteractor && IsInteractor)
            {
                handler = InteractorAdded;
            }
            else if (!IsInteractor && wasInteractor)
            {
                handler = InteractorRemoved;
            }

            if (handler != null)
            {
                handler(null, new ElementEventArgs(Element));
            }

            if (!IsInteractor)
            {
                Clear();
            }
        }
    }

    /// <summary>
    /// Event arguments associated with the <see cref="WpfInteractor.InteractorAdded"/> 
    /// and <see cref="WpfInteractor.InteractorRemoved"/> events.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Belongs here.")]
    public class ElementEventArgs : EventArgs
    {
        private readonly FrameworkElement _element;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementEventArgs"/> class.
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> related to the event.</param>
        public ElementEventArgs(FrameworkElement element)
        {
            _element = element;
        }

        /// <summary>
        /// Gets the <see cref="FrameworkElement"/> related to the event.
        /// </summary>
        public FrameworkElement Element
        {
            get { return _element; }
        }
    }
}
