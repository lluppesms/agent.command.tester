# Command Line Agent Test

This is an example project that shows how you can call an Azure AI Foundry Agent from a console app.  This give a super easy way to see how you can use these resources in just about any program.

## Setup

Create an Agent in your Azure AI Foundry, then add these two keys to your appSettings.json or your User Secrets, then run the application:

``` bash
{
	"Foundry": {
		"ProjectEndpoint": "https://xxxxxx.services.ai.azure.com/api/projects/lll-oai-service-project",
		"AgentId": "asst_xxxxxx"
	}
}
```
