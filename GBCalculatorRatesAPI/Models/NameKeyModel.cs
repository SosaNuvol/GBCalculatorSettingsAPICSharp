using System.Text.Json;
using Newtonsoft.Json;

namespace GBCalculatorRatesAPI.Models;

public class NameKeyModel {
	[JsonProperty("name")]
	public required string Name { get; set; }

	[JsonProperty("key")]
	public required string Key { get; set; }
}