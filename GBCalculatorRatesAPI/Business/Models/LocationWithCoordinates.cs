namespace GBCalculatorRatesAPI.Business.Models;

public class LocationWithCoordinates
{
	public required string LocationID { get; set; }
	public required string Address { get; set; }
	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public required string Status { get; set; }
}