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
    /// Provides a stream of gaze point data.
    /// See <see cref="GazePointEventArgs"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Suffix is DataStream, not just Stream. Low risk for confusion.")]
    public sealed class GazePointDataStream : DataStreamBase<GazePointEventArgs>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GazePointDataStream"/> class.
        /// </summary>
        /// <param name="mode">Specifies the kind of processing applied to the eye-gaze data.</param>
        public GazePointDataStream(GazePointDataMode mode)
        {
            Mode = mode;
        }

        /// <summary>
        /// Gets the kind of processing applied to the eye-gaze data.
        /// </summary>
        public GazePointDataMode Mode { get; private set; }

        /// <summary>
        /// Assigns the behavior corresponding to the data stream type to an interactor.
        /// </summary>
        /// <param name="interactor">The global interactor to add the data stream behavior to.</param>
        protected override void AssignBehavior(Interactor interactor)
        {
            var parameters = new GazePointDataParams() { GazePointDataMode = Mode };
            interactor.CreateGazePointDataBehavior(ref parameters);
        }

        /// <summary>
        /// Extracts data points from an event from the EyeX Engine.
        /// </summary>
        /// <param name="behaviors">The <see cref="Behavior"/> instances containing the event data.</param>
        /// <returns>The collection of data points.</returns>
        protected override IEnumerable<GazePointEventArgs> ExtractDataPoints(IEnumerable<Behavior> behaviors)
        {
            foreach (var behavior in behaviors
                .Where(behavior => behavior.BehaviorType == BehaviorType.GazePointData))
            {
                GazePointDataEventParams parameters;
                if (behavior.TryGetGazePointDataEventParams(out parameters) &&
                    parameters.GazePointDataMode == Mode)
                {
                    yield return new GazePointEventArgs(parameters.X, parameters.Y, parameters.Timestamp);
                }
            }
        }
    }

    /// <summary>
    /// Provides event data for the gaze point data stream.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Belongs here.")]
    public sealed class GazePointEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GazePointEventArgs"/> class.
        /// </summary>
        /// <param name="x">X coordinate in physical pixels.</param>
        /// <param name="y">Y coordinate in physical pixels.</param>
        /// <param name="timestamp">The point in time when the data point was captured. Milliseconds.</param>
        public GazePointEventArgs(double x, double y, double timestamp)
        {
            X = x;
            Y = y;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Gets the X coordinate in physical pixels.
        /// </summary>
        public double X { get; private set; }

        /// <summary>
        /// Gets the Y coordinate in physical pixels.
        /// </summary>
        public double Y { get; private set; }

        /// <summary>
        /// Gets the point in time when the data point was captured. Milliseconds.
        /// </summary>
        public double Timestamp { get; private set; }
    }
}
