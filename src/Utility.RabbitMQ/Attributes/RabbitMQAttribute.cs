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
        public RabbitMQAttribute(string appId, string routingKey)
        {
            AppId = appId;
            RouteKey = routingKey;
        }
    }
}
