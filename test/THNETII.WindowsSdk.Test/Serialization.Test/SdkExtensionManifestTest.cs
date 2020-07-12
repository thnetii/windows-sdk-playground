using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using Xunit;

namespace THNETII.WindowsSdk.Serialization.Test
{
    public static class SdkExtensionManifestTest
    {
        public static IEnumerable<object[]> GetSdkExtensionVersions()
        {
            var versions = WindowsSdkTestHelper.GetSdkVersions();
            if (versions is null)
                return Enumerable.Empty<object[]>();
            var extNames = WindowsSdkTestHelper.GetExtensionNames();
            return extNames.SelectMany(ext => versions.Select(v => new[] { ext, v }));
        }

        [SkippableTheory]
        [MemberData(nameof(GetSdkExtensionVersions))]
        public static void CanDeserializeSdkManifest(string extension, string sdkVersion)
        {
            string? sdkRoot = WindowsSdkTestHelper.GetKitsRoot10();
            Skip.If(sdkRoot is null, "Could not find the Windows SDK in the registry");
            string sdkManifestPath = Path.Combine(sdkRoot, "Extension SDKs",
                extension, sdkVersion, "SDKManifest.xml");
            Skip.IfNot(File.Exists(sdkManifestPath), $"Extension SDK Manifest file \"{sdkManifestPath}\" does not exist");

            var serializer = new XmlSerializer(typeof(SdkManifestFileList));
            object sdkManifestObject;
            using (var sdkManifestStream = File.OpenRead(sdkManifestPath))
            using (var sdkManifestReader = XmlReader.Create(sdkManifestStream))
            {
                sdkManifestObject = serializer.Deserialize(sdkManifestReader);
            }

            var sdkFileList = Assert.IsType<SdkManifestFileList>(sdkManifestObject);
            Assert.NotNull(sdkFileList.TargetPlatform);
            Assert.NotNull(sdkFileList.TargetPlatformVersion);

            Assert.NotEmpty(sdkFileList.ContainedApiContracts);
            Assert.All(sdkFileList.ContainedApiContracts, c =>
            {
                Assert.NotEmpty(c.Name);
                Assert.NotNull(c.Version);

                var asmName = c.ToAssemblyName();
                Assert.Equal(c.Name, asmName.Name);
                Assert.Equal(c.Version, asmName.Version);
            });
        }
    }
}
