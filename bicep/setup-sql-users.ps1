$token = (az account get-access-token --resource https://database.windows.net/ --query accessToken -o tsv)

Invoke-Sqlcmd -ServerInstance "zylcdhpgv7uezc6dy7d3ngcwyi-b5l3uoo37ijuxbntne4gq2ska4.database.fabric.microsoft.com,1433" -Database "fx_data_sqldb-af3802bf-c4ca-4c83-aa5a-366c574104d4" -AccessToken $token -Query @"
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'sp-demo-01')
    CREATE USER [sp-demo-01] FROM EXTERNAL PROVIDER;
ALTER ROLE db_owner ADD MEMBER [sp-demo-01];
"@
