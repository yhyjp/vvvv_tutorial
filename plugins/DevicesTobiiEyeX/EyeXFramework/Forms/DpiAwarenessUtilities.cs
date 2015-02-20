//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework.Forms
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Provides utility functions for DPI awareness.
    /// </summary>
    public static class DpiAwarenessUtilities
    {
        /// <summary>
        /// Marks the running process as being DPI aware.
        /// </summary>
        public static void SetProcessDpiAware()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
                Environment.OSVersion.Version.Major >= 6) // meaning Vista or later
            {
                // Disable automatic DPI scaling.
                // DPI scaling breaks the mapping between client coordinates and screen coordinates.
                NativeMethods.SetProcessDPIAware();
            }
        }

        private static class NativeMethods
        {
            [DllImport("user32")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetProcessDPIAware();
        }
    }
}
