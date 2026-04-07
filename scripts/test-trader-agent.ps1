$url = "http://localhost:5001/trader"
$body = @{ Message = "accout =1 buy, rate 1.22, for aud/usd, trading 10000" } | ConvertTo-Json

$response = Invoke-RestMethod -Uri $url -Method Post -Body $body -ContentType "application/json"
Write-Output $response
