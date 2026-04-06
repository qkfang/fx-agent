

- minimal output for logging and description of the code changes
- don't include any markdown files
- don't include any unit test
- keep the code as readable and simple as possible
- never include any business name or application name from customer
- make sure remove any sensitive information in mock data
- any sample or mock telemetry data must follow open telemetry schema
- the fabric sql database is :  "FxDatabase": "Data Source=zylcdhpgv7uezc6dy7d3ngcwyi-b5l3uoo37ijuxbntne4gq2ska4.database.fabric.microsoft.com,1433;Initial Catalog=fx_data_sqldb-af3802bf-c4ca-4c83-aa5a-366c574104d4;Multiple Active Result Sets=False;Connect Timeout=30;Encrypt=True;Trust Server Certificate=False;Authentication=Active Directory Default"


- must only use microsoft agent framework 1.0, and foundry agent server v2 
- agent interaction must use openai responeses apis only 
- MAF documentation here: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/02-agents/AgentsWithFoundry
- MCP is model context protocol using .net SDK, it is hosted via remote server, and consume by mcp client