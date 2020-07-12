using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using THNETII.Common;

namespace THNETII.WindowsSdk.Serialization
{
    [XmlRoot("FileList"), XmlType]
    public class SdkManifestFileList
    {
        [XmlAttribute("PlatformIdentity")]
        public string? PlatformIdentity { get; set; }

        [XmlAttribute("TargetPlatform")]
        public string TargetPlatform { get; set; } = null!;

        private readonly DuplexConversionTuple<string?, Version?> targetPlatformMinVersion =
            VersionSerializationHelper.GetConversionTuple();

        [XmlAttribute("TargetPlatformMinVersion")]
        public string? TargetPlatformMinVersionString
        {
            get => targetPlatformMinVersion.RawValue;
            set => targetPlatformMinVersion.RawValue = value!;
        }

        [XmlIgnore]
        public Version? TargetPlatformMinVersion
        {
            get => targetPlatformMinVersion.ConvertedValue;
            set => targetPlatformMinVersion.ConvertedValue = value!;
        }

        private readonly DuplexConversionTuple<string?, Version?> targetPlatformVersion =
            VersionSerializationHelper.GetConversionTuple();

        [XmlAttribute("TargetPlatformVersion")]
        public string? TargetPlatformVersionString
        {
            get => targetPlatformVersion.RawValue;
            set => targetPlatformVersion.RawValue = value!;
        }

        [XmlIgnore]
        public Version? TargetPlatformVersion
        {
            get => targetPlatformVersion.ConvertedValue;
            set => targetPlatformVersion.ConvertedValue = value!;
        }

        [XmlAttribute("SDKType")]
        public string? SDKType { get; set; }

        [XmlAttribute("DisplayName")]
        public string? DisplayName { get; set; }

        [XmlAttribute("AppliesTo")]
        public string? AppliesTo { get; set; }

        private readonly DuplexConversionTuple<string?, Version?> minVsVersion =
            VersionSerializationHelper.GetConversionTuple();

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

        [XmlAttribute("ProductFamilyName")]
        public string ProductFamilyName { get; set; } = "Windows";

        [XmlAttribute("SupportsMultipleVersion")]
        public string? SupportsMultipleVersion { get; set; }

        private static readonly char[] SemicolonSeparator = new[] { ';' };

        [XmlAttribute("TargetFramework")]
        protected string TargetFrameworkString
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

        [XmlAttribute("SupportPrefer32Bit")]
        public string? SupportPrefer32Bit { get; set; }

        [XmlAttribute("MoreInfo")]
        public string? MoreInfoLink { get; set; }

        [XmlElement("File")]
        public List<SdkFileReference> Files { get; } =
            new List<SdkFileReference>();

        [XmlArray("ContainedApiContracts")]
        [XmlArrayItem("ApiContract")]
        public List<ApiContract> ContainedApiContracts { get; } =
            new List<ApiContract>();
    }
}
