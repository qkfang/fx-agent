param(
    [string]$StorageAccount = "fxagst",
    [string]$Container = "articles",
    [string]$Prefix = "news/"
)

$blobs = az storage blob list `
    --account-name $StorageAccount `
    --container-name $Container `
    --prefix $Prefix `
    --auth-mode login `
    --query "[].name" -o tsv

if (-not $blobs) {
    Write-Host "No blobs found under '$Prefix'."
    return
}

$count = ($blobs | Measure-Object -Line).Lines
Write-Host "Deleting $count blob(s) under '$Prefix'..."

az storage blob delete-batch `
    --source $Container `
    --account-name $StorageAccount `
    --auth-mode login `
    --pattern "${Prefix}*"

Write-Host "Done."
