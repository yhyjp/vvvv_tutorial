//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace EyeXFramework
{
    using System.Globalization;

    /// <summary>
    /// Generates unique IDs for interactors.
    /// </summary>
    internal static class InteractorIDGenerator
    {
        private static readonly object Lock = new object();
        private static int _nextId;

        /// <summary>
        /// Returns a unique ID.
        /// </summary>
        /// <returns>A unique ID.</returns>
        public static string CreateUniqueID()
        {
            lock (Lock)
            {
                int id = _nextId;
                _nextId++;
                return string.Format(CultureInfo.InvariantCulture, "I{0:00}", id);
            }
        }
    }
}
