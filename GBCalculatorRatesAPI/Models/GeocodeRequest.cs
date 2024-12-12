namespace GBCalculatorRatesAPI.Models;

using GBCalculatorRatesAPI.Business.Models;
using Newtonsoft.Json;

public class GeoCodeRequest
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

public class GeoCodeResponse: GeoCodeRequest {
	[JsonProperty("totalCount")]
	public int TotalCount { get; set; } = 0;

	[JsonProperty("payload")]
	public IList<LocationDbEntity> Payload { get; set; } = new List<LocationDbEntity>();

	public bool GenerateCenterPoint(List<GBGeoCodes> pointsList) {
		if (pointsList == null || pointsList.Count == 0) {
			return false;
		}

		double totalLatitude = 0;
		double totalLongitude = 0;

		foreach (var point in pointsList) {
			totalLatitude += point.Latitude;
			totalLongitude += point.Longitude;
		}

		double centerLatitude = totalLatitude / pointsList.Count;
		double centerLongitude = totalLongitude / pointsList.Count;

		// Assuming you have a property or method to set the center point
		SetCenterPoint(centerLatitude, centerLongitude);

		return true;
	}

	private void SetCenterPoint(double latitude, double longitude) {
		this.Longitude = longitude;
		this.Latitude = latitude;
		RadiusInMeters = 0;
	}
}