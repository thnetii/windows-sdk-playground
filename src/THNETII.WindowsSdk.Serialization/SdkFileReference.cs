using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace THNETII.WindowsSdk.Serialization
{
    [XmlType]
    [DebuggerDisplay(nameof(Reference) + " = {" + nameof(Reference) + ",nq}, ToolboxItems: {" + nameof(ToolboxItems) + "}")]
    public class SdkFileReference
    {
        [XmlAttribute("Reference")]
        public string? Reference { get; set; }

        [XmlElement("ToolboxItems")]
        public List<SdkToolboxItems> ToolboxItems { get; } =
            new List<SdkToolboxItems>();
    }
}
