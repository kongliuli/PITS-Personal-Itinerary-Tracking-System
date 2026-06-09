using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using PITS.MVP.App.ViewModels;

namespace PITS.MVP.App.Views;

public partial class MapPage : ContentPage
{
    private readonly MapViewModel _viewModel;

    public MapPage(MapViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MapViewModel.UseSpeedColors) ||
            e.PropertyName == nameof(MapViewModel.SpeedColoredSegments))
        {
            MainThread.BeginInvokeOnMainThread(RenderPolylines);
        }
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

        if (_viewModel.UseSpeedColors && _viewModel.SpeedColoredSegments.Count > 0)
        {
            RenderSpeedColoredPolylines();
        }
        else
        {
            RenderSinglePolyline(trips);
        }

        if (TripMap.Pins.Any())
        {
            var firstPin = TripMap.Pins.First();
            TripMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                firstPin.Location, Distance.FromKilometers(5)));
        }
    }

    private void RenderSinglePolyline(IEnumerable<Core.Entities.Trip> trips)
    {
        var polyline = new Polyline { StrokeColor = Colors.Blue, StrokeWidth = 3 };
        foreach (var trip in trips.OrderBy(t => t.StartedAt))
        {
            if (trip.Location != null)
                polyline.Geopath.Add(new Location(trip.Location.Y, trip.Location.X));
        }

        if (polyline.Geopath.Count > 1)
            TripMap.MapElements.Add(polyline);
    }

    private void RenderSpeedColoredPolylines()
    {
        foreach (var segment in _viewModel.SpeedColoredSegments)
        {
            if (segment.Locations.Count < 2) continue;

            var polyline = new Polyline
            {
                StrokeColor = segment.Color,
                StrokeWidth = 4
            };

            foreach (var loc in segment.Locations)
            {
                polyline.Geopath.Add(loc);
            }

            TripMap.MapElements.Add(polyline);
        }
    }

    private void RenderPolylines()
    {
        // 清除现有的 Polyline 元素（保留 Pin）
        var elementsToRemove = TripMap.MapElements.OfType<Polyline>().ToList();
        foreach (var element in elementsToRemove)
        {
            TripMap.MapElements.Remove(element);
        }

        if (_viewModel.UseSpeedColors && _viewModel.SpeedColoredSegments.Count > 0)
        {
            RenderSpeedColoredPolylines();
        }
        else
        {
            // 重新绘制单色轨迹
            var polyline = new Polyline { StrokeColor = Colors.Blue, StrokeWidth = 3 };
            foreach (var segment in _viewModel.SpeedColoredSegments)
            {
                foreach (var loc in segment.Locations)
                {
                    if (!polyline.Geopath.Contains(loc))
                        polyline.Geopath.Add(loc);
                }
            }
            if (polyline.Geopath.Count > 1)
                TripMap.MapElements.Add(polyline);
        }
    }
}
