param (
  $packageFolderPath,
  $signedPath
)

# Create directory for signed packages
New-Item $signedPath -ItemType Directory

# Get directory of expanded packages
$directories = Get-ChildItem -Path $packageFolderPath -Directory

# Repack into nuget packages (and remove folder)
ForEach ($directory in $directories) {  
  $directoryWithoutRootFolder = "{0}/*" -f $directory.FullName
  $zipFileName = "{0}.zip" -f $directory.Name
  $zipPath = Join-Path -Path $signedPath -ChildPath $zipFileName
  Compress-Archive -Path $directoryWithoutRootFolder -DestinationPath $zipPath

  $packageFileName = "{0}.nupkg" -f $directory.Name
  $packagePath = Join-Path -Path $signedPath -ChildPath $packageFileName
  Move-Item -Path $zipPath -Destination $packagePath
  Remove-Item -LiteralPath $directory.FullName -Force -Recurse
}