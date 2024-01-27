$ErrorActionPreference = "Stop"

$rootDir = "."
$project = "$rootDir/FirstPersonFunk/FirstPersonFunk.csproj"
$thunderstoreDir = "$rootDir/thunderstore"

$buildDir = "$rootDir/FirstPersonFunk/bin/Release/net462"
$outDir = "$rootDir/out"
$thunderstorePluginDir = "$thunderstoreDir/BepInEx/plugins/FirstPersonFunk"

# Clean up everything
if (Test-Path $buildDir) {
  Remove-Item $buildDir -Recurse -Force
}

if (Test-Path $outDir) {
  Remove-Item $outDir -Recurse -Force
}

if (Test-Path $thunderstorePluginDir) {
  Remove-Item $thunderstorePluginDir -Recurse -Force
}

New-Item $buildDir -ItemType Directory -Force
New-Item $outDir -ItemType Directory -Force
New-Item $thunderstorePluginDir -ItemType Directory -Force

# Build the project
dotnet clean "$project" --configuration Release
dotnet build "$project" --configuration Release

# GitHub release
$version = (Get-Item "$buildDir/FirstPersonFunk.dll").VersionInfo.ProductVersion
$version = $version -replace '\+.*$'
Compress-Archive -Path "$buildDir/*" -DestinationPath "$outDir/plugin.zip" -Force

# Thunderstore release
Copy-Item "$buildDir/*" "$thunderstorePluginDir/" -Recurse -Force
Copy-Item "$rootDir/README.md" "$thunderstoreDir/README.md"

# Edit the Thunderstore JSON's `version_number` property to match
$manifest = "$thunderstoreDir/manifest.json"
$edited = (jq --arg version $version '.version_number = $version' $manifest)

# Write the edited JSON
# PowerShell struggles with LF with no BOM, but I will will it anyways
$lines = $edited -split "`r`n"
$stream = [System.IO.StreamWriter] $manifest

foreach ($line in $lines) {
  $stream.Write($line)
  $stream.Write("`n")
}

$stream.Close()

Compress-Archive -Path "$thunderstoreDir/*" -DestinationPath "$outDir/thunderstore.zip" -Force
