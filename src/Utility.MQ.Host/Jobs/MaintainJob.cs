using Emapp.Attributes;

namespace Utility.MQ.Jobs
{
    /// <summary>
    /// mq数据维护
    /// </summary>
    [EmappService("MQ", "maintain")]
    class MaintainJob
    {
        /// <summary>
        /// 占位
        /// </summary>
        /// <returns></returns>
        public void Blank()
        {
        }
    }
}

