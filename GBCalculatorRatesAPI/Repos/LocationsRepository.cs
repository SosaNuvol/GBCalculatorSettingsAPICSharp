namespace GBCalculatorRatesAPI.Repos;

using GBCalculatorRatesAPI.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LocationsRepository
{
	private readonly IMongoCollection<LocationDbEntity> _locationsCollection;
	public IMongoCollection<LocationDbEntity> LocationsCollection {
		get { return _locationsCollection; }
	}
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
		_locationsCollection = database.GetCollection<LocationDbEntity>(_collectionName);
	
	}

	public async Task<List<LocationDbEntity>> FindAsync(FilterDefinition<LocationDbEntity> filter)
	{
		return await _locationsCollection.Find(filter).ToListAsync();
	}

	public async Task<List<LocationDbEntity>> QueryAsyn(BsonDocument query) {
		return await _locationsCollection.Find(query).ToListAsync();
	}

	public async Task<List<LocationDbEntity>> GetAllAsync()
	{
		return await _locationsCollection.Find(Builders<LocationDbEntity>.Filter.Empty).ToListAsync();
	}

	public async Task<LocationDbEntity> GetByIdAsync(string id)
	{
		var filter = Builders<LocationDbEntity>.Filter.Eq("_id", new ObjectId(id));
		return await _locationsCollection.Find(filter).FirstOrDefaultAsync();
	}

	public async Task CreateAsync(LocationDbEntity document)
	{
		await _locationsCollection.InsertOneAsync(document);
	}

	public async Task UpdateAsync(string id, LocationDbEntity document)
	{
		var filter = Builders<LocationDbEntity>.Filter.Eq("_id", new ObjectId(id));
		await _locationsCollection.ReplaceOneAsync(filter, document);
	}

	public async Task DeleteAsync(string id)
	{
		var filter = Builders<LocationDbEntity>.Filter.Eq("_id", new ObjectId(id));
		await _locationsCollection.DeleteOneAsync(filter);
	}
}