using Utility.MQ.Documents;
using MongoDB.Driver;

namespace Utility.MQ.Repositories;

public interface IMQMongoDbContext
{
    /// <summary>
    /// 获取集合
    /// </summary>
    /// <typeparam name="TDocument"></typeparam>
    /// <param name="collectionName">集合名称,
    /// 如果集合名称为空，就取<typeparamref name="TDocument"/>的CollectionNameAttribute
    /// 否则就取类名，忽略Entity结束名
    ///  </param>
    /// <returns></returns>
    IMongoCollection<TDocument> GetCollection<TDocument>(string collectionName);

    IMongoCollection<FailedMessageDocument> FailedMessageCollection { get; }

    IMongoCollection<WrongMessageDocument> WrongMessageCollection { get; }

    /// <summary>
    /// 检查索引
    /// </summary>
    void Migrate();
}
