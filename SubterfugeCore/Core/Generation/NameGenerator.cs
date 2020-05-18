using System.Collections.Generic;
using System.Linq;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.Generation
{
    public class NameGenerator
    {
        private SeededRandom seeder;

        List<string> _outpostNames;
        
        /// <summary>
        /// Creates an instance of the name generator using an already generated seeded random tool.
        /// </summary>
        /// <param name="seeder">The seeder to use for random name selection</param>
        public NameGenerator(SeededRandom seeder)
        {
            this.seeder = seeder;
            this.populateNames();
        }

        /// <summary>
        /// Sets the list of outpost names to be pulled from.
        /// </summary>
        public void populateNames()
        {
            _outpostNames = new List<string>() {
            "Rokovo",
            "Latvia",
            "Pichu",  // Pokemon easer egg
            "Shiloh", // My dog. RIP <3
            "Dozer",
            "Nautilus",
            "Minceraft", // Pretty obvious
            "Sans", // Undertale
            "London",
            "Helsinki",
            "Subterfuge",
            "Lyons",
            "Norris",
            "Hooper",
            "Glover",
            "Mills",
            };
        }

        public string GetRandomName()
        {
            if (_outpostNames.Count > 0)
            {
                int selection = seeder.NextRand(0, _outpostNames.Count - 1);
                string name = _outpostNames[selection];
                _outpostNames.Remove(name);
                return name;
            }
            // Should never get here but...
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[seeder.NextRand(0, s.Length)]).ToArray());
        }

        public bool HasNames()
        {
            return _outpostNames.Count > 0;
        }
        
    }
}