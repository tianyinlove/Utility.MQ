using BenchmarkDotNet.Running;
using System;

namespace Utility.MQ.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<MqPublishBenchmark>();
        }
    }
}
