using BenchmarkDotNet.Running;
using System;

namespace Utility.RabbitMQ.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<MqPublishBenchmark>();
        }
    }
}
