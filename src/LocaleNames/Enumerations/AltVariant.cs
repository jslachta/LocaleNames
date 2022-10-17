using System;
using System.Collections.Generic;
using System.Text;

namespace LocaleNames.Enumerations
{
    /// <summary>
    /// Provides variants for name.
    /// </summary>
    public enum AltVariant
    {
        /// <summary>
        /// The none variant.
        /// </summary>
        None = 0,

        /// <summary>
        /// The common name variant.
        /// </summary>
        Common = 1,

        /// <summary>
        /// The alternative name variant.
        /// </summary>
        Alternative = 2,

        /// <summary>
        /// The short name variant.
        /// </summary>
        Short = 4,

        /// <summary>
        /// The long name variant.
        /// </summary>
        Long = 8,

        /// <summary>
        /// The menu name variant.
        /// </summary>
        Menu = 16
    }
}
