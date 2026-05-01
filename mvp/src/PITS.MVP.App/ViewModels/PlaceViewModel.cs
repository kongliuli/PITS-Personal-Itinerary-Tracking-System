using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetTopologySuite.Geometries;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;

namespace PITS.MVP.App.ViewModels;

public partial class PlaceViewModel : BaseViewModel
{
    private readonly IPlaceService _placeService;

    [ObservableProperty] private string _newPlaceName = "";
    [ObservableProperty] private PlaceCategory _selectedCategory = PlaceCategory.Other;
    
    public ObservableCollection<Place> Places { get; } = new();
    public List<PlaceCategory> Categories { get; } = Enum.GetValues<PlaceCategory>().ToList();

    public PlaceViewModel(IPlaceService placeService)
    {
        _placeService = placeService;
        Title = "地点管理";
    }

    public async Task InitializeAsync()
    {
        await LoadPlacesAsync();
    }

    private async Task LoadPlacesAsync()
    {
        await ExecuteAsync(async () =>
        {
            Places.Clear();
            var places = await _placeService.GetAllAsync();
            foreach (var place in places)
            {
                Places.Add(place);
            }
        });
    }

    [RelayCommand]
    private async Task AddPlaceAsync()
    {
        if (string.IsNullOrWhiteSpace(NewPlaceName)) return;

        var location = await Geolocation.GetLocationAsync(
            new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10)));

        if (location != null)
        {
            var place = new Place
            {
                Name = NewPlaceName,
                Category = SelectedCategory,
                Location = new Point(location.Longitude, location.Latitude) { SRID = 4326 },
                Radius = 200
            };

            await _placeService.AddAsync(place);
            NewPlaceName = "";
            await LoadPlacesAsync();
        }
    }

    [RelayCommand]
    private async Task DeletePlaceAsync(Place place)
    {
        await _placeService.DeleteAsync(place.Id);
        await LoadPlacesAsync();
    }
}
