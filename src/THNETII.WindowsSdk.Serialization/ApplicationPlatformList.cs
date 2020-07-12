using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace THNETII.WindowsSdk.Serialization
{
    [DebuggerDisplay("{" + nameof(Platforms) + "}")]
    [XmlRoot("PreviousPlatforms"), XmlType(Namespace = "http://microsoft.com/schemas/Windows/SDK/PreviousPlatforms")]
    public class ApplicationPlatformList : IList<ApplicationPlatform>
    {
        public ApplicationPlatform this[int index]
        {
            get => Platforms[index];
            set => Platforms[index] = value;
        }

        [XmlElement("ApplicationPlatform")]
        public List<ApplicationPlatform> Platforms { get; } =
            new List<ApplicationPlatform>();

        public int Count => Platforms.Count;

        bool ICollection<ApplicationPlatform>.IsReadOnly =>
            ((ICollection<ApplicationPlatform>)Platforms).IsReadOnly;

        public void Add(ApplicationPlatform item) => Platforms.Add(item);

        public void Clear() => Platforms.Clear();

        public bool Contains(ApplicationPlatform item) => Platforms.Contains(item);

        public void CopyTo(ApplicationPlatform[] array, int arrayIndex) =>
            Platforms.CopyTo(array, arrayIndex);

        public IEnumerator<ApplicationPlatform> GetEnumerator() =>
            Platforms.GetEnumerator();

        public int IndexOf(ApplicationPlatform item) => Platforms.IndexOf(item);

        public void Insert(int index, ApplicationPlatform item) =>
            Platforms.Insert(index, item);

        public bool Remove(ApplicationPlatform item) => Platforms.Remove(item);

        public void RemoveAt(int index) => Platforms.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable)Platforms).GetEnumerator();
    }
}
