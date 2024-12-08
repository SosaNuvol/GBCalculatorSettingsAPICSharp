namespace GBCalculatorRatesAPI.Repos;

using GBCalculatorRatesAPI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using QUAD.DSM;

public class CacheRepository<T> {
	private readonly IMongoCollection<CacheDbEntity<T>> _dataCacheCollection;
	private readonly ILogger<CacheRepository<T>> _logger;
	private readonly IConfiguration _configuration;
	private readonly string _connectionString;
	private readonly string _dbName;
	private readonly string _dataCacheCollectionName;

	public CacheRepository(ILogger<CacheRepository<T>> logger, IConfiguration configuration) {
		_logger = logger;
		_configuration = configuration;
		_connectionString = _configuration["MONGODB_URI"] ?? throw new Exception("MONGODB_URI not set up in environment");
		_dbName = _configuration["MONGODB_DB_NAME"] ?? throw new Exception("MONGODB_DB_NAME not set up in environment");
		_dataCacheCollectionName = _configuration["COLLECTION_DATA_CACHE"] ?? throw new Exception("COLLECTION_RATECHANGES not set up in environment");

		var client = new MongoClient(_connectionString);
		var database = client.GetDatabase(_dbName);
		_dataCacheCollection = database.GetCollection<CacheDbEntity<T>>(_dataCacheCollectionName);
	}

	public async Task<DSMEnvelop<CacheDbEntity<T>, CacheRepository<T>>> GetLatestItem() {
		var response = DSMEnvelop<CacheDbEntity<T>, CacheRepository<T>>.Initialize(_logger);

		try {
			var cacheType = Utilities.Tools.GetCacheType<T>();
			var filter = Builders<CacheDbEntity<T>>.Filter.Eq(entity => entity.CacheType, cacheType);
			var sort = Builders<CacheDbEntity<T>>.Sort.Descending(entity => entity.CreatedDate);

			var latestItem = await _dataCacheCollection
				.Find(filter)
				.Sort(sort)
				.Limit(1)
				.FirstOrDefaultAsync();

			if (latestItem == null) return response.Warning(DSMEnvelopeCodeEnum.API_REPOS_05010, $"|| ** Item {Utilities.Tools.GetCacheType<T>()} was not found");

			response.Success(latestItem);
		} catch(Exception ex) {
			response.Error(ex, DSMEnvelopeCodeEnum.API_REPOS_05001, $"|| ** Error on GetLatertItem in CacheRepository for \"{typeof(T).Name}\".");
		}

        return response;
	}

	internal async Task<DSMEnvelop<CacheDbEntity<T>, CacheRepository<T>>> UpdateCache(T payload)
	{
		var response = DSMEnvelop<CacheDbEntity<T>, CacheRepository<T>>.Initialize(_logger);

		try {
			var newCacheDbEntity = new CacheDbEntity<T> {
				Payload = payload,
				CacheType = Utilities.Tools.GetCacheType<T>(),
				ExpiresAt = DateTime.UtcNow.AddDays(1), // Set appropriate expiration time
				CreatedDate = DateTime.UtcNow
			};

			await _dataCacheCollection.InsertOneAsync(newCacheDbEntity);

			response.Success(newCacheDbEntity);

		} catch(Exception ex) {
			response.Error(ex, DSMEnvelopeCodeEnum.API_REPOS_05010, $"|| ** Error on UpdateCache in CacheRepository for \"{typeof(T).Name}\".");
		}

		return response;
	}
}