using System;
using System.ComponentModel;

namespace BazthalLib.Configuration
{
    [Obsolete("Moved to BazthalLib.Extensibility.Serialization.ColorJsonConverter", false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class ColorJsonConverter : Extensibility.Serialization.ColorJsonConverter
    {
        // Intentionally left blank. This class is only here to maintain backward compatibility with existing code
        // that references this namespace. It inherits all functionality from the new location.
        // This class can be removed in future major releases.

        public ColorJsonConverter() {
            DebugUtils.LogObsoleteUsage("BazthalLib.Configuration.ColorJsonConverter", "BazthalLib.Extensibility.Serialization.ColorJsonConverter");
        }
    }

}
