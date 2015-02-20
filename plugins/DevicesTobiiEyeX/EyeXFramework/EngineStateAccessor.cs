//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

using Tobii.EyeX.Framework;

namespace EyeXFramework
{
    using System;
    using Tobii.EyeX.Client;

    /// <summary>
    /// Accesses and monitors engine states.
    /// Used by the EyeXHost.
    /// </summary>
    /// <typeparam name="T">Data type of the engine state.</typeparam>
    internal class EngineStateAccessor<T>
    {
        private readonly string _statePath;
        private readonly AsyncDataHandler _handler;
        private EngineStateValue<T> _currentValue = EngineStateValue<T>.Invalid;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineStateAccessor{T}"/> class.
        /// </summary>
        /// <param name="statePath">The state path.</param>
        public EngineStateAccessor(string statePath)
        {
            _statePath = statePath;
            _handler = OnStateChanged;
        }

        /// <summary>
        /// Event raised when the associated engine state value has changed.
        /// </summary>
        public event EventHandler<EngineStateValue<T>> Changed; 

        /// <summary>
        /// Gets the current value of the engine state.
        /// </summary>
        /// <returns>The state value.</returns>
        public EngineStateValue<T> GetCurrentValue()
        {
            return _currentValue;
        }

        /// <summary>
        /// Method to be invoked when the interaction context has been created.
        /// </summary>
        /// <param name="context">The interaction context.</param>
        public void OnContextCreated(Context context)
        {
            context.RegisterStateChangedHandler(_statePath, _handler);
        }

        /// <summary>
        /// Method to be invoked when a connection to the EyeX Engine has been established.
        /// </summary>
        /// <param name="context">The interaction context.</param>
        public void OnConnected(Context context)
        {
            context.GetStateAsync(_statePath, _handler);
        }

        /// <summary>
        /// Method to be invoked when the connection to the EyeX Engine has been lost.
        /// </summary>
        public void OnDisconnected()
        {
            SetCurrentValue(EngineStateValue<T>.Invalid);
        }

        private void OnStateChanged(AsyncData data)
        {
            using (data)
            {
                ResultCode resultCode;
                if (!data.TryGetResultCode(out resultCode) || resultCode != ResultCode.Ok)
                {
                    return;
                }

                using (var stateBag = data.GetDataAs<StateBag>())
                {
                    T value;
                    if (stateBag.TryGetStateValue(out value, _statePath))
                    {
                        SetCurrentValue(new EngineStateValue<T>(value));
                    }
                }
            }
        }

        private void SetCurrentValue(EngineStateValue<T> value)
        {
            _currentValue = value;

            var handler = Changed;
            if (handler != null)
            {
                handler(this, value);
            }
        }
    }
}
