using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_testing.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        public DatabaseService(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Models.NearEarthObject>().Wait();

        }

        public async Task<List<Models.NearEarthObject>> GetAsteroidsAsync()
        {
            var asteroids = await _database.Table<Models.NearEarthObject>().ToListAsync();
            Debug.WriteLine($"Retrieved {asteroids.Count} asteroids from database.");
            return asteroids;
        }

        public async Task<int> SaveAsteroidAsync(Models.NearEarthObject asteroid)
        {
            Debug.WriteLine($"Inserting asteroid: {asteroid.Name}");
            return await _database.InsertAsync(asteroid);
        }

        public Task<int> ClearAsteroidsAsync()
        {
            return _database.DeleteAllAsync<Models.NearEarthObject>();
        }
    }
}
