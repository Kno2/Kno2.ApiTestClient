function ilmerge() {
	param(
		[string]$ilmergePath = $(throw "ilmergePath path is required"),
		[string]$mergedexe = $(throw "mergedexe path is required"),
		[string]$sourceExe = $(throw "source executable file is required"),
		[string[]]$sourceDlls = $(throw "source dlls files are required")
	)

	& $ilmergePath /v4 /out:$mergedexe /internalize $sourceExe $sourceDlls
}
