db.locations.updateMany(
    { latitude: { $exists: true }, longitude: { $exists: true } },
    [
        {
            $set: {
                location: [
                    { $convert: { input: "$longitude", to: "double", onError: null, onNull: null } },
                    { $convert: { input: "$latitude", to: "double", onError: null, onNull: null } }
                ]
            }
        }
    ]
);


           // latitude: 40.2334527,
            // longitude: -111.6650843,
            // radiusInMeters: 30,

db.locations.find({
  location: {
    $geoWithin: {
      $centerSphere: [[-111.6650843, 40.2334527], 5 / 6378.1]
      // Radius in radians: radius in km / Earth's radius (approx. 6378.1 km)
    }
  }
});