using System;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Serialization;

using THNETII.Common;

namespace THNETII.WindowsSdk.Serialization
{
    [XmlType]
    [DebuggerDisplay("{" + nameof(ToAssemblyName) + "(),nq}")]
    public class ApiContract
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = null!;

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

        public override string ToString() =>
            FormattableString.Invariant($"{Name}, Version={Version}");

        public AssemblyName ToAssemblyName() => new AssemblyName(ToString());
    }
}
