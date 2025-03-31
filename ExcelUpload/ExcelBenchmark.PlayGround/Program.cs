using BenchmarkDotNet.Running;

namespace ExcelBenchmark.PlayGround
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<ExcelBenchmark>();
        }
    }
}
