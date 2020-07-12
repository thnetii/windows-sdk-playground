using System.Diagnostics;
using System.Xml.Serialization;

namespace THNETII.WindowsSdk.Serialization
{
    [XmlType]
    [DebuggerDisplay(nameof(VSCategory) + ": {" + nameof(VSCategory) + ",nq}")]
    public class SdkToolboxItems
    {
        [XmlAttribute("VSCategory")]
        public string VSCategory { get; set; } = null!;
    }
}
