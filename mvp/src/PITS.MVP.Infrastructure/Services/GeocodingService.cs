using System.Text.Json;
using PITS.MVP.Core.Services;

namespace PITS.MVP.Infrastructure.Services;

public class GeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private const string NominatimBaseUrl = "https://nominatim.openstreetmap.org";

    public GeocodingService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "PITS-MVP/1.0");
    }

    public async Task<string?> ReverseGeocodeAsync(double latitude, double longitude)
    {
        try
        {
            var url = $"{NominatimBaseUrl}/reverse?format=json&lat={latitude}&lon={longitude}&zoom=18&addressdetails=1";
            var response = await _httpClient.GetStringAsync(url);
            var result = JsonSerializer.Deserialize<NominatimReverseResponse>(response);
            return result?.DisplayName ?? result?.Address?.ToString();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(double Latitude, double Longitude)?> GeocodeAsync(string address)
    {
        try
        {
            var url = $"{NominatimBaseUrl}/search?format=json&q={Uri.EscapeDataString(address)}&limit=1";
            var response = await _httpClient.GetStringAsync(url);
            var results = JsonSerializer.Deserialize<NominatimSearchResponse[]>(response);
            
            if (results != null && results.Length > 0)
            {
                return (results[0].Lat, results[0].Lon);
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    private class NominatimReverseResponse
    {
        public string? DisplayName { get; set; }
        public NominatimAddress? Address { get; set; }
    }

    private class NominatimSearchResponse
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    private class NominatimAddress
    {
        public string? Road { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }
}
