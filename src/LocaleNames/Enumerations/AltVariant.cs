using System;
using System.Collections.Generic;
using System.Text;

namespace LocaleNames.Enumerations
{
    /// <summary>
    /// Provides variants for name.
    /// </summary>
    [Flags]
    public enum AltVariant
    {
        /// <summary>
        /// The common name variant.
        /// </summary>
        Common = 0,

        /// <summary>
        /// The alternative name variant.
        /// </summary>
        Alternative = 1,

        /// <summary>
        /// The short name variant.
        /// </summary>
        Short = 2,

        /// <summary>
        /// The long name variant.
        /// </summary>
        Long = 4,

        /// <summary>
        /// The menu name variant.
        /// </summary>
        Menu = 8
    }
}
