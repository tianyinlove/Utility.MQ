using System;
using System.Collections.Generic;
using System.Text;
using Utility.RabbitMQ.Attributes;

namespace Utility.Core.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RabbitMQ("utilitymqtest", "authupdate.operate", "RabbitMQConfig")]
    public class AuthUpdateMessage
    {
        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }
    }
}
