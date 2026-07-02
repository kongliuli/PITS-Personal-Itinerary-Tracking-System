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
        else if (e.PropertyName == nameof(MapViewModel.ShowHeatmap) ||
                 e.PropertyName == nameof(MapViewModel.HeatmapPoints))
        {
            MainThread.BeginInvokeOnMainThread(RenderHeatmap);
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
                Address = trip.Description ?? string.Empty,
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

    private void RenderHeatmap()
    {
        // 清除现有的热力图 Circle 标记
        var circlesToRemove = TripMap.MapElements.OfType<Polygon>().ToList();
        foreach (var circle in circlesToRemove)
        {
            TripMap.MapElements.Remove(circle);
        }

        if (!_viewModel.ShowHeatmap) return;

        foreach (var hp in _viewModel.HeatmapPoints)
        {
            // 密度越高颜色越深（从黄到红）
            var color = GetHeatmapColor(hp.Intensity);
            var circle = new Polygon
            {
                StrokeColor = color,
                FillColor = color,
                StrokeWidth = 2
            };

            // 生成圆形近似点
            var center = hp.Location;
            var radiusDeg = hp.Radius / 111000.0; // 近似转换米到度
            for (int i = 0; i <= 36; i++)
            {
                var angle = 2 * Math.PI * i / 36;
                var lat = center.Latitude + radiusDeg * Math.Sin(angle);
                var lon = center.Longitude + radiusDeg * Math.Cos(angle) / Math.Cos(center.Latitude * Math.PI / 180);
                circle.Geopath.Add(new Location(lat, lon));
            }

            TripMap.MapElements.Add(circle);
        }
    }

    private static Color GetHeatmapColor(double intensity)
    {
        // 从黄色(低密度)到红色(高密度)，带透明度
        var r = 255;
        var g = (int)(255 * (1 - intensity));
        var b = 0;
        var alpha = 0.3 + intensity * 0.5;
        return Color.FromRgba(r, g, b, alpha);
    }
}
