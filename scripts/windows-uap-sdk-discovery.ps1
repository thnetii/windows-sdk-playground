[CmdletBinding()]
param (
    [Parameter()]
    [string]$BuildDirectory = (Join-Path (Join-Path $PSScriptRoot "..") "bld"),
    [Parameter()]
    [string]$OutputDirectory = (Join-Path (Join-Path $PSScriptRoot "..") (Join-Path "bin" "Release"))
)

[System.Text.Encoding]$Utf8NoBom = New-Object System.Text.UTF8Encoding @($false)
function New-NuspecXmlWriterSettings {
    [OutputType([System.Xml.XmlWriterSettings])]
    param ()
    [System.Xml.XmlWriterSettings]$Settings = `
        New-Object System.Xml.XmlWriterSettings
    $Settings.Indent = $true
    $Settings.Encoding = $Utf8NoBom
    return $Settings
}
[System.Xml.XmlWriterSettings]$NuspecXmlWriterSettings = New-NuspecXmlWriterSettings

$NuspecXsdNs = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd"
function New-MicrosoftNuspec {
    [OutputType([xml])]
    param (
        [Parameter(Mandatory=$true)]
        [string]$Path
    )
    $PSCmdlet.WriteVerbose("Creating new nuspec file `"$Path`"")
    New-Item -ItemType File $Path -Force | Out-Null
    [System.Xml.XmlWriter]$NuspecXmlWriter = [System.Xml.XmlWriter]::Create($Path, $NuspecXmlWriterSettings)
    $NuspecXmlWriter.WriteStartDocument()
    # void WriteStartElement(string prefix, string localName, string ns)
    $NuspecXmlWriter.WriteStartElement("", "package", $NuspecXsdNs)
    $NuspecXmlWriter.WriteStartElement("metadata")
    $NuspecXmlWriter.WriteElementString("id", "")
    $NuspecXmlWriter.WriteElementString("version", "")
    $NuspecXmlWriter.WriteElementString("authors", "Microsoft")
    $NuspecXmlWriter.WriteElementString("owners", "Microsoft")
    $NuspecXmlWriter.WriteElementString("requireLicenseAcceptance", "true")
    $NuspecXmlWriter.WriteElementString("description", "")
    $NuspecXmlWriter.WriteElementString("copyright", "© Microsoft Corporation. All rights reserved.")
    $NuspecXmlWriter.WriteStartElement("license")
    $NuspecXmlWriter.WriteAttributeString("type", "file")
    $NuspecXmlWriter.WriteString("LICENSE")
    $NuspecXmlWriter.WriteEndElement()
    $NuspecXmlWriter.WriteEndElement()
    $NuspecXmlWriter.WriteEndElement()
    $NuspecXmlWriter.WriteEndDocument()
    $NuspecXmlWriter.Flush()
    $NuspecXmlWriter.Close()
    Remove-Variable NuspecXmlWriter
    return [xml](Get-Content $Path)
}
function Set-NuspecMetadata {
    param (
        [Parameter(Mandatory=$true)]
        [ValidateNotNull()]
        [xml]$NuspecXml,
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$Id,
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$Version,
        [Parameter(Mandatory=$false)]
        [string]$AppendDescription = $null
    )
    [System.Xml.XmlElement]$Metadata = $NuspecXml.package.metadata
    $Metadata.id = $Id
    $Metadata.version = $Version
    [string]$Description = $Metadata.description
    if ($AppendDescription -and $Description -notlike "*$AppendDescription*") {
        $DescriptionBuilder = New-Object System.Text.StringBuilder
        if ($Description) {
            $DescriptionBuilder.AppendLine($Description)
        }
        $DescriptionBuilder.AppendLine($AppendDescription) | Out-Null
        $Metadata.description = $DescriptionBuilder.ToString()
        Remove-Variable DescriptionBuilder
    }
}
function Add-NuspecLicenses {
    param (
        [Parameter(Mandatory=$true)]
        [ValidateNotNull()]
        [xml]$NuspecXml,
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$NuspecPath,
        [Parameter(Mandatory=$false)]
        [string]$LicensesDirectory = $null,
        [Parameter(Mandatory=$false)]
        [ValidateNotNull()]
        [string]$TargetFramework = "uap"
    )

    [System.Xml.XmlElement]$NuspecFilesElement = $NuspecXml.package.files
    if (-not $NuspecFilesElement) {
        $NuspecFilesElement = $NuspecXml.CreateElement("files", $NuspecXsdNs)
        $NuspecXml.package.AppendChild($NuspecFilesElement) | Out-Null
    }

    $PSCmdlet.WriteVerbose("Adding license files from `"$LicensesDirectory`" to `"nuspec://$($NuspecXml.package.metadata.id)/$($NuspecXml.package.metadata.version)`", TFM: $TargetFramework")
    $LicenseBundlePath = Join-Path $NuspecPath (Join-Path ".." "LICENSE")
    [System.Collections.Generic.HashSet[string]]$Licenses = New-Object "System.Collections.Generic.HashSet[string]"
    Get-Content -ErrorAction "SilentlyContinue" $LicenseBundlePath | ForEach-Object { $Licenses.Add($_) | Out-Null }
    [string]$LicensesTargetPath = Join-Path "licenses" $TargetFramework
    Get-ChildItem -File -Recurse $LicensesDirectory | ForEach-Object {
        $LicenseFileSource = $_.FullName
        $LicenseRelative = [System.IO.Path]::GetRelativePath($LicensesDirectory, $LicenseFileSource)
        $LicenseFileTarget = Join-Path $LicensesTargetPath $LicenseRelative

        [System.Xml.XmlElement]$LicenseFileElement = $NuspecFilesElement.file |`
            Where-Object { $_.src -Like $LicenseFileSource -and $_.target -Like $LicenseFileTarget } |`
            Select-Object -First 1
        if (-not $LicenseFileElement) {
            $LicenseFileElement = $NuspecXml.CreateElement("file", $NuspecXsdNs)
            $LicenseFileElement.SetAttribute("src", $LicenseFileSource)
            $LicenseFileElement.SetAttribute("target", $LicenseFileTarget)
            $NuspecFilesElement.AppendChild($LicenseFileElement) | Out-Null
        }
        $Licenses.Add($LicenseFileTarget) | Out-Null
    }

    $Licenses | Sort-Object | Out-File -FilePath $LicenseBundlePath -Encoding $Utf8NoBom -Force
    $LicenseBundlePath = Resolve-Path $LicenseBundlePath
    [System.Xml.XmlElement]$LicenseMetadataElement = $NuspecXml.package.metadata["license"]
    if (-not $LicenseMetadataElement) {
        $LicenseMetadataElement = $NuspecXml.CreateElement("license", $NuspecXsdNs)
        $NuspecXml.package.metadata.AppendChild($LicenseMetadataElement) | Out-Null
    }
    $LicenseMetadataElement.SetAttribute("type", "file")
    $LicenseMetadataElement.InnerText = "LICENSE"

    [System.Xml.XmlElement]$LicenseFileElement = $NuspecFilesElement.file | Where-Object -Property target -Like "LICENSE" | Select-Object -First 1
    if ($LicenseFileElement) {
        $LicenseFileElement.SetAttribute("src", $LicenseBundlePath)
    } else {
        $LicenseFileElement = $NuspecXml.CreateElement("file", $NuspecXsdNs)
        $LicenseFileElement.SetAttribute("src", $LicenseBundlePath)
        $LicenseFileElement.SetAttribute("target", "LICENSE")
        $NuspecFilesElement.InsertBefore($LicenseFileElement, $NuspecFilesElement.FirstChild) | Out-Null
    }
}
function Add-NuspecLibraryFiles {
    param(
        [Parameter(Mandatory=$true)]
        [ValidateNotNull()]
        [xml]$NuspecXml,
        [Parameter(Mandatory=$true)]
        [string]$SourceDirectory,
        [Parameter(Mandatory=$false)]
        [ValidateNotNull()]
        [string]$TargetFramework = "uap"
    )

    [System.Xml.XmlElement]$NuspecFilesElement = $NuspecXml.package.files
    if (-not $NuspecFilesElement) {
        $NuspecFilesElement = $NuspecXml.CreateElement("files", $NuspecXsdNs)
        $NuspecXml.package.AppendChild($NuspecFilesElement) | Out-Null
    }

    $PSCmdlet.WriteVerbose("Adding library files from `"$SourceDirectory`" to `"nuspec://$($NuspecXml.package.metadata.id)/$($NuspecXml.package.metadata.version)`", TFM: $TargetFramework")
    [string]$TargetPathRoot = Join-Path "lib" $TargetFramework
    Get-ChildItem -File -Recurse $SourceDirectory | ForEach-Object {
        $SourceFilePath = $_.FullName
        $SourceRelativePath = [System.IO.Path]::GetRelativePath($SourceDirectory, $SourceFilePath)
        $TargetFilePath = Join-Path $TargetPathRoot $SourceRelativePath
        [System.Xml.XmlElement]$TargetFileElement = $NuspecFilesElement.file |`
            Where-Object { $_.src -like $SourceFilePath -and $_.target -like $TargetFilePath } |`
            Select-Object -First 1
        if (-not $TargetFileElement) {
            $TargetFileElement = $NuspecXml.CreateElement("file", $NuspecXsdNs)
            $TargetFileElement.SetAttribute("src", $SourceFilePath)
            $TargetFileElement.SetAttribute("target", $TargetFilePath)
            $NuspecFilesElement.AppendChild($TargetFileElement) | Out-Null
        }
    }
}
function Get-WindowsSdkManifestVersion {
    [OutputType([version])]
    param (
        [Parameter(Mandatory=$true, Position=0)]
        [ValidateNotNullOrEmpty()]
        [string]$WindowsKitsRoot10
    )
    [xml]$SdkManifestXml = Get-Content (Join-Path $WindowsKitsRoot10 "SDKManifest.xml")
    [System.Reflection.AssemblyName]$PlatformIdentity = New-Object System.Reflection.AssemblyName @($SdkManifestXml.FileList.PlatformIdentity)
    return $PlatformIdentity.Version
}
function ConvertTo-AssemblyName {
    [OutputType([System.Reflection.AssemblyName])]
    param (
        [Parameter(Mandatory=$true, ValueFromPipeline=$true, Position=0)]
        [System.Xml.XmlElement]$ApiContract
    )
    begin {
        $DefaultVersion = "1.0.0.0"
    }
    process {
        $AssemblyName = $ApiContract.name
        $Version = $ApiContract.version
        if (-not $Version) {
            $Version = $DefaultVersion
        }
        return New-Object System.Reflection.AssemblyName @(
            "$AssemblyName, Version=$Version"
        )
    }
}
function Get-UapPlatformInformation {
    [OutputType([PSCustomObject])]
    param (
        [Parameter(Mandatory=$true, Position=0)]
        [ValidateNotNullOrEmpty()]
        [string]$RootDirectory,
        [Parameter(Mandatory=$false, Position=1)]
        [string]$SdkVersion = $null
    )
    process {
        if (-not $SdkVersion) {
            [string]$SdkVersion = Get-WindowsSdkManifestVersion $RootDirectory
        }

        [string]$UapDirectory = Join-Path $RootDirectory (Join-Path "Platforms" (Join-Path "UAP" $SdkVersion))
        [string]$UapPlatformXmlPath = Join-Path $UapDirectory "Platform.xml"
        [xml]$UapPlatformXml = Get-Content -Path $UapPlatformXmlPath

        [version]$UapPlatformVersion = $UapPlatformXml.ApplicationPlatform.version
        [string]$UapNugetTfm = "uap" + $UapPlatformVersion.ToString(3)

        return [PSCustomObject]@{
            Name=[string]($UapPlatformXml.ApplicationPlatform.name);
            FriendlyName=[string]($UapPlatformXml.ApplicationPlatform.friendlyName);
            Version=$UapPlatformVersion;
            TargetFrameworkMoniker=$UapNugetTfm;
            ReferencesDirectory=(Join-Path $RootDirectory (Join-Path "References" $SdkVersion));
            ApiContracts= $UapPlatformXml.ApplicationPlatform.ContainedApiContracts.ApiContract | ConvertTo-AssemblyName
        }
    }
}
function Get-WindowsSdkInfo {
    [OutputType([PSCustomObject])]
    param (
        [Parameter(Mandatory=$true, Position=0)]
        [ValidateNotNullOrEmpty()]
        [string]$RootDirectory,
        [Parameter(Mandatory=$false, ValueFromPipeline=$true)]
        [string]$Version = $null
    )
    process {
        if (-not $Version) {
            $Version = Get-WindowsSdkManifestVersion $RootDirectory
        }

        return [PSCustomObject]@{
            Version = [version]$Version;
            LicensesDirectory = (Join-Path $RootDirectory (Join-Path "Licenses" $Version));
        }
    }
}
function Out-UapApiContractsNuspec {
    param (
        [Parameter(Mandatory=$true, Position = 0)]
        [PSCustomObject]$UapInfo,
        [Parameter(Mandatory=$true, Position = 1)]
        [PSCustomObject]$SdkInfo,
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$TargetDirectory
    )

    $UapInfo.ApiContracts | ForEach-Object {
        [System.Reflection.AssemblyName]$AssemblyName = $_
        [string]$AssemblySourceDirectory = Join-Path $UapInfo.ReferencesDirectory `
            (Join-Path $AssemblyName.Name $AssemblyName.Version)
        [string]$NuspecDirectory = Join-Path $TargetDirectory $AssemblyName.Name
        [string]$NuspecFilePath = Join-Path $NuspecDirectory "$($AssemblyName.Name).$($AssemblyName.Version).nuspec"
        $PSCmdlet.WriteVerbose("$AssemblySourceDirectory -> $NuspecFilePath")
        [xml]$NuspecXml = Get-Content -ErrorAction "SilentlyContinue" $NuspecFilePath
        if (-not $NuspecXml) {
            $NuspecXml = New-MicrosoftNuspec -Path $NuspecFilePath
            $NuspecFilePath = Resolve-Path $NuspecFilePath
        }
        Set-NuspecMetadata -NuspecXml $NuspecXml `
            -Id $AssemblyName.Name -Version $AssemblyName.Version `
            -AppendDescription "API Contract in $($UapInfo.FriendlyName)"
        Add-NuspecLicenses -NuspecXml $NuspecXml -NuspecPath $NuspecFilePath `
            -TargetFramework $UapInfo.TargetFrameworkMoniker `
            -LicensesDirectory $SdkInfo.LicensesDirectory
        Add-NuspecLibraryFiles -NuspecXml $NuspecXml `
            -SourceDirectory $AssemblySourceDirectory `
            -TargetFramework $UapInfo.TargetFrameworkMoniker
        $NuspecXml.Save($NuspecFilePath)
    }
}

New-Item -ItemType Directory $BuildDirectory -Force | Out-Null
$NugetExecutablePath = Join-Path $BuildDirectory (Join-Path "nuget" "nuget.exe")
$NugetExecutable = $null
if (Test-Path -PathType Leaf $NugetExecutablePath) {
    $NugetExecutable = $NugetExecutablePath
} else {
    $NugetExecutable = Get-Command -ErrorAction "SilentlyContinue" "nuget" | Select-Object -First 1
}
if (-not $NugetExecutable) {
    New-Item -ItemType File $NugetExecutablePath -Force | Out-Null
    Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" `
        -OutFile $NugetExecutablePath
}
$PSCmdlet.WriteVerbose("Using nuget version $((Get-Command $NugetExecutable).Version)")

$NuspecDirectory = Join-Path $BuildDirectory "nuspec"
New-Item -ItemType Directory $NuspecDirectory -Force | Out-Null

[Microsoft.Win32.RegistryKey]$WindowsKitsInstalledRoots =  Get-Item -Path "HKLM:\SOFTWARE\Microsoft\Windows Kits\Installed Roots"
[string]$WindowsKitsRoot10 = $WindowsKitsInstalledRoots | Get-ItemPropertyValue -Name KitsRoot10
$WindowsKitsInstalledRoots.GetSubKeyNames() | Get-WindowsSdkInfo $WindowsKitsRoot10 | ForEach-Object {
    $WinSdkInfo = $_
    $UapInfo = Get-UapPlatformInformation $WindowsKitsRoot10 -SdkVersion $WinSdkInfo.Version
    $PSCmdlet.WriteVerbose([string]::Join([System.Environment]::NewLine,
        @("Universal Application Platform") + ($UapInfo | Format-List Name, Version, FriendlyName, TargetFrameworkMoniker, ReferencesDirectory | Out-String)))
    Out-UapApiContractsNuspec $UapInfo $WinSdkInfo -TargetDirectory $NuspecDirectory
    Get-ChildItem -File -Recurse -Filter "*.nuspec" | ForEach-Object {
        $NugetArgs = @(
            "pack",
            "`"$($_.FullName)`"",
            "-Force",
            "-OutputDirectory",
            "`"$OutputDirectory`""
        )
        $PSCmdlet.WriteVerbose(@("nuget") + $NugetArgs)
        & $NugetExecutable $NugetArgs | ForEach-Object { $PSCmdlet.WriteVerbose("> " + $_) }
    }
}
