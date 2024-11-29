namespace GBCalculatorRatesAPI.Repos;

using GBCalculatorRatesAPI.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LocationsRepository
{
	private readonly IMongoCollection<Location> _locationsCollection;
	private readonly IConfiguration _configuration;
	private readonly string _connectionString;
	private readonly string _dbName;
	private readonly string _collectionName;

	public LocationsRepository(IConfiguration configuration)
	{
		_configuration = configuration;
		_connectionString = _configuration["MONGODB_URI"] ?? throw new Exception("MONGODB_URI not set up in environment");
		_dbName = _configuration["MONGODB_DB_NAME"] ?? throw new Exception("MONGODB_DB_NAME not set up in environment");
		_collectionName = _configuration["COLLECTION_LOCATIONS"] ?? throw new Exception("COLLECTION_LOCATIONS not set up in environment");

		var client = new MongoClient(_connectionString);
		var database = client.GetDatabase(_dbName);
		_locationsCollection = database.GetCollection<Location>(_collectionName);
	}

	public async Task<List<Location>> GetAllAsync()
	{
		return await _locationsCollection.Find(Builders<Location>.Filter.Empty).ToListAsync();
	}

	public async Task<Location> GetByIdAsync(string id)
	{
		var filter = Builders<Location>.Filter.Eq("_id", new ObjectId(id));
		return await _locationsCollection.Find(filter).FirstOrDefaultAsync();
	}

	public async Task CreateAsync(Location document)
	{
		await _locationsCollection.InsertOneAsync(document);
	}

	public async Task UpdateAsync(string id, Location document)
	{
		var filter = Builders<Location>.Filter.Eq("_id", new ObjectId(id));
		await _locationsCollection.ReplaceOneAsync(filter, document);
	}

	public async Task DeleteAsync(string id)
	{
		var filter = Builders<Location>.Filter.Eq("_id", new ObjectId(id));
		await _locationsCollection.DeleteOneAsync(filter);
	}
}