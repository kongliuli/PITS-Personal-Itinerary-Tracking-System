using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace PITS.MVP.App.Views;

public partial class MapPage : ContentPage
{
    private readonly ViewModels.MapViewModel _viewModel;

    public MapPage(ViewModels.MapViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadMapDataAsync();
    }

    private async Task LoadMapDataAsync()
    {
        var trips = await _viewModel.GetFilteredTripsAsync();

        TripMap.Pins.Clear();
        TripMap.MapElements.Clear();

        foreach (var trip in trips.Where(t => t.Location != null))
        {
            var pin = new Pin
            {
                Label = trip.ActivityType.ToString(),
                Address = trip.Description,
                Location = new Location(trip.Location!.Y, trip.Location.X),
                Type = PinType.Place
            };
            TripMap.Pins.Add(pin);
        }

        var polyline = new Polyline { StrokeColor = Colors.Blue, StrokeWidth = 3 };
        foreach (var trip in trips.OrderBy(t => t.StartedAt))
        {
            if (trip.Location != null)
                polyline.Geopath.Add(new Location(trip.Location.Y, trip.Location.X));
        }
        
        if (polyline.Geopath.Count > 1)
            TripMap.MapElements.Add(polyline);

        if (TripMap.Pins.Any())
        {
            var firstPin = TripMap.Pins.First();
            TripMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                firstPin.Location, Distance.FromKilometers(5)));
        }
    }
}
