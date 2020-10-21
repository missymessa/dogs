using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mono.Options;

namespace dogs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string dogsFilePath = @"data\team_dogs.json"; ;

            var options = new OptionSet()
            {
                { "d|dogs=", "Path of the JSON file with the list of dogs.",  d => dogsFilePath = d }
            };

            options.Parse(args);

            ServiceProvider serviceProvider = new ServiceCollection()
                .AddLogging(l => l.AddConsole())
                .AddSingleton<IDogDataDAO, DogDataJsonDAO>()
                .AddSingleton<IFileProxy, FileProxy>()
                .AddSingleton<PlayDateCalculator>()
                .BuildServiceProvider();

            var calculator = ActivatorUtilities.CreateInstance<PlayDateCalculator>(serviceProvider, dogsFilePath);

            calculator.PrintListOfDogsBySize();
        }
    }
}
