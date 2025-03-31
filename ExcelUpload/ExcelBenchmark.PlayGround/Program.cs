using BenchmarkDotNet.Running;

namespace ExcelBenchmark.PlayGround
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<ExcelBenchmark>();

            // For specific method test
            //var bench = new ExcelBenchmark();
            //bench.Setup();
            //bench.ClosedXml_Write(); 
        }
    }
}
