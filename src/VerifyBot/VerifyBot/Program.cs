using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace VerifyBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                    services.AddHostedService<Startup>());
        }
    }
}