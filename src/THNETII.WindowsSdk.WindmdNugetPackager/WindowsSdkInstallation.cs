using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using THNETII.Common;
using THNETII.WindowsSdk.Serialization;

namespace THNETII.WindowsSdk.WindmdNugetPackager
{
    public class WindowsSdkInstallation
    {
        private static readonly XmlSerializer platformSerializer =
            new XmlSerializer(typeof(ApplicationPlatform));
        private static readonly XmlSerializer previousPlatformsSerializer =
            new XmlSerializer(typeof(ApplicationPlatformList));

        public WindowsSdkInstallation(string rootPath, string version,
            ILogger<WindowsSdkInstallation>? logger = null)
        {
            this.logger = logger ?? NullLogger<WindowsSdkInstallation>
                .Instance;

            RootPath = rootPath.ThrowIfNullOrEmpty(nameof(rootPath));
            if (!Directory.Exists(rootPath))
                throw new ArgumentException(null, nameof(rootPath),
                    new DirectoryNotFoundException($"The specified Windows SDK root directory '{rootPath}' does not exist"));
            Version = version.ThrowIfNullOrEmpty(nameof(version));

            LicensesPath = Path.Join(RootPath, "Licenses", Version);
            UapPlatformPath = Path.Join(RootPath, "Platforms", "UAP", Version);
            ReferencesPath = Path.Join(RootPath, "References", Version);
            UnionMetadataPath = Path.Join(RootPath, "UnionMetadata", Version);
            UnionMetadataFacadePath = Path.Join(UnionMetadataPath, "Facade");
        }

        private readonly ILogger<WindowsSdkInstallation> logger;

        public string RootPath { get; }
        public string Version { get; }
        public string LicensesPath { get; }
        public string UapPlatformPath { get; }
        public string ReferencesPath { get; }
        public string UnionMetadataPath { get; }
        public string UnionMetadataFacadePath { get; }

        public ApplicationPlatform? GetUapApplicationPlatform()
        {
            string platformPath = Path.Combine(UapPlatformPath, "Platform.xml");
            try
            {
                using var platformStream = File.OpenRead(platformPath);
                using var platformReader = XmlReader.Create(platformStream);
                logger.LogDebug("Derserializing UAP Definition '{Platform}'", platformPath);
                return (ApplicationPlatform)platformSerializer
                    .Deserialize(platformReader);
            }
            catch (IOException ioExcept)
            {
                logger.LogError(ioExcept, "Unable to read Application Platform definition '{Platform}'", platformPath);
                return null;
            }
        }

        public string GetApiContractPath(ApiContract contract)
        {
            _ = contract ?? throw new ArgumentNullException(nameof(contract));

            return Path.Combine(ReferencesPath, contract.Name, contract.VersionString);
        }
    }
}
