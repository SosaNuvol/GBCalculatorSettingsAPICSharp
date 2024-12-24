namespace GBCalculatorRatesAPI.Repos;

using GBCalculatorRatesAPI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using QUAD.DSM;

public class TransactionsRepository
{
	private readonly IMongoCollection<TransactionDbEntity> _transactionsCollection;
	public IMongoCollection<TransactionDbEntity> TransactionsCollection {
		get { return _transactionsCollection; }
	}

	private readonly ILogger<TransactionsRepository> _logger;
	private readonly IConfiguration _configuration;
	private readonly string _connectionString;
	private readonly string _dbName;
	private readonly string _collectionName;

	public TransactionsRepository(ILogger<TransactionsRepository> logger, IConfiguration configuration)
	{
		_logger = logger;
		_configuration = configuration;
		_connectionString = _configuration["MONGODB_URI"] ?? throw new Exception("MONGODB_URI not set up in environment");
		_dbName = _configuration["MONGODB_DB_NAME"] ?? throw new Exception("MONGODB_DB_NAME not set up in environment");
		_collectionName = _configuration["COLLECTION_TRANSACTIONS"] ?? throw new Exception("COLLECTION_TRANSACTIONS not set up in environment");

		var client = new MongoClient(_connectionString);
		var database = client.GetDatabase(_dbName);
		_transactionsCollection = database.GetCollection<TransactionDbEntity>(_collectionName);
	
	}

	public async Task<DSMEnvelop<IList<TransactionDbEntity>,TransactionsRepository>> GetAllAsync()
	{
		var response = DSMEnvelop<IList<TransactionDbEntity>,TransactionsRepository>.Initialize(_logger);

		try
		{
			var data = await _transactionsCollection.Find(Builders<TransactionDbEntity>.Filter.Empty).ToListAsync();

			response.Success(data);
		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}

}