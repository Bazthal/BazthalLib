using System;
using System.ComponentModel;

namespace BazthalLib.Configuration
{
    [Obsolete("Moved to BazthalLib.Extensibility.Serialization.ThemeColorsJsonConverter", false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class ThemeColorsJsonConverter : Extensibility.Serialization.ColorJsonConverter
    {
        // Intentionally left blank. This class is only here to maintain backward compatibility with existing code
        // that references this namespace. It inherits all functionality from the new location.

        // This class can be removed in future major releases.
        public ThemeColorsJsonConverter() {
            DebugUtils.LogObsoleteUsage("BazthalLib.Configuration.ThemeColorsJsonConverter", "BazthalLib.Extensibility.Serialization.ThemeColorsJsonConverter");
        }  
    }

}
