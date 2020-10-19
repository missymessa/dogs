using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace dogs.test
{
    [TestFixture]
    public class DogDataDAOTests
    {
        private const string DOG_DATA_JSON_FILE_PATH = @"data\dog_breed_data.json";
        private const string DOG_DATA_JSON = "{\"breeds\":[{\"name\":\"Boston Terrier\",\"size\":\"small\"},{\"name\":\"Boxer\",\"size\":\"large\"},{\"name\":\"Black Labrador\",\"size\":\"large\"},{\"name\":\"Golden Retriever\",\"size\":\"large\"},{\"name\":\"Jack Russell Terrier\",\"size\":\"small\"},{\"name\":\"Pembroke Welsh Corgi\",\"size\":\"medium\"},{\"name\":\"Pug\",\"size\":\"small\"},{\"name\":\"Shiba Inu\",\"size\":\"medium\"}]}";

        public IDogDataDAO SetUp(Mock<IFileProxy> fileProxyMockOverride = null)
        {
            // Set up mocks
            Mock<IFileProxy> fileProxyMock = new Mock<IFileProxy>();
            if (fileProxyMockOverride == null)
            {
                fileProxyMock.Setup(m => m.ReadAllText(DOG_DATA_JSON_FILE_PATH)).Returns(DOG_DATA_JSON);
            }
            else
            {
                fileProxyMock = fileProxyMockOverride;
            }
    
            var collection = new ServiceCollection()
                .AddLogging(l => { l.AddProvider(new NUnitLogger()); })
                .AddSingleton<DogDataJsonDAO>()
                .AddSingleton(fileProxyMock.Object);

            ServiceProvider services = collection.BuildServiceProvider();

            return ActivatorUtilities.CreateInstance<DogDataJsonDAO>(services);
        }

        [Test]
        public void GetDogData_LoadsExpectedData()
        {
            var dogDataDAO = SetUp();
            var dogData = dogDataDAO.GetDogData();

            dogData.Should()
                .NotBeNull().And
                .BeEquivalentTo(new DogData()
                {
                    Breeds = new List<Breed>()
                    {
                        new Breed() { Name = "Boston Terrier", Size = Size.small },
                        new Breed() { Name = "Boxer", Size = Size.large },
                        new Breed() { Name = "Black Labrador", Size = Size.large },
                        new Breed() { Name = "Golden Retriever", Size = Size.large },
                        new Breed() { Name = "Jack Russell Terrier", Size = Size.small },
                        new Breed() { Name = "Pembroke Welsh Corgi", Size = Size.medium },
                        new Breed() { Name = "Pug", Size = Size.small },
                        new Breed() { Name = "Shiba Inu", Size = Size.medium }
                    }
                });
        }

        [Test]
        public void GetDogData_MissingDataFile()
        {
            Mock<IFileProxy> fileProxyMock = new Mock<IFileProxy>();
            fileProxyMock.Setup(m => m.ReadAllText(DOG_DATA_JSON_FILE_PATH)).Throws<FileNotFoundException>();

            var dogDataDAO = SetUp(fileProxyMockOverride: fileProxyMock);
            dogDataDAO.Invoking(i => i.GetDogData()).Should().Throw<FileNotFoundException>();
        }
    }
}