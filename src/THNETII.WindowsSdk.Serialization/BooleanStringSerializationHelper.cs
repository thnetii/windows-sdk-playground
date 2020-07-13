using System;

using THNETII.Common;
using THNETII.TypeConverter;

namespace THNETII.WindowsSdk.Serialization
{
    internal static class BooleanStringSerializationHelper
    {
        private static readonly Func<string?, bool> rawConvert =
            BooleanStringConverter.ParseOrDefault;
        private static readonly Func<bool, string> rawReverseConvert =
            BooleanStringConverter.ToString;

        internal static DuplexConversionTuple<string?, bool> GetConversionTuple() =>
            new DuplexConversionTuple<string?, bool>(rawConvert, rawReverseConvert);
    }
}
