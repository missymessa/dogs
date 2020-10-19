using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mono.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dogs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string dogsFilePath = null;
            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.PropertyNameCaseInsensitive = true;
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));


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
