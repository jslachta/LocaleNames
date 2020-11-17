using System.Collections.Generic;

namespace PrepareLocaleData.Model
{
    /// <summary>
    /// DTO for loading the data from embedded resource.
    /// </summary>
    public class ResourceLocale
    {
        /// <summary>
        /// Dictionary - code as a key, translation as a value.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        public Dictionary<string, string> Values { get; set; }
    }
}
