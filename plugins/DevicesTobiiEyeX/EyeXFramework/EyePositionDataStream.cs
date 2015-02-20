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
    /// Provides a stream of eye position data, that is, eye positions in 3D space.
    /// See <see cref="EyePositionEventArgs"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Suffix is DataStream, not just Stream. Low risk for confusion.")]
    public sealed class EyePositionDataStream : DataStreamBase<EyePositionEventArgs>
    {
        /// <summary>
        /// Assigns the behavior corresponding to the data stream type to an interactor.
        /// </summary>
        /// <param name="interactor">The global interactor to add the data stream behavior to.</param>
        protected override void AssignBehavior(Interactor interactor)
        {
            var behavior = interactor.CreateBehavior(BehaviorType.EyePositionData);
            behavior.Dispose();
        }

        /// <summary>
        /// Extracts data points from an event from the EyeX Engine.
        /// </summary>
        /// <param name="behaviors">The <see cref="Behavior"/> instances containing the event data.</param>
        /// <returns>The collection of data points.</returns>
        protected override IEnumerable<EyePositionEventArgs> ExtractDataPoints(IEnumerable<Behavior> behaviors)
        {
            foreach (var behavior in behaviors
                .Where(behavior => behavior.BehaviorType == BehaviorType.EyePositionData))
            {
                EyePositionDataEventParams parameters;
                if (behavior.TryGetEyePositionDataEventParams(out parameters))
                {
                    var left = new EyePosition(parameters.HasLeftEyePosition != EyeXBoolean.False, parameters.LeftEyeX, parameters.LeftEyeY, parameters.LeftEyeZ);
                    var leftNormalized = new EyePosition(parameters.HasLeftEyePosition != EyeXBoolean.False, parameters.LeftEyeXNormalized, parameters.LeftEyeYNormalized, parameters.LeftEyeZNormalized);
                    var right = new EyePosition(parameters.HasRightEyePosition != EyeXBoolean.False, parameters.RightEyeX, parameters.RightEyeY, parameters.RightEyeZ);
                    var rightNormalized = new EyePosition(parameters.HasRightEyePosition != EyeXBoolean.False, parameters.RightEyeXNormalized, parameters.RightEyeYNormalized, parameters.RightEyeZNormalized);
                    yield return new EyePositionEventArgs(left, leftNormalized, right, rightNormalized, parameters.Timestamp);
                }
            }
        }
    }

    /// <summary>
    /// Provides event data for the eye position data stream.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Belongs here.")]
    public sealed class EyePositionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EyePositionEventArgs"/> class.
        /// </summary>
        /// <param name="leftEye">Position of the left eye.</param>
        /// <param name="leftEyeNormalized">Normalized position of the left eye.</param>
        /// <param name="rightEye">Position of the right eye.</param>
        /// <param name="rightEyeNormalized">Normalized position of the right eye.</param>
        /// <param name="timestamp">The point in time when the data point was captured. Milliseconds.</param>
        public EyePositionEventArgs(EyePosition leftEye, EyePosition leftEyeNormalized, EyePosition rightEye, EyePosition rightEyeNormalized, double timestamp)
        {
            LeftEye = leftEye;
            LeftEyeNormalized = leftEyeNormalized;
            RightEye = rightEye;
            RightEyeNormalized = rightEyeNormalized;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Gets the position of the left eye.
        /// </summary>
        public EyePosition LeftEye { get; private set; }

        /// <summary>
        /// Gets the normalized position of the left eye.
        /// </summary>
        public EyePosition LeftEyeNormalized { get; private set; }

        /// <summary>
        /// Gets the position of the right eye.
        /// </summary>
        public EyePosition RightEye { get; private set; }

        /// <summary>
        /// Gets the normalized position of the right eye.
        /// </summary>
        public EyePosition RightEyeNormalized { get; private set; }

        /// <summary>
        /// Gets the point in time when the data point was captured. Milliseconds.
        /// </summary>
        public double Timestamp { get; private set; }
    }

    /// <summary>
    /// Position of an eye in 3D space.
    /// The position is taken relative to the center of the screen where the eye tracker is mounted.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Belongs here.")]
    public sealed class EyePosition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EyePosition"/> class.
        /// </summary>
        /// <param name="isValid">Flag indicating if the eye position is valid. (Sometimes only a single eye is tracked.)</param>
        /// <param name="x">X coordinate of the eye position, in millimeters.</param>
        /// <param name="y">Y coordinate of the eye position, in millimeters.</param>
        /// <param name="z">Z coordinate of the eye position, in millimeters.</param>
        public EyePosition(bool isValid, double x, double y, double z)
        {
            IsValid = isValid;
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Gets a value indicating whether the eye position is valid. (Sometimes only a single eye is tracked.)
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// Gets the X coordinate of the eye position, in millimeters.
        /// </summary>
        public double X { get; private set; }

        /// <summary>
        /// Gets the Y coordinate of the eye position, in millimeters.
        /// </summary>
        public double Y { get; private set; }

        /// <summary>
        /// Gets the Z coordinate of the eye position, in millimeters.
        /// </summary>
        public double Z { get; private set; }
    }
}
