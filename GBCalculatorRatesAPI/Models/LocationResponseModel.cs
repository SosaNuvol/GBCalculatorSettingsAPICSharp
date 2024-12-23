namespace GBCalculatorRatesAPI.Models;

using Newtonsoft.Json;

public class LocationResponseModel
{
	[JsonProperty("totalCount")]
	public int TotalCount { get; set; } = 0;

	[JsonProperty("totalDistributors")]
	public int TotalDistributors { get; set; } = 0;

	[JsonProperty("totalMerchants")]
	public int TotalMerchants { get; set; } = 0;

	[JsonProperty("locations")]
	public IList<ILocation> Locations { get; set; }

	public LocationResponseModel(IList<ILocation> locations)
	{
		Locations = locations;

		TotalCount = locations.Count;
		TotalDistributors = locations.Where(loc => !string.IsNullOrEmpty(loc.BusinessCategory) && loc.BusinessCategory.Equals(LocationConstants.DISTRIBUTOR)).ToList().Count;
		TotalMerchants = locations.Where(loc => !string.IsNullOrEmpty(loc.BusinessCategory) && loc.BusinessCategory.Equals(LocationConstants.MERCHANT)).ToList().Count;
	}
}