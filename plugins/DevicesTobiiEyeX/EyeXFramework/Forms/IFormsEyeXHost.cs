//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework.Forms
{
    /// <summary>
    /// Interface used for mocking the EyeXHost.
    /// </summary>
    public interface IFormsEyeXHost
    {
        /// <summary>
        /// Connects a behavior map so that it may receive queries and events from the EyeX Engine.
        /// </summary>
        /// <param name="behaviorMap">Map to be connected.</param>
        void Connect(BehaviorMap behaviorMap);

        /// <summary>
        /// Trigger an activation ("gaze click").
        /// Use this method if you want to bind the click command to a key other than the one used 
        /// by the EyeX Engine -- or to something other than a key press event.
        /// </summary>
        void TriggerActivation();
    }
}
