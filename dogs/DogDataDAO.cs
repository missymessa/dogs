using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dogs
{
    public interface IDogDataDAO
    {
        DogData GetDogData();
    }

    public class DogDataJsonDAO : IDogDataDAO
    {
        private readonly string _jsonFilePath = @"data\dog_breed_data.json";
        private readonly IFileProxy _fileProxy;

        public DogDataJsonDAO(IFileProxy fileProxy)
        {
            _fileProxy = fileProxy;
        }

        public DogData GetDogData()
        {
            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.PropertyNameCaseInsensitive = true;
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            return JsonSerializer.Deserialize<DogData>(_fileProxy.ReadAllText(_jsonFilePath), jsonSerializerOptions);
        }
    }
}
