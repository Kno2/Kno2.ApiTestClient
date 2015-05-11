#C:\Program Files (x86)\Git\cmd\git.exe
$script:GitPath = 'C:\Program Files (x86)\Git\cmd\git.exe'

task Get-Version {
  $versionText = getVersion
  Write-Host "Version: $versionText"
  New-Item (Join-Path $artifacts_dir "VERSION") -type file -force -value "$versionText"
}

task Get-ReleaseNotes {
  Write-Host "Building release notes"
  $releaseNotes = getReleaseNotes
  if($releaseNotes){
    $releateNotesPath = getReleaseNotesPath
    New-Item $releateNotesPath -type file -force -value "## Release Notes ##`n"
    foreach($line in $releaseNotes){
      Add-Content $releateNotesPath $line
    }
  } else {
    Write-Host "No release notes available" -foregroundcolor red -backgroundcolor yellow
  }
}

function getVersion {
  $gitPath = $script:GitPath;
  $describeText = (& "$gitPath" --no-pager describe --long --tags --always --abbrev=10);
  $describe = ($describeText -split "-");
  $version = (($describe[0]) + "." + (($describe[1], '0', 1 -ne $null)[0])) -replace "v.", "";
  if($version -match '\d.\d.\d'){
    return $version;
  }else{
    return '0.0.0'
  }
}

function getReleaseNotesPath() {
  return (Join-Path $artifacts_dir "ReleaseNotes.md")
}

function getReleaseNotes {
  $gitPath = $script:GitPath;
  $tag = (& "$gitPath" --no-pager describe --tags --always --abbrev=0);
  write-host "Pulling since tag $tag"
  $releaseNotes = (& "$gitPath" --no-pager log "$tag..HEAD" --pretty=format:" - %h %s");
  if($releaseNotes) {
    return $releaseNotes
  } else {
    Write-Host "No tags"
    return (& "$gitPath" --no-pager log --pretty=format:" - %h %s");
  }
}

function getArtifactsPath() {
  $version = getVersion
  $artifacts_path = Join-Path $artifacts_dir "$solution_name`_$version`_$build_level"
  return Create-Directory $artifacts_path;
}

function getHash() {
  $hash = (& "$gitPath" rev-parse --short HEAD);
  return $hash
}
