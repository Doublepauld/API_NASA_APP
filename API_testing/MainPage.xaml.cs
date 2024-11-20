using API_testing.Models;
using API_testing.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;


namespace API_testing
{
    public partial class MainPage : ContentPage
    {
        private readonly NASAApiService _nasaApiService;
        private readonly DatabaseService _databaseService;
        public ObservableCollection<NearEarthObject> Asteroids { get; set; }

        public MainPage()
        {
            InitializeComponent();

            // Initialize services
            _nasaApiService = new NASAApiService();
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "asteroids.db");
            _databaseService = new DatabaseService(dbPath);
            Debug.WriteLine($"Database Path: {dbPath}");

            // Initialize collection for binding
            Asteroids = new ObservableCollection<NearEarthObject>();

            // Bind to UI
            BindingContext = this;

            // Load data
            
            LoadAsteroids(DateTime.UtcNow.ToString("yyyy-MM-dd"), DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd"));
        }

        private async void LoadAsteroids(string DateStart, string DateEnd)
        {
            try
            {
                // Fetch data from API
                var apiKey = "wuKarJ47whqhzHh3GVkBBDHAEStFpJhEdOHlBqbF";
                
                var asteroidsFromApi = await _nasaApiService.GetAsteroidsAsync(DateStart, DateEnd, apiKey);

                // Store data in SQLite
                await _databaseService.ClearAsteroidsAsync(); // Clear old data
                foreach (var asteroid in asteroidsFromApi)
                {
                    await _databaseService.SaveAsteroidAsync(asteroid);
                }

                // Load data from SQLite to display
                var asteroidsFromDb = await _databaseService.GetAsteroidsAsync();

                // Update ObservableCollection
                Asteroids.Clear();
                foreach (var asteroid in asteroidsFromDb)
                {
                    Asteroids.Add(asteroid);
                }

                Debug.WriteLine("Data loaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void OnRefreshButtonClicked(object sender, EventArgs e)
        {
            // Calculate most recent dates
            var startDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var endDate = DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd");
            Debug.WriteLine("testing");
            // Reload asteroids with the new date range
            LoadAsteroids(startDate, endDate);
        }

        private async void OnFilterButtonClicked(object sender, EventArgs e)
        {
            // Display an action sheet for the filter options
            string action = await DisplayActionSheet(
                "Filter by:",
                "Cancel",
                null,
                "No Filter (Default)",
                "Distance (Closest to Furthest)",
                "Date of Approach (Soonest to Latest)",
                "Diameter (Biggest to Smallest)",
                "Hazardous (True to False)"
            );

            if (action == "Cancel")
                return;

            // Use Task.Run to perform the filtering in the background
            var filteredAsteroids = await Task.Run(() =>
            {
                switch (action)
                {
                    case "No Filter (Default)":
                        return Asteroids.ToList(); // Return a List, not ObservableCollection

                    case "Distance (Closest to Furthest)":
                        return Asteroids.OrderBy(a => a.DistanceFromEarth).ToList();

                    case "Date of Approach (Soonest to Latest)":
                        return Asteroids.OrderBy(a => DateTime.Parse(a.ApproachDate)).ToList();

                    case "Diameter (Biggest to Smallest)":
                        return Asteroids.OrderByDescending(a => a.EstimatedDiameter).ToList();

                    case "Hazardous (True to False)":
                        return Asteroids.OrderByDescending(a => a.PotentiallyHazardous).ToList();

                    default:
                        return Asteroids.ToList(); // Default to no filter
                }
            });

            // After filtering, update the ObservableCollection on the main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Clear current list and add the filtered items
                Asteroids.Clear();
                foreach (var asteroid in filteredAsteroids)
                {
                    Asteroids.Add(asteroid);
                }
            });
        }
    }

}
