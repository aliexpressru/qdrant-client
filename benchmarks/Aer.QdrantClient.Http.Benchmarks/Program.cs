using BenchmarkDotNet.Running;

namespace Aer.QdrantClient.Http.Benchmarks;

internal class Program
{
    static void Main(string[] args) =>
        //BenchmarkRunner.Run(typeof(Program).Assembly)
        BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(
                ["--filter", "*"]
            //, new DebugInProcessConfig()
            );
}
