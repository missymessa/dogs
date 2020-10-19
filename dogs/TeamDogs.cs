using System;
using System.Collections.Generic;

namespace dogs
{
    public class TeamDogs
    {
        public List<Dog> Dogs { get; set; }
    }

    public class Dog
    {
        public string Name { get; set; }
        public List<string> Breeds { get; set; }
        public bool IsMix { get; set; }

        public string GetBreed()
        {
            if(!IsMix && Breeds.Count == 1)
            {
                return Breeds[0];
            }

            throw new IsMixedBreedException("Could not return breed. This dog is mixed breed.");
        }
    }

    public class IsMixedBreedException : Exception
    {
        public IsMixedBreedException(string message) : base(message)
        {

        }
    }
}
