using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

using THNETII.Common;

namespace THNETII.WindowsSdk.Serialization
{
    [XmlRoot("FileList"), XmlType]
    public class SdkExtensionManifestFileList
    {
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

        [XmlAttribute("ProductFamilyName")]
        public string? ProductFamilyName { get; set; }

        [XmlAttribute("SupportsMultipleVersion")]
        public string? SupportsMultipleVersion { get; set; }

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

        private readonly DuplexConversionTuple<string?, bool> supportPrefer32Bit =
            BooleanStringSerializationHelper.GetConversionTuple();

        [XmlAttribute("SupportPrefer32Bit")]
        public string? SupportPrefer32BitString
        {
            get => supportPrefer32Bit.RawValue;
            set => supportPrefer32Bit.RawValue = value;
        }

        [XmlIgnore]
        public bool SupportPrefer32Bit
        {
            get => supportPrefer32Bit.ConvertedValue;
            set => supportPrefer32Bit.ConvertedValue = value;
        }

        [XmlAttribute("MoreInfo")]
        public string? MoreInfoLink { get; set; }

        [XmlArray("ContainedApiContracts")]
        [XmlArrayItem("ApiContract")]
        public List<ApiContract> ContainedApiContracts { get; } =
            new List<ApiContract>();
    }
}
