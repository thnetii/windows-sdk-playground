using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.AccessControl;

using Microsoft.Win32;

namespace THNETII.WindowsSdk
{
    public static class WindowsSdkTestHelper
    {
        private const string WindowsKitsInstalledRootsKeyPath =
            @"Software\Microsoft\Windows Kits\Installed Roots";
        private const string KitsRoot10Name =
            "KitsRoot10";

        [SuppressMessage("Usage", "PC001:API not supported on all platforms")]
        public static string? GetKitsRoot10()
        {
            try
            {
                using var key = Registry.LocalMachine
                    .OpenSubKey(WindowsKitsInstalledRootsKeyPath, RegistryRights.ReadKey);
                return (string?)key?.GetValue(KitsRoot10Name);
            }
            catch (PlatformNotSupportedException)
            {
                return null;
            }
        }

        [SuppressMessage("Usage", "PC001:API not supported on all platforms")]
        public static string[]? GetSdkVersions()
        {
            try
            {
                using var key = Registry.LocalMachine
                    .OpenSubKey(WindowsKitsInstalledRootsKeyPath);
                return key?.GetSubKeyNames();
            }
            catch (PlatformNotSupportedException)
            {
                return null;
                throw;
            }
        }

        public static IEnumerable<string>? GetExtensionNames()
        {
            string? root = GetKitsRoot10();
            if (root is null)
                return null;
            return new DirectoryInfo(Path.Combine(root, "Extension SDKs"))
                .EnumerateDirectories().Select(di => di.Name);
        }
    }
}
