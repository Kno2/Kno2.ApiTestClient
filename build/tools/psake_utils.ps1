function ZipFiles($zipfilename, $sourcedir)
{
	Write-Host $zipfilename
	$targetDirectory = Split-Path -Parent $zipfilename
	
	Create-Directory($targetDirectory)
	
	# No overwrite flag with CreateFromDirectory 
	if(Test-Path -Path $zipfilename){
		Remove-Item -Force -Recurse $zipfilename -ErrorAction SilentlyContinue
	}
	
	[Reflection.Assembly]::LoadWithPartialName("System.IO.Compression.FileSystem") | Out-Null
	$compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
	[System.IO.Compression.ZipFile]::CreateFromDirectory($sourcedir,$zipfilename, $compressionLevel, $false)
}

function Delete-Directory($directoryName){
	if ((Test-Path -path $directoryName) -eq $True){
		Remove-Item -Force -Recurse $directoryName -ErrorAction SilentlyContinue
	}
}
 
function Create-Directory($directoryName){
	if ((Test-Path -path $directoryName) -eq $False){
		New-Item $directoryName -ItemType Directory | Out-Null
	}
	return Resolve-Path -Path $directoryName
}

function Generate-Assembly-Info{

	param(
	[string]$title,
	[string]$description,
	[bool]$clsCompliant = $false,
	[string]$internalsVisibleTo = "",
	[string]$configuration,
	[string]$company,
	[string]$product,
	[string]$copyright,
	[string]$version,
	[string]$fileVersion,
	[string]$infoVersion,
	[string]$trademark,
	[string]$culture,	
	[string]$file = $(throw "file is a required parameter.")
	)
	
	if($infoVersion -eq ""){
		$infoVersion = $fileVersion
	}

	$asmInfo = "using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

[assembly: AssemblyTitle(""$title"")]
[assembly: AssemblyDescription(""$description"")]
[assembly: AssemblyConfiguration(""$configuration"")]
[assembly: AssemblyCompany(""$company"")]
[assembly: AssemblyProduct(""$product"")]
[assembly: AssemblyCopyright(""$copyright"")]
[assembly: AssemblyTrademark(""$trademark"")]
[assembly: AssemblyCulture(""$culture"")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion(""$version"")]
[assembly: AssemblyFileVersion(""$fileVersion"")]
[assembly: AssemblyInformationalVersion(""$infoVersion"")]
"

	if($clsCompliant -eq $true){
		$asmInfo += "[assembly: CLSCompliantAttribute("+$clsCompliant.ToString().ToLower()+")]" + [Environment]::NewLine
	}

	if($internalsVisibleTo -ne ""){
		$asmInfo += "[assembly: InternalsVisibleTo(""$internalsVisibleTo"")]" + [Environment]::NewLine	
	}

	$dir = [System.IO.Path]::GetDirectoryName($file)

	if ([System.IO.Directory]::Exists($dir) -eq $false)
	{
		Write-Host "Creating directory $dir"
		[System.IO.Directory]::CreateDirectory($dir)
	}
	
	Write-Host "Generating assembly info file: $file"
	Write-Output $asmInfo > $file
}
