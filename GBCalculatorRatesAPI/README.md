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

This is for Development:

```terminal
az functionapp deployment source config-zip \
  --resource-group RG-GBCalculator \
  --name GBCalculatorSettingsAPICSharp \
  --src ./bin/Release/net8.0/puglish.zip
```

## Azure Portal Access

### Credentials

- UID: Goldbackdev@outlook.com
- PWD: G0ldb@ck!2024

### Mongod DB Credentials

- UID: gbMongoAdmin
- PWD: M0r0ni10ThreeFive
- URI: mongodb+srv://gbMongoAdmin:M0r0ni10ThreeFive@gbcalculatormongodbclusterprod01.global.mongocluster.cosmos.azure.com/?tls=true&authMechanism=SCRAM-SHA-256&retrywrites=false&maxIdleTimeMS=120000

### API Management

- Domain: gbdomainapi.xyz
- Sub-domain: gbcapi.gbdomainapi.xyz

## Mongo DB Location Collection Setup

This collection needs to be setup.  Here are the steps

1. Create the Collection
2. Create the location index
3. Populate the loction property that will be used by the index
4. Test that the index is working by calling queries

### Create the Collection

This is easy by calling the end point 

## TroubleShooting MongoDB

Below are the scripts I used:

```mongo
db.locations.updateMany(
  {}, // Filter: Match all documents
  { 
    $set: { 
      businessLogoFileUrl: "https://gbcloudstore.blob.core.windows.net/goldback-images/gb_gold_on_black.jpeg" 
    }
  }
);

db.locations.createIndex({ "location": "2dsphere" });

db.locations.updateMany(
  { 
    longitude: { $exists: true }, 
    latitude: { $exists: true } 
  },
  [
    {
      $set: {
        "location": {
          type: "Point",
          coordinates: ["$longitude", "$latitude"] // Use existing longitude and latitude
        }
      }
    }
  ]
);


db.locations.find(
			{
				"location": {
					"$near": {
						"$geometry": {
							"type": "Point",
							"coordinates": [-81.14579839999999, 28.66048]
						},
						"$maxDistance": 30000
					}
				}
			}
);

db.locations.find({ _id: { "$oid" : "676acf03d723a791dd2e5854" } })
```
