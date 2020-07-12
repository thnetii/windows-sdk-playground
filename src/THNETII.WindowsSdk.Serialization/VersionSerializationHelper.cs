using System;

using THNETII.Common;

namespace THNETII.WindowsSdk.Serialization
{
    internal static class VersionSerializationHelper
    {
        private static readonly Func<string?, Version?> rawConvert =
            s => string.IsNullOrEmpty(s) ? null! : Version.Parse(s);
        private static readonly Func<Version?, string?> rawReverseConvert =
            v => v?.ToString();

        internal static DuplexConversionTuple<string?, Version?>
            GetConversionTuple() =>
            new DuplexConversionTuple<string?, Version?>(
                rawConvert, rawReverseConvert);
    }
}
