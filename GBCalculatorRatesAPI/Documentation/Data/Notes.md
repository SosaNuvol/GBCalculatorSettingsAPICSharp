# Notes on How to Run Data

## December 6th

I created a new DB on Azure that I hope will be able to do the GeoCoding and searching we are looking for.

Ran the geocoding with the new data from Spencer.  Here is the result:

```text
[2024-12-06T15:18:27.733Z]       || ** Not Ran:   68
[2024-12-06T15:18:27.734Z]       || **     Ran: 1965
[2024-12-06T15:18:27.735Z]       || ** Not Set:    0
```

The total amount of records should be 2033.

### Updating the New Cluster DB

So the issue is that some of the distributors that did not have addresses the latitude and longitude properties were not created so when I try to run the command to create the location property which contains the Geo Coordinates for searching an error gets thrown.

Here is the command:

This command creates the property location in the locations collection.  As you can see this is of type "Point".

```mongodb
db.locations.updateMany(
    {},
    {
        $set: {
            location: {
                type: "Point",
                coordinates: ["$longitude", "$latitude"]
            }
        }
    }
);
```

So I need to create the long and lati properties for this command to work for all locations.  This means I need to run a command that will create the properties for all documents and give it a value of 0 or null.

```mongodb
db.locations.updateMany(
    { $or: [ { latitude: { $exists: false } }, { longitude: { $exists: false } } ] },
    { $set: { latitude: null, longitude: null } }
);
```

I got this error when trying to update the db location property:

```txt
Mongo Server error (MongoCommandException): Command failed with error 16755 (Location16755): 'Can't extract geo keys { _id: { "$oid" : "675262625caca36f19a28def" } } Point must only contain numeric elements' on server fc-f5b885442566-000.global.mongocluster.cosmos.azure.com:10260. 

The full response is:
{
    "ok" : 0.0,
    "errmsg" : "Can't extract geo keys { _id: { \"$oid\" : \"675262625caca36f19a28def\" } } Point must only contain numeric elements",
    "code" : 16755.0,
    "codeName" : "Location16755"
}
```

Thw query I ran was this:

```mongodb

db.locations.aggregate([
    {
        $addFields: {
            location: {
                type: "Point",
                coordinates: ["$longitude", "$latitude"]
            }
        }
    },
    {
        $merge: {
            into: "locations",
            whenMatched: "merge",
            whenNotMatched: "fail"
        }
    }
]);

```

Now I'm going to try this command:

```mongodb
db.locations.updateMany(
    { longitude: { $type: "double" }, latitude: { $type: "double" } },
    [
        {
            $set: {
                location: {
                    type: "Point",
                    coordinates: ["$longitude", "$latitude"]
                }
            }
        }
    ]
);
```

This worked with a match of 1965 of 2033.  Leaving 68 that were not modified.

### Steps To Configuring the Cluster MongoDB on Azure

1. I connected to it first.  Make sure you replace the password.
2. I have to create a new property called `location` for all documents that have a longitude and latitude coordinate.

   This is a `Point` type that contains an array of `double` types.

   Below is the command:

  The assumption here is that the data has already been loaded from the googlesheets.

  ```mongodb
  db.locations.aggregate([
      {
          $addFields: {
              location: {
                  type: "Point",
                  coordinates: ["$longitude", "$latitude"]
              }
          }
      },
      {
          $merge: {
              into: "locations",
              whenMatched: "merge",
              whenNotMatched: "fail"
          }
      }
  ]);
  ```

1. Create the index that will help with the query.  Below is the command:

Make sure that you create the `location` property in the documents first.

```mongodb
db.locations.createIndex({ location: "2dsphere" });
```

This creates an index called `location_2dsphere`.
