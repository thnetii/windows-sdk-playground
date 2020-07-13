using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using THNETII.Common;

namespace THNETII.WindowsSdk.Serialization
{
    [XmlRoot("FileList"), XmlType]
    public class SdkManifestFileList
    {
        [XmlAttribute("DisplayName")]
        public string? DisplayName { get; set; }

        [XmlAttribute("PlatformIdentity")]
        public string? PlatformIdentity { get; set; }

        private readonly DuplexConversionTuple<string?, Version?> minVsVersion =
            VersionSerializationHelper.GetConversionTuple();

        private static readonly char[] SemicolonSeparator = new[] { ';' };

        [XmlAttribute("TargetFramework")]
        public string? TargetFrameworkString
        {
            set
            {
                string[] tfms = value is null
                    ? Array.Empty<string>()
                    : value.Split(SemicolonSeparator, StringSplitOptions.RemoveEmptyEntries);
                TargetFrameworks.Clear();
                TargetFrameworks.AddRange(tfms);
            }
            get => TargetFrameworks.Count == 0
                ? string.Empty
                : string.Join(";", TargetFrameworks) + ';';
        }

        [XmlIgnore]
        public List<string> TargetFrameworks { get; } = new List<string>();

        [XmlAttribute("MinVSVersion")]
        public string? MinVSVersionString
        {
            get => minVsVersion.RawValue;
            set => minVsVersion.RawValue = value!;
        }

        [XmlIgnore]

        public Version? MinVSVersion
        {
            get => minVsVersion.ConvertedValue;
            set => minVsVersion.ConvertedValue = value;
        }

        private readonly DuplexConversionTuple<string?, Version?> minOsVersion =
            VersionSerializationHelper.GetConversionTuple();

        [XmlAttribute("MinOSVersion")]
        public string? MinOSVersionString
        {
            get => minOsVersion.RawValue;
            set => minOsVersion.RawValue = value;
        }

        [XmlIgnore]
        public Version? MinOSVersion
        {
            get => minOsVersion.ConvertedValue;
            set => minOsVersion.ConvertedValue = value;
        }

        private readonly DuplexConversionTuple<string?, Version?> maxOSVersionTested =
            VersionSerializationHelper.GetConversionTuple();

        [XmlAttribute("MaxOSVersionTested")]
        public string? MaxOSVersionTestedString
        {
            get => maxOSVersionTested.RawValue;
            set => maxOSVersionTested.RawValue = value;
        }

        [XmlIgnore]
        public Version? MaxOSVersionTested
        {
            get => maxOSVersionTested.ConvertedValue;
            set => maxOSVersionTested.ConvertedValue = value;
        }

        [XmlAttribute("UnsupportedDowntarget")]
        public string? UnsupportedDowntarget { get; set; }

        [XmlElement("File")]
        public List<SdkFileReference> Files { get; } =
            new List<SdkFileReference>();
    }
}
