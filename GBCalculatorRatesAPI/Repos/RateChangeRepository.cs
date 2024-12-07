using GBCalculatorRatesAPI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using QUAD.DSM;

namespace GBCalculatorRatesAPI.Repos;

public class RateChangeRepository {
	private readonly IMongoCollection<RateChangeDbEntity> _rateChangesCollection;
	private readonly ILogger<RateChangeRepository> _logger;
	private readonly IConfiguration _configuration;
	private readonly string _connectionString;
	private readonly string _dbName;
	private readonly string _collectionName;

	public RateChangeRepository(ILogger<RateChangeRepository> logger, IConfiguration configuration)
	{
		_logger = logger;
		_configuration = configuration;
		_connectionString = _configuration["MONGODB_URI"] ?? throw new Exception("MONGODB_URI not set up in environment");
		_dbName = _configuration["MONGODB_DB_NAME"] ?? throw new Exception("MONGODB_DB_NAME not set up in environment");
		_collectionName = _configuration["COLLECTION_RATECHANGES"] ?? throw new Exception("COLLECTION_RATECHANGES not set up in environment");

		var client = new MongoClient(_connectionString);
		var database = client.GetDatabase(_dbName);
		_rateChangesCollection = database.GetCollection<RateChangeDbEntity>(_collectionName);
	}

	public async Task<DSMEnvelop<RateChangeDbEntity>> CreateAsync(RateChangeDbEntity document)
	{
		var response = DSMEnvelop<RateChangeDbEntity>.Initialize();

		try {
			await _rateChangesCollection.InsertOneAsync(document);

			response.Success(document);
		} catch (Exception ex) {
			_logger.LogError($"|| ** Error occurred when trying to create a rateChange: {ex.Message}");
			response.Error(DSMEnvelopeCodeEnum.API_REPOS_05000, $"Error: {ex.Message}");
		}

		return response;
	}
}