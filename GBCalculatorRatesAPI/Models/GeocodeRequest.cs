namespace GBCalculatorRatesAPI.Models;

using GBCalculatorRatesAPI.Business.Models;
using Newtonsoft.Json;
using static GBCalculatorRatesAPI.Utilities.Tools;

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

public class GeoCodeResponseV3: GeoCodeRequest {

	[JsonProperty("queryResult")]
	public required GeoSearchQueryResult QueryResult { get; set; }
}

public class GeoCodeResponse: GeoCodeRequest {
	[JsonProperty("totalCount")]
	public int TotalCount { get; set; } = 0;

	[JsonProperty("payload")]
	public IList<LocationDbEntity> Payload { get; set; } = new List<LocationDbEntity>();

	public bool GenerateCenterPoint(List<GBGeoCodes> pointsList) {
		CenterPoint centerPoint;

		if (!Utilities.Tools.GenerateCenterPointTry(pointsList, out centerPoint)) return false;

		// Assuming you have a property or method to set the center point
		SetCenterPoint(centerPoint.Latitude, centerPoint.Longitude);

		return true;
	}

	private void SetCenterPoint(double latitude, double longitude) {
		this.Longitude = longitude;
		this.Latitude = latitude;
		RadiusInMeters = 0;
	}
}