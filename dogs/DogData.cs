using System;
using System.Collections.Generic;
using System.Text;

namespace dogs
{
    public class DogData
    {
        public List<Breed> Breeds { get; set; }
    }

    public class Breed
    {
        public string Name { get; set; }
        public Size Size { get; set; }
    }

    public enum Size
    {
        toy,
        small,
        medium,
        large,
        extraLarge
    }
}
