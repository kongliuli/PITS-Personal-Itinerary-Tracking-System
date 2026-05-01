namespace PITS.MVP.Core.Services;

public interface IGeocodingService
{
    Task<string?> ReverseGeocodeAsync(double latitude, double longitude);
    Task<(double Latitude, double Longitude)?> GeocodeAsync(string address);
}
