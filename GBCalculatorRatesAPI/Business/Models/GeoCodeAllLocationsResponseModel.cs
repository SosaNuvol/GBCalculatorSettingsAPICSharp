using Newtonsoft.Json;

namespace GBCalculatorRatesAPI.Business.Models;

public class GeoCodeAllLocationsResponseModel {
	[JsonProperty("notRanGeoCode")]
	public required int NotRanGeoCode { get; set; } = 0;

	[JsonProperty("ranGeoCode")]
	public required int RanGeoCode { get; set; } = 0;

	[JsonProperty("notSet")]
	public required int NotSet { get; set; } = 0;

	[JsonProperty("locations")]
	public required IList<LocationWithCoordinates> Locations {  get; set; }
}