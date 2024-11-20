using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using API_testing.Models;
using System.Diagnostics;
using System.IO;

namespace API_testing.Services
{
    public class NASAApiService
    {
        private readonly HttpClient _httpClient;

        public NASAApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<NearEarthObject>> GetAsteroidsAsync(string startDate, string endDate, string apiKey)
        {
            var url = $"https://api.nasa.gov/neo/rest/v1/feed?start_date={startDate}&end_date={endDate}&api_key={apiKey}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonDocument.Parse(json);

                var asteroids = data.RootElement
                    .GetProperty("near_earth_objects")
                    .EnumerateObject()
                    .SelectMany(day => day.Value.EnumerateArray())
                    .Select(neo => new NearEarthObject
                    {
                        Name = neo.GetProperty("name").GetString(),
                        Id = int.Parse(neo.GetProperty("id").GetString()),
                        EstimatedDiameter = GetDoubleValue(neo.GetProperty("estimated_diameter").GetProperty("kilometers").GetProperty("estimated_diameter_max")),
                        DistanceFromEarth = GetDoubleValue(neo.GetProperty("close_approach_data")[0].GetProperty("miss_distance").GetProperty("kilometers")),
                        PotentiallyHazardous = neo.GetProperty("is_potentially_hazardous_asteroid").GetBoolean(),
                        ApproachDate = neo.GetProperty("close_approach_data")[0].GetProperty("close_approach_date").GetString(),
                        OrbitingBody = neo.GetProperty("close_approach_data")[0].GetProperty("orbiting_body").GetString()
                    })
                    .ToList();

                return asteroids;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Request Failed - Status Code: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                throw new Exception($"API request failed with status: {response.StatusCode}");
            }
        }

        // Helper method to handle type mismatches
        private double GetDoubleValue(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                return element.GetDouble();
            }
            else if (element.ValueKind == JsonValueKind.String && double.TryParse(element.GetString(), out double result))
            {
                return result;
            }
            else
            {
                throw new Exception("Invalid data type for numeric value");
            }
        }
    }
}
