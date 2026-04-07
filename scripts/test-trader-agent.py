import requests

url = "http://localhost:5001/trader"
body = {"Message": "accout =1 buy, rate 1.22, for aud/usd, trading 10000"}

response = requests.post(url, json=body)
print(response.text)
