namespace GBCalculatorRatesAPI.Repos;

using GBCalculatorRatesAPI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

public class CacheRepository<T> {
	private readonly IMongoCollection<CacheDbEntity<T>> _dataCacheCollection;
	private readonly ILogger<CacheDbEntity<T>> _logger;
	private readonly IConfiguration _configuration;
	private readonly string _connectionString;
	private readonly string _dbName;
	private readonly string _dataCacheCollectionName;

	public CacheRepository(ILogger<CacheDbEntity<T>> logger, IConfiguration configuration) {
		_logger = logger;
		_configuration = configuration;
		_connectionString = _configuration["MONGODB_URI"] ?? throw new Exception("MONGODB_URI not set up in environment");
		_dbName = _configuration["MONGODB_DB_NAME"] ?? throw new Exception("MONGODB_DB_NAME not set up in environment");
		_dataCacheCollectionName = _configuration["COLLECTION_DATA_CACHE"] ?? throw new Exception("COLLECTION_RATECHANGES not set up in environment");

		var client = new MongoClient(_connectionString);
		var database = client.GetDatabase(_dbName);
		_dataCacheCollection = database.GetCollection<CacheDbEntity<T>>(_dataCacheCollectionName);
	}

	public async Task<CacheDbEntity<T>> GetLatestItem() {
        var cacheType = GetCacheType();
        var filter = Builders<CacheDbEntity<T>>.Filter.Eq(entity => entity.CacheType, cacheType);
        var sort = Builders<CacheDbEntity<T>>.Sort.Descending(entity => entity.CreatedDate);

        var latestItem = await _dataCacheCollection
            .Find(filter)
            .Sort(sort)
            .Limit(1)
            .FirstOrDefaultAsync();

        return latestItem;
	}

    private string GetCacheType() {
        return typeof(T).Name;
    }
}