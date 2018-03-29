param(
    [Parameter(Mandatory = $true)]
    [string]$ManifestFile
)


[xml]$xml = Get-Content -Path $ManifestFile



#$version = $xml.Package.Identity.Version.Clone()
#$ver = [System.Version]::Parse($version)
#$ver = New-Object -TypeName System.Version -ArgumentList ($ver.Major.ToString() +"."+ $ver.Minor.ToString()+"."+$ver.Build.ToString()+"."+($ver.Revision+1).ToString())
#$xml.Package.Identity.Version = $ver.ToString()



$id = $xml.Package.Applications.Application.Id
$app = $xml.Package.Applications.Application.Clone()
For ($i=1; $i -le 9; $i++) {
    $appL = $app.Clone()
$appL.Id = $id + $i.ToString()
$xml.Package.Applications.AppendChild($appL)
}



$xml.Save($ManifestFile)


Add-AppxPackage -Register $ManifestFile