using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dogs
{
    public class PlayDateCalculator
    {
        private readonly IDogDataDAO _dogDataDAO;
        private readonly IFileProxy _fileProxy;
        private readonly DogData _dogBreedData;
        private readonly TeamDogs _teamDogs;

        public PlayDateCalculator(IDogDataDAO dogDataDAO, IFileProxy fileProxy, string dogFilePath)
        {
            _dogDataDAO = dogDataDAO;
            _fileProxy = fileProxy;
            _dogBreedData = _dogDataDAO.GetDogData();

            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.PropertyNameCaseInsensitive = true;
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            _teamDogs = JsonSerializer.Deserialize<TeamDogs>(_fileProxy.ReadAllText(dogFilePath), jsonSerializerOptions);
        }

        [ExcludeFromCodeCoverage]
        public void PrintListOfDogsBySize()
        {
            foreach (var dogSize in GetAverageDogSizeList())
            {
                if (dogSize.Value.Any())
                {
                    Console.WriteLine("These dogs are compatible size for a play date: ");
                }

                foreach (var dog in dogSize.Value)
                {
                    Console.WriteLine($"{dog.Name} ({dogSize.Key})");
                }
            }
        }

        public Dictionary<Size, List<Dog>> GetAverageDogSizeList()
        {
            Dictionary<Size, List<Dog>> returnDictionary = new Dictionary<Size, List<Dog>>();

            foreach(var size in Enum.GetValues(typeof(Size)))
            {
                returnDictionary.Add((Size)size, new List<Dog>());
            }

            foreach (Dog dog in _teamDogs.Dogs)
            {
                if (dog.IsMix)
                {
                    List<Size> breedsSizes = new List<Size>();

                    foreach (string breed in dog.Breeds)
                    {
                        breedsSizes.Add(_dogBreedData.Breeds.Find(x => x.Name == breed).Size);
                    }

                    double breedAverage = breedsSizes.Average(x => (int)x);
                    Size averageSize = (Size)Math.Ceiling(breedAverage);

                    returnDictionary[averageSize].Add(dog);
                }
                else
                {
                    string dogBreedToFind = dog.GetBreed();
                    Breed dogBreed = _dogBreedData.Breeds.Find(x => x.Name == dogBreedToFind);

                    if(dogBreed == null)
                    {
                        throw new DogBreedNotFoundException($"Breed '{dogBreedToFind}' was not found in the source data.");
                    }

                    returnDictionary[dogBreed.Size].Add(dog);
                }
            }

            return returnDictionary;
        }
    }

    public class DogBreedNotFoundException : Exception
    {
        public DogBreedNotFoundException(string message) : base(message)
        {

        }
    }
}
