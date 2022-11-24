using Utility.RabbitMQ.Configuration;
using Utility.RabbitMQ.Documents;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq;

namespace Utility.RabbitMQ.Repositories;

public class MQMongoDbContext : IMQMongoDbContext
{
    private readonly IOptionsMonitor<AppSettings> _configAccessor;

    /// <summary>
    /// mongodb公共方法
    /// </summary>
    /// <param name="connectionString">连接</param>
    /// <param name="databaseName">库名</param>
    public MQMongoDbContext(IOptionsMonitor<AppSettings> configAccessor)
    {
        _configAccessor = configAccessor;
    }
    MongoClient _client;
    IMongoDatabase _database;
    /// <summary>
    /// 
    /// </summary>
    protected MongoClient Client { get => _client ??= new MongoClient(_configAccessor.CurrentValue.MongodbConnectionString); }

    /// <summary>
    /// 
    /// </summary>
    protected IMongoDatabase Database { get => _database ??= Client.GetDatabase(_configAccessor.CurrentValue.MongodbDatabase); }

    public IMongoCollection<FailedMessageDocument> FailedMessageCollection => GetCollection<FailedMessageDocument>("FailedMessage");

    public IMongoCollection<WrongMessageDocument> WrongMessageCollection => GetCollection<WrongMessageDocument>("WrongMessage");

    /// <summary>
    /// 获取集合
    /// </summary>
    /// <typeparam name="TDocument"></typeparam>
    /// <param name="collectionName">集合名称,
    /// 如果集合名称为空，就取<typeparamref name="TDocument"/>的CollectionNameAttribute
    /// 否则就取类名，忽略Entity结束名
    ///  </param>
    /// <returns></returns>
    public IMongoCollection<TDocument> GetCollection<TDocument>(string collectionName)
    {
        return Database.GetCollection<TDocument>(collectionName);
    }

    public void Migrate()
    {
        {
            var collection = FailedMessageCollection;
            var indexes = collection.Indexes.List().ToList();
            if (!indexes.Any(d => d.GetElement("name").Value.AsString == "QueueNamePublishTime"))
            {
                collection.Indexes.CreateOne(new CreateIndexModel<FailedMessageDocument>(
                    Builders<FailedMessageDocument>.IndexKeys.Ascending(d => d.QueueName).Ascending(d => d.PublishTime),
                    new CreateIndexOptions
                    {
                        Name = "QueueNamePublishTime",
                        Background = true
                    }));
            }
        }
        {
            var collection = WrongMessageCollection;
            var indexes = collection.Indexes.List().ToList();
            if (!indexes.Any(d => d.GetElement("name").Value.AsString == "QueueNamePublishTime"))
            {
                collection.Indexes.CreateOne(new CreateIndexModel<WrongMessageDocument>(
                    Builders<WrongMessageDocument>.IndexKeys.Ascending(d => d.QueueName).Ascending(d => d.PublishTime),
                    new CreateIndexOptions
                    {
                        Name = "QueueNamePublishTime",
                        Background = true
                    }));
            }
        }
    }
}
