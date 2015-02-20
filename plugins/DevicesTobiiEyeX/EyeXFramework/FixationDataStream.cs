//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Tobii.EyeX.Client;
    using Tobii.EyeX.Framework;

    /// <summary>
    /// Provides a stream of fixation data.
    /// See <see cref="FixationEventArgs"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Suffix is DataStream, not just Stream. Low risk for confusion.")]
    public sealed class FixationDataStream : DataStreamBase<FixationEventArgs>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FixationDataStream"/> class.
        /// </summary>
        /// <param name="mode">Specifies the kind of fixation detection to be used.</param>
        public FixationDataStream(FixationDataMode mode)
        {
            Mode = mode;
        }

        /// <summary>
        /// Gets the kind of fixation detection used.
        /// </summary>
        public FixationDataMode Mode { get; private set; }

        /// <summary>
        /// Assigns the behavior corresponding to the data stream type to an interactor.
        /// </summary>
        /// <param name="interactor">The global interactor to add the data stream behavior to.</param>
        protected override void AssignBehavior(Interactor interactor)
        {
            var parameters = new FixationDataParams { FixationDataMode = Mode };
            interactor.CreateFixationDataBehavior(ref parameters);
        }

        /// <summary>
        /// Extracts data points from an event from the EyeX Engine.
        /// </summary>
        /// <param name="behaviors">The <see cref="Behavior"/> instances containing the event data.</param>
        /// <returns>The collection of data points.</returns>
        protected override IEnumerable<FixationEventArgs> ExtractDataPoints(IEnumerable<Behavior> behaviors)
        {
            foreach (var behavior in behaviors
                .Where(behavior => behavior.BehaviorType == BehaviorType.FixationData))
            {
                FixationDataEventParams parameters;
                if (behavior.TryGetFixationDataEventParams(out parameters) &&
                    parameters.FixationDataMode == Mode)
                {
                    yield return new FixationEventArgs(parameters.EventType, parameters.X, parameters.Y, parameters.Timestamp);
                }
            }
        }
    }

    /// <summary>
    /// Provides event data for the fixation data stream.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Belongs here.")]
    public sealed class FixationEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FixationEventArgs"/> class.
        /// </summary>
        /// <param name="eventType">Specifies the kind of fixation event that occurred.</param>
        /// <param name="x">X coordinate of the gaze point in physical pixels.</param>
        /// <param name="y">Y coordinate of the gaze point in physical pixels.</param>
        /// <param name="timestamp">Timestamp in milliseconds.</param>
        public FixationEventArgs(FixationDataEventType eventType, double x, double y, double timestamp)
        {
            EventType = eventType;
            X = x;
            Y = y;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Gets the kind of fixation event that occurred.
        /// The sequence is: Begin, [Data, ...], End.
        /// </summary>
        public FixationDataEventType EventType { get; private set; }

        /// <summary>
        /// Gets the X coordinate of the gaze point in physical pixels.
        /// </summary>
        public double X { get; private set; }

        /// <summary>
        /// Gets the Y coordinate of the gaze point in physical pixels.
        /// </summary>
        public double Y { get; private set; }

        /// <summary>
        /// Gets the timestamp in milliseconds.
        /// </summary>
        public double Timestamp { get; private set; }
    }
}
