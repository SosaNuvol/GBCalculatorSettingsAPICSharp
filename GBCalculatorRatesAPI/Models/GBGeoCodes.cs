using System.Text.Json;
using Newtonsoft.Json;

namespace GBCalculatorRatesAPI.Models;

public interface IGBGeoCodes {
	double Latitude { get; }

	double Longitude { get; }

	string? FormattedAddress { get; }

	string? CompoundCode { get; }

	string? GlobalCode { get; }

	string? Status { get; }
}

public class GBGeoCodes: IGBGeoCodes {
	
	[JsonProperty("latitude")]
	public double Latitude { get; set; }

	[JsonProperty("longitude")]
	public double Longitude { get; set; }

	[JsonProperty("formattedAddress")]
	public string? FormattedAddress { get; set; }

	[JsonProperty("compoundCode")]
	public string? CompoundCode { get; set; }

	[JsonProperty("globalCode")]
	public string? GlobalCode { get; set; }

	[JsonProperty("status")]
	public string? Status { get; set; }
}