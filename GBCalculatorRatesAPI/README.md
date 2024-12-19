# API CSharp Application

This application was written to replace the TypeScript one.

## Deployment

Looks like Azure is unpredictable.  So I tried to deploy using the plugin and nothing happened.  The changes did not take effect.  For this reason I deployed it manually using the following commands.

### 1. Build the project

Be in the root of the project

```terminal
% dotnet build --configuration Release
```

### 2. Publish the project

This creates a deployment release.  Not really sure what it does.

```terminal
dotnet publish --configuration Release
```

### 3. Create a zip file

```terminal
cd bin/Release/net8.0/publish
zip -r ../publish.zip *
```

### 4. Deploy to Azure

This is the command line version.  According to CoPilot this is the statement:

```terminal
az functionapp deployment source config-zip \
  --resource-group <YourResourceGroup> \
  --name <YourFunctionAppName> \
  --src <PathToYourZipFile>
```

Below is what I did that crashed the functions in Azure.  But later when I deployed it with the VS Code plugin it worked.

```terminal
az functionapp deployment source config-zip \
  --resource-group RG-GBCalculator \
  --name GBCalculatorSettingsAPICSharp \
  --src ./bin/Release/net8.0/puglish.zip
```
