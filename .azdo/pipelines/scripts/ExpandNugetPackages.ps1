param (
  $packagePath
)

$packages = Get-ChildItem -Path $packagePath -Filter '*.nupkg' 

ForEach ($package in $packages) {
  $renamedPath = "{0}/{1}.zip" -f $packagePath, $package.BaseName
  Move-Item -Path $package.FullName -Destination $renamedPath

  $destPath = Join-Path -Path $packagePath -ChildPath $package.BaseName
  Expand-Archive -Path $renamedPath -DestinationPath $destPath
  Remove-Item -Path $renamedPath
}