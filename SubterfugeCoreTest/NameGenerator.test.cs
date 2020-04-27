using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class NameGenerator_test
    {
        private NameGenerator _generator;

        [TestInitialize]
        public void Setup()
        {
            _generator = new NameGenerator(new SeededRandom(1234));
        }

        [TestMethod]
        public void GeneratesRandomNames()
        {
            string name1 = _generator.GetRandomName();
            string name2 = _generator.GetRandomName();
            Assert.IsTrue(name1.Equals(name2) == false);
        }

        [TestMethod]
        public void NoDuplicatedNames()
        {
            List<string> strings = new List<string>();
            while (_generator.HasNames())
            {
                strings.Add(_generator.GetRandomName());
            }
            
            // Ensure no duplicates.
            Assert.AreEqual(strings.Count, strings.Distinct().Count());
        }

        [TestMethod]
        public void IfNoNamesLeftNoDuplicatesGenerated()
        {
            List<string> strings = new List<string>();
            while (_generator.HasNames())
            {
                strings.Add(_generator.GetRandomName());
            }
            
            // Generate 100 more names.
            for (int i = 0; i < 100; i++)
            {
                strings.Add(_generator.GetRandomName());
            }

            // Ensure no duplicates.
            Assert.AreEqual(strings.Count, strings.Distinct().Count());
        }
        
    }
}