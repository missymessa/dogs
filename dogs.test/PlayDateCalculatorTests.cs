using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dogs.test
{
    [TestFixture]
    public class PlayDateCalculatorTests
    {
        private const string DOG_FILE_PATH_GOOD = @"C:\this\path\is\good\";
        private const string DOG_FILE_PATH_BAD = @"C:\this\path\is\garbage\";
        private const string DOG_DATA_JSON = "{\"dogs\":[{\"name\":\"Coco\",\"breeds\":[\"Pug\"],\"isMix\":false},{\"name\":\"Oliver\",\"breeds\":[\"Jack Russell Terrier\"],\"isMix\":true},{\"name\":\"Cassie\",\"breeds\":[\"Pembroke Welsh Corgi\"],\"isMix\":false},{\"name\":\"Odie\",\"breeds\":[\"Boxer\"],\"isMix\":true},{\"name\":\"Lexi\",\"breeds\":[\"Shiba Inu\"],\"isMix\":true},{\"name\":\"Oreo\",\"breeds\":[\"Boston Terrier\"],\"isMix\":false},{\"name\":\"Marius\",\"breeds\":[\"Black Labrador\",\"Golden Retriever\"],\"isMix\":true}]}";

        public PlayDateCalculator SetUp(
            string dogFilePathOverride = null,
            Mock<IFileProxy> fileProxyMockOverride = null, 
            Mock<IDogDataDAO> dogDataDAOMockOverride = null)
        {
            // Set up mocks
            string dogFilePath = dogFilePathOverride ?? DOG_FILE_PATH_GOOD;

            Mock<IFileProxy> fileProxyMock = new Mock<IFileProxy>();
            if (fileProxyMockOverride == null)
            {
                fileProxyMock.Setup(m => m.ReadAllText(DOG_FILE_PATH_GOOD)).Returns(DOG_DATA_JSON);
                fileProxyMock.Setup(m => m.ReadAllText(DOG_FILE_PATH_BAD)).Throws<FileNotFoundException>();
            }
            else
            {
                fileProxyMock = fileProxyMockOverride;
            }

            Mock<IDogDataDAO> dogDataDAOMock = new Mock<IDogDataDAO>();
            if (dogDataDAOMockOverride == null)
            {
                dogDataDAOMock.Setup(m => m.GetDogData()).Returns(new DogData()
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
            else
            {
                dogDataDAOMock = dogDataDAOMockOverride;
            }

            var services = new ServiceCollection()
                .AddLogging(l => { l.AddProvider(new NUnitLogger()); })
                .AddSingleton<PlayDateCalculator>()
                .AddSingleton(fileProxyMock.Object)
                .AddSingleton(dogDataDAOMock.Object)
                .BuildServiceProvider();

            return ActivatorUtilities.CreateInstance<PlayDateCalculator>(services, dogFilePath);
        }

        [Test]
        public void GetAverageDogSizeList_GivesExpectedResults()
        {
            var playDateCalculator = SetUp();
            
            var results = playDateCalculator.GetAverageDogSizeList();

            results.Should().
                NotBeEmpty().And.
                BeEquivalentTo(new Dictionary<Size, List<Dog>>()
                {
                    { Size.toy, new List<Dog>() },
                    { Size.small, new List<Dog>()
                        {
                            new Dog() { Name = "Coco", Breeds = new List<string>() { "Pug" }, IsMix = false },
                            new Dog() { Name = "Oreo", Breeds = new List<string>() { "Boston Terrier" }, IsMix = false },
                            new Dog() { Name = "Oliver", Breeds = new List<string>() { "Jack Russell Terrier" }, IsMix = true }
                        }
                    },
                    { Size.medium, new List<Dog>()
                        {
                            new Dog() { Name = "Cassie", Breeds = new List<string>() { "Pembroke Welsh Corgi" }, IsMix = false },
                            new Dog() { Name = "Lexi", Breeds = new List<string>() { "Shiba Inu" }, IsMix = true }
                        }
                    },
                    { Size.large, new List<Dog>()
                        {
                            new Dog() { Name = "Odie", Breeds = new List<string>() { "Boxer" }, IsMix = true },
                            new Dog() { Name = "Marius", Breeds = new List<string>() { "Black Labrador", "Golden Retriever" }, IsMix = true }
                        }
                    },
                    { Size.extraLarge, new List<Dog>() }
                });
        }

        [Test]
        public void BreedDataMissingFromDAOSource()
        {
            var dogData = "{\"dogs\":[{\"name\":\"DogName\",\"breeds\":[\"Puggle\"],\"isMix\":false}]}";

            Mock<IFileProxy> fileProxyMock = new Mock<IFileProxy>();
            fileProxyMock.Setup(m => m.ReadAllText(DOG_FILE_PATH_GOOD)).Returns(dogData);

            var playDateCalculator = SetUp(fileProxyMockOverride: fileProxyMock);
            playDateCalculator.Invoking(i => i.GetAverageDogSizeList()).Should().Throw<DogBreedNotFoundException>();
        }

        [Test]
        public void DogHasMoreThanOneBreedAndNotMarkedAsMix()
        {
            var dogData = "{\"dogs\":[{\"name\":\"DogName\",\"breeds\":[\"Pug\", \"Beagle\"],\"isMix\":false}]}";

            Mock<IFileProxy> fileProxyMock = new Mock<IFileProxy>();
            fileProxyMock.Setup(m => m.ReadAllText(DOG_FILE_PATH_GOOD)).Returns(dogData);

            var playDateCalculator = SetUp(fileProxyMockOverride: fileProxyMock);
            playDateCalculator.Invoking(i => i.GetAverageDogSizeList()).Should().Throw<IsMixedBreedException>();
        }

        [Test]
        public void BreedSizeAverageCalculationValidation()
        {
            Mock<IDogDataDAO> dogDataDAOMock = new Mock<IDogDataDAO>();
            dogDataDAOMock.Setup(m => m.GetDogData()).Returns(new DogData()
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

            var dogData = "{\"dogs\":[{\"name\":\"Boxapug\",\"breeds\":[\"Pug\", \"Boxer\"],\"isMix\":true},{\"name\":\"Corgidor\",\"breeds\":[\"Pembroke Welsh Corgi\", \"Black Labrador\"],\"isMix\":true}]}";

            Mock<IFileProxy> fileProxyMock = new Mock<IFileProxy>();
            fileProxyMock.Setup(m => m.ReadAllText(DOG_FILE_PATH_GOOD)).Returns(dogData);

            var playDateCalculator = SetUp(fileProxyMockOverride: fileProxyMock, dogDataDAOMockOverride: dogDataDAOMock);

            var results = playDateCalculator.GetAverageDogSizeList();

            // Pug and Boxer mix should average to "medium"
            results[Size.medium].Should()
                .HaveCount(1).And
                .BeEquivalentTo(new List<Dog>()
                {
                    new Dog() { Name = "Boxapug", Breeds = new List<string>() { "Pug", "Boxer" }, IsMix = true }
                });

            // Corgi and Lab mix should round up average to "large"
            results[Size.large].Should()
                .HaveCount(1).And
                .BeEquivalentTo(new List<Dog>()
                {
                    new Dog() { Name = "Corgidor", Breeds = new List<string>() { "Pembroke Welsh Corgi", "Black Labrador" }, IsMix = true }
                });
        }

        [Test]
        public void InputFileDoesNotExist()
        {
            Action act = () => SetUp(dogFilePathOverride: DOG_FILE_PATH_BAD);
            act.Should().Throw<FileNotFoundException>();
        }
    }
}