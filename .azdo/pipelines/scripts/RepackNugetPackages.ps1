param (
  $packageFolderPath,
  $signedPath
)

# Create directory for signed packages
New-Item $signedPath -ItemType Directory -Force | Out-Null

# Get directory of expanded packages
$directories = Get-ChildItem -Path $packageFolderPath -Directory

# Repack into nuget packages (and remove folder)
Add-Type -AssemblyName System.IO.Compression

ForEach ($directory in $directories) {
  $packageFileName = "{0}.nupkg" -f $directory.Name
  $packagePath = Join-Path -Path $signedPath -ChildPath $packageFileName

  $fileStream = [System.IO.File]::Open($packagePath, [System.IO.FileMode]::Create)

  try {
    $archive = [System.IO.Compression.ZipArchive]::new($fileStream, [System.IO.Compression.ZipArchiveMode]::Create, $false)

    try {
      Get-ChildItem -Path $directory.FullName -Recurse -File |
        Sort-Object FullName |
        ForEach-Object {
          $relativePath = $_.FullName.Substring($directory.FullName.Length + 1)
          # NuGet package entries must use forward slashes so restore works on Unix filesystems.
          $entryName = $relativePath -replace '\\', '/'
          $entry = $archive.CreateEntry($entryName, [System.IO.Compression.CompressionLevel]::Optimal)
          $entryStream = $entry.Open()
          $inputStream = [System.IO.File]::OpenRead($_.FullName)

          try {
            $inputStream.CopyTo($entryStream)
          }
          finally {
            $inputStream.Dispose()
            $entryStream.Dispose()
          }
        }
    }
    finally {
      $archive.Dispose()
    }
  }
  finally {
    $fileStream.Dispose()
  }

  Remove-Item -LiteralPath $directory.FullName -Force -Recurse
}
