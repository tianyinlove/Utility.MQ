using System;
using System.Collections.Generic;
using System.Text;

namespace Utility.RabbitMQ.Attributes
{
    /// <summary>
    /// 消息队列属性
    /// </summary>
    public class RabbitMQAttribute : Attribute
    {
        /// <summary>
        /// MQ配置名GetSection获取
        /// </summary>
        public string ConfigName { get; set; }

        /// <summary>
        /// 生产者应用id
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 路由
        /// </summary>
        public string RouteKey { get; set; }

        /// <summary>
        /// 标记消息的默认发送路由
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="routingKey"></param>
        /// <param name="configName">MQ配置名GetSection获取</param>
        public RabbitMQAttribute(string appId, string routingKey, string configName = "")
        {
            AppId = appId;
            RouteKey = routingKey;
            ConfigName = ConfigName;
        }
    }
}
