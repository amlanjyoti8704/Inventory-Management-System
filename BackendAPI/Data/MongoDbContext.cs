using MongoDB.Driver;
using MongoDB.Bson;
using BackendAPI.Models;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoDatabase database)
    {
        _database = database;
    }

    public IMongoDatabase Database => _database;

    // Typed collection accessors
    public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    public IMongoCollection<Category> Categories => _database.GetCollection<Category>("category");
    public IMongoCollection<ConsumableItems> ConsumableItems => _database.GetCollection<ConsumableItems>("consumableItems");
    public IMongoCollection<IssueRecords> IssueRecords => _database.GetCollection<IssueRecords>("issue_records");
    public IMongoCollection<PurchaseDetailsModal> PurchaseDetails => _database.GetCollection<PurchaseDetailsModal>("purchase_details");
    public IMongoCollection<AlertLog> AlertLogs => _database.GetCollection<AlertLog>("alert_log");

    /// <summary>
    /// Auto-increment ID generator using a counters collection.
    /// Each collection gets its own counter document.
    /// </summary>
    public async Task<int> GetNextSequenceAsync(string collectionName)
    {
        var countersCollection = _database.GetCollection<BsonDocument>("counters");

        var filter = Builders<BsonDocument>.Filter.Eq("_id", collectionName);
        var update = Builders<BsonDocument>.Update.Inc("seq", 1);
        var options = new FindOneAndUpdateOptions<BsonDocument>
        {
            ReturnDocument = ReturnDocument.After,
            IsUpsert = true
        };

        var result = await countersCollection.FindOneAndUpdateAsync(filter, update, options);
        return result["seq"].AsInt32;
    }

    /// <summary>
    /// Synchronous version of GetNextSequenceAsync for controllers that use sync patterns.
    /// </summary>
    public int GetNextSequence(string collectionName)
    {
        return GetNextSequenceAsync(collectionName).GetAwaiter().GetResult();
    }
}
