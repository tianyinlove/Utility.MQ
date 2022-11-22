namespace Emapp.Attributes
{
    /// <summary>
    /// 设置mq消费者
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false)]
    public class MQConsumerOptionsAttribute : Attribute
    {
        /// <summary>
        /// 是否自动添加消费服务
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 服务标签，可通过配置开关
        /// </summary>
        public string DeployTag { get; set; }
    }
}
