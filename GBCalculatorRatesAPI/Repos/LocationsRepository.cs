namespace GBCalculatorRatesAPI.Repos;

using GBCalculatorRatesAPI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using QUAD.DSM;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LocationsRepository
{
	private readonly IMongoCollection<LocationDbEntity> _locationsCollection;
	public IMongoCollection<LocationDbEntity> LocationsCollection {
		get { return _locationsCollection; }
	}
	private readonly ILogger<LocationsRepository> _logger;
	private readonly IConfiguration _configuration;
	private readonly string _connectionString;
	private readonly string _dbName;
	private readonly string _collectionName;

	public LocationsRepository(ILogger<LocationsRepository> logger, IConfiguration configuration)
	{
		_logger = logger;
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

	public async Task<DSMEnvelop<bool, LocationsRepository>> NukeCollection()
	{
		var response = DSMEnvelop<bool, LocationsRepository>.Initialize(_logger);

		try
		{
			await _locationsCollection.DeleteManyAsync(Builders<LocationDbEntity>.Filter.Empty);

			response.Success(true);
		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	public async Task<DSMEnvelop<UpdateResult, LocationsRepository>> SetUpGeoLocationPropertyInAllDocuments()
	{
		var response = DSMEnvelop<UpdateResult, LocationsRepository>.Initialize(_logger);

		try
		{
			var filter = Builders<LocationDbEntity>.Filter.And(
				Builders<LocationDbEntity>.Filter.Exists("longitude"),
				Builders<LocationDbEntity>.Filter.Exists("latitude")
			);

			var update = Builders<LocationDbEntity>.Update.Set("location", new BsonDocument
			{
				{ "type", "Point" },
				{ "coordinates", new BsonArray { "$longitude", "$latitude" } }
			});

			var updateResult = await _locationsCollection.UpdateManyAsync(filter, update);

			response.Success(updateResult);
		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	public async Task<DSMEnvelop<bool,LocationsRepository>> CreateLocationIndex()
	{
		var response = DSMEnvelop<bool,LocationsRepository>.Initialize(_logger);

		try
		{
			var indexKeysDefinition = Builders<LocationDbEntity>.IndexKeys.Geo2DSphere("location");
			await _locationsCollection.Indexes.CreateOneAsync(new CreateIndexModel<LocationDbEntity>(indexKeysDefinition));

			response.Success(true);
		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}
}