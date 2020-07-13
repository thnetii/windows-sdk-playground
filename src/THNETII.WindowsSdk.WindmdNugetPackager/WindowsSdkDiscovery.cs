using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Xml;
using System.Xml.Serialization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32;

using THNETII.WindowsSdk.Serialization;

namespace THNETII.WindowsSdk.WindmdNugetPackager
{
    public class WindowsSdkDiscovery
    {
        private const string WindowsKitsInstalledRootsKeyPath =
           @"Software\Microsoft\Windows Kits\Installed Roots";
        private const string KitsRoot10KeyName = "KitsRoot10";

        private static readonly XmlSerializer manifestSerializer =
            new XmlSerializer(typeof(SdkManifestFileList));
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<WindowsSdkDiscovery>? logger;

        public WindowsSdkDiscovery(
            IServiceProvider serviceProvider,
            ILogger<WindowsSdkDiscovery>? logger = null)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger ?? NullLogger<WindowsSdkDiscovery>.Instance;
        }

        public IEnumerable<WindowsSdkInstallation> GetInstalledSdks()
        {
            var (sdkRoot, sdkVersions) = GetRegistryDiscovery();
            if (sdkRoot is null)
                return Enumerable.Empty<WindowsSdkInstallation>();

            sdkVersions ??= Array.Empty<string>();
            var installations = new List<WindowsSdkInstallation>(sdkVersions.Length);
            foreach (var sdkVersion in sdkVersions)
            {
                var logger = serviceProvider.GetService<ILogger<WindowsSdkInstallation>>();
                installations.Add(new WindowsSdkInstallation(sdkRoot, sdkVersion, logger));
            }

            return installations;
        }

        public WindowsSdkInstallation? GetManifestInstallation()
        {
            string? sdkVersion = null;
            var (sdkRoot, sdkVersions) = GetRegistryDiscovery();
            if (sdkRoot is null)
            {

                return null;
            }

            string sdkManifestPath = Path.Combine(sdkRoot, "SDKManifest.xml");
            try
            {
                SdkManifestFileList sdkManifestList;
                using (var sdkManifestStream = File.OpenRead(sdkManifestPath))
                using (var sdkManifestReader = XmlReader.Create(sdkManifestStream))
                {
                    logger.LogDebug("Deserializing SDK Manifest '{SDKManifest}'", sdkManifestPath);
                    sdkManifestList = (SdkManifestFileList)manifestSerializer
                        .Deserialize(sdkManifestReader);
                }

                if (sdkManifestList.PlatformIdentity is string ident)
                {
                    var asmName = new AssemblyName(ident);
                    logger.LogInformation($"Identified '{{{nameof(sdkManifestList.PlatformIdentity)}}}' from SDK Manifest {{SDKManifest}}",
                        asmName, sdkManifestPath);
                    sdkVersion = asmName.Version?.ToString();
                }
            }
            catch (IOException ioExcept)
            {
                logger.LogError(ioExcept, "Unable to read SDK Manifest from '{SDKManifest}'", sdkManifestPath);
            }

            sdkVersion ??= sdkVersions?[^1];
            if (sdkVersion is null)
                return null;

            try { return new WindowsSdkInstallation(sdkRoot, sdkVersion); }
            catch (ArgumentException ae)
            when (ae.InnerException is DirectoryNotFoundException)
            {

                return null;
            }
        }

        [SuppressMessage("Usage", "PC001: API not supported on all platforms")]
        private (string? Root, string[]? Versions) GetRegistryDiscovery()
        {
            string? sdkRoot = null;
            string[]? sdkVersions = null;

            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(
                    WindowsKitsInstalledRootsKeyPath, RegistryRights.ReadKey);
                if (!(key is null))
                {
                    sdkRoot = (string?)key.GetValue(KitsRoot10KeyName);
                    sdkVersions = key.GetSubKeyNames();
                }
            }
            catch (PlatformNotSupportedException pnse)
            {
                logger.LogError(pnse,
                    "Unable to open get value entry '{Name}' from Registry Key '{RegistryKey}'",
                    KitsRoot10KeyName,
                    @"HKEY_LOCAL_MACHINE\" + WindowsKitsInstalledRootsKeyPath);
                return default;
            }

            if (string.IsNullOrEmpty(sdkRoot))
            {
                logger.LogError("Could not find the Windows SDK in the registry");
                return default;
            }

            logger.LogDebug($"Discovered Windows SDK root at {{{KitsRoot10KeyName}}}", sdkRoot);

            return (sdkRoot, sdkVersions);
        }
    }
}
