using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Serialization;

using THNETII.Common;

namespace THNETII.WindowsSdk.Serialization
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "(),nq}")]
    [XmlRoot("ApplicationPlatform"), XmlType]
    public class ApplicationPlatform
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = null!;

        [XmlAttribute("friendlyName")]
        public string FriendlyName { get; set; } = null!;

        private readonly DuplexConversionTuple<string?, Version?> version =
            VersionSerializationHelper.GetConversionTuple();

        [XmlAttribute("version")]
        public string VersionString
        {
            get => version.RawValue!;
            set => version.RawValue = value;
        }

        [XmlIgnore]
        public Version Version
        {
            get => version.ConvertedValue!;
            set => version.ConvertedValue = value;
        }

        private readonly DuplexConversionTuple<string?, Version?> minimumVisualStudioVersion =
            VersionSerializationHelper.GetConversionTuple();

        [XmlElement("MinimumVisualStudioVersion")]
        public string? MinimumVisualStudioVersionString
        {
            get => minimumVisualStudioVersion.RawValue;
            set => minimumVisualStudioVersion.RawValue = value;
        }

        [XmlIgnore]
        public Version? MinimumVisualStudioVersion
        {
            get => minimumVisualStudioVersion.ConvertedValue;
            set => minimumVisualStudioVersion.ConvertedValue = value;
        }

        [XmlElement]
        public bool VersionedContent { get; set; }

        [XmlArray("ContainedApiContracts")]
        [XmlArrayItem("ApiContract")]
        public List<ApiContract> ContainedApiContracts { get; } =
            new List<ApiContract>();

        private string DebuggerDisplay()
        {
            var sb = new StringBuilder(Name);
            if (string.IsNullOrEmpty(VersionString))
            {
                if (sb.Length == 0)
                    sb.Append(", ");
                sb.Append("version=").Append(VersionString);
            }
            if (string.IsNullOrEmpty(FriendlyName))
            {
                if (sb.Length == 0)
                    sb.Append(FriendlyName);
                else
                    sb.Append(" (").Append(FriendlyName).Append(")");
            }

            return sb.ToString();
        }
    }
}
