using System.Text.Json;
using Newtonsoft.Json;

namespace GBCalculatorRatesAPI.Models;

public interface IGBGeoCodes {
	double Latitude { get; }

	double Longitude { get; }
}

public class GBGeoCodes: IGBGeoCodes {
	
	[JsonProperty("latitude")]
	public double Latitude { get; set; }

	[JsonProperty("longitude")]
	public double Longitude { get; set; }
}