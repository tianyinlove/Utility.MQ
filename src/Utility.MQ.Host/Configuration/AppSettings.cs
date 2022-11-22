﻿using MongoDB.Driver;

namespace Utility.MQ.Configuration;

/// <summary>
/// 应用配置
/// </summary>
public class AppSettings
{
    public string MongodbConnectionString { get; set; }

    public string MongodbDatabase { get; set; }
}
