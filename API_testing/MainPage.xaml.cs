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

        private Dictionary<string, string> PlanetTranslations = new Dictionary<string, string>
        {
            {"Mercury","Merkur" },
            { "Earth", "Země" },
            { "Mars", "Mars" },
            { "Venus", "Venuše" },
            { "Jupiter", "Jupiter" },
            { "Saturn", "Saturn" },
            { "Uranus", "Uran" },
            { "Neptune", "Neptun" }
           
        };

        private Dictionary<bool, string> Ano_ne = new Dictionary<bool, string>
        {
            {true, "Ano" },
            { false,"Ne"}

        };


        public MainPage()
        {
            InitializeComponent();

            
            _nasaApiService = new NASAApiService();
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "asteroids.db");
            _databaseService = new DatabaseService(dbPath);
            Debug.WriteLine($"Database Path: {dbPath}");

            
            Asteroids = new ObservableCollection<NearEarthObject>();

            
            BindingContext = this;

           
            
            LoadAsteroids(DateTime.UtcNow.ToString("yyyy-MM-dd"), DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd"));
        }

        private async void LoadAsteroids(string DateStart, string DateEnd)
        {
            try
            {
                
                var apiKey = "";

                var asteroidsFromApi = await _nasaApiService.GetAsteroidsAsync(DateStart, DateEnd, apiKey);

               
                await _databaseService.ClearAsteroidsAsync(); 
                foreach (var asteroid in asteroidsFromApi)
                {
                    
                    if (PlanetTranslations.ContainsKey(asteroid.OrbitingBody))
                    {
                        asteroid.OrbitingBody = PlanetTranslations[asteroid.OrbitingBody];
                    }

                    
                    await _databaseService.SaveAsteroidAsync(asteroid);
                }

              
                var asteroidsFromDb = await _databaseService.GetAsteroidsAsync();

                
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
            
            var startDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var endDate = DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd");
            Debug.WriteLine("testing");
            
            LoadAsteroids(startDate, endDate);
        }

        private async void OnFilterButtonClicked(object sender, EventArgs e)
        {
          
            string action = await DisplayActionSheet(
                "Filrovat přes:",
                "Zrušit",
                null,
                "bez Filtru",
                "Vzdálenost (nejbližší to Nejvzdálenější)",
                "Datum přiblížení (nejbližší po nejdelší)",
                "Poloměr (Největší po největší)",
                "nebezpečné (Ano nebo ne)"
            );

            if (action == "Zrušit")
                return;

           
            var filteredAsteroids = await Task.Run(() =>
            {
                switch (action)
                {
                    case "bez Filtru":
                        return Asteroids.ToList(); 

                    case "Vzdálenost (nejbližší to Nejvzdálenější)":
                        return Asteroids.OrderBy(a => a.DistanceFromEarth).ToList();

                    case "Datum přiblížení (nejbližší po nejdelší)":
                        return Asteroids.OrderBy(a => DateTime.Parse(a.ApproachDate)).ToList();

                    case "Poloměr (Největší po největší)":
                        return Asteroids.OrderByDescending(a => a.EstimatedDiameter).ToList();

                    case "nebezpečné (Ano nebo ne)":
                        return Asteroids.OrderByDescending(a => a.PotentiallyHazardous).ToList();

                    default:
                        return Asteroids.ToList(); 
                }
            });

           
            MainThread.BeginInvokeOnMainThread(() =>
            {
               
                Asteroids.Clear();
                foreach (var asteroid in filteredAsteroids)
                {
                    Asteroids.Add(asteroid);
                }
            });
        }
    }

}
