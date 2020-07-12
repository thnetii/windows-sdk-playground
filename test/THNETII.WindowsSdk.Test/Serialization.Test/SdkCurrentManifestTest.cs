using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using Xunit;

namespace THNETII.WindowsSdk.Serialization.Test
{
    public static class SdkCurrentManifestTest
    {
        [SkippableFact]
        public static void CanDeserializeSdkManifest()
        {
            string? sdkRoot = WindowsSdkTestHelper.GetKitsRoot10();
            Skip.If(sdkRoot is null, "Could not find the Windows SDK in the registry");
            var sdkManifestPath = Path.Combine(sdkRoot, "SDKManifest.xml");
            Skip.IfNot(File.Exists(sdkManifestPath), "Windows SDK Manifest file does not exist");

            var serializer = new XmlSerializer(typeof(SdkManifestFileList));
            object sdkManifestObject;
            using (var sdkManifestStream = File.OpenRead(sdkManifestPath))
            using (var sdkManifestReader = XmlReader.Create(sdkManifestStream))
            {
                sdkManifestObject = serializer.Deserialize(sdkManifestReader);
            }

            var sdkFileList = Assert.IsType<SdkManifestFileList>(sdkManifestObject);
            Assert.NotNull(sdkFileList.PlatformIdentity);
        }
    }
}
