using Newtonsoft.Json;

namespace GBCalculatorRatesAPI.Models;

public class GeocodeRequest
{
    [JsonProperty("latitude")]
	public double Latitude { get; set; }

	[JsonProperty("longitude")]
    public double Longitude { get; set; }

	[JsonProperty("radiusInMeters")]
    public double RadiusInMeters { get; set; }

	[JsonProperty("zipCodes")]
	public string? ZipCodes { get; set; }
}