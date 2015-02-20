//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Tobii.EyeX.Client;

    /// <summary>
    /// Holds an engine state value and a flag indicating the validity of the value.
    /// </summary>
    /// <typeparam name="T">Data type of the engine state.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Type is used both as a property value and event data.")]
    public sealed class EngineStateValue<T> : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EngineStateValue{T}"/> class.
        /// </summary>
        /// <param name="value">The state handler.</param>
        public EngineStateValue(T value)
        {
            Value = value;
            IsValid = true;
        }

        private EngineStateValue()
        {
            // Will result in a value where IsValid is false
        }

        /// <summary>
        /// Gets a value representing an invalid state value.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Mainly for internal use.")]
        public static EngineStateValue<T> Invalid
        {
            get
            {
                return new EngineStateValue<T>();
            }
        }

        /// <summary>
        /// Gets the state value.
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the state value is valid.
        /// The state will always be unknown when disconnected from the EyeX Engine.
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            if (IsValid)
            {
                if (typeof(T) == typeof(Size2))
                {
                    var value = (Size2)((object)Value);
                    return string.Format(CultureInfo.InvariantCulture, "{0:0.0} x {1:0.0}", value.Width, value.Height);
                }

                if (typeof(T) == typeof(Rect))
                {
                    var value = (Rect)((object)Value);
                    return string.Format(CultureInfo.InvariantCulture, "({0}, {1}), {2} x {3}", value.X, value.Y, value.Width, value.Height);
                }

                return Value.ToString();
            }
            else
            {
                return "INVALID";
            }
        }
    }
}
