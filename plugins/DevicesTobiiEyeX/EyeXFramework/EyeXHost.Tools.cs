//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

using System;

namespace EyeXFramework
{
    using Tobii.EyeX.Framework;

    public partial class EyeXHost
    {
        /// <summary>
        /// Starts the recalibration tool.
        /// </summary>
        public void LaunchRecalibration()
        {
            EnsureStarted();
            _context.LaunchConfigurationTool(ConfigurationTool.Recalibrate, data => { });
        }

        /// <summary>
        /// Starts the guest calibration tool.
        /// </summary>
        public void LaunchGuestCalibration()
        {
            EnsureStarted();
            _context.LaunchConfigurationTool(ConfigurationTool.GuestCalibration, data => { });
        }

        /// <summary>
        /// Starts the calibration testing tool.
        /// </summary>
        public void LaunchCalibrationTesting()
        {
            EnsureStarted();
            _context.LaunchConfigurationTool(ConfigurationTool.TestEyeTracking, data => { });            
        }

        /// <summary>
        /// Starts the display setup tool.
        /// </summary>
        public void LaunchDisplaySetup()
        {
            EnsureStarted();
            _context.LaunchConfigurationTool(ConfigurationTool.SetupDisplay, data => { });   
        }

        /// <summary>
        /// Starts the profile creation tool.
        /// </summary>
        public void LaunchProfileCreation()
        {
            EnsureStarted();
            _context.LaunchConfigurationTool(ConfigurationTool.CreateNewProfile, data => { });   
        }
    }
}
