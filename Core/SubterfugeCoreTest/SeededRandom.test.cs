using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class SeededRandomTest
    {

        [TestMethod]
        public void SameSeedTest()
        {
            int seed = 1;
            SeededRandom rand1 = new SeededRandom(seed);
            SeededRandom rand2 = new SeededRandom(seed);

            for(int i = 0; i < 1000; i++)
            {
                Assert.AreEqual(rand1.NextDouble(), rand2.NextDouble());
            }
            for (int i = 0; i < 1000; i++)
            {
                Assert.AreEqual(rand1.NextRand(0, 10), rand2.NextRand(0, 10));
            }
            for (int i = 0; i < 1000; i++)
            {
                Assert.AreEqual(rand1.NextDouble(), rand2.NextDouble());
                Assert.AreEqual(rand1.NextRand(0, 10), rand2.NextRand(0, 10));
            }
        }

        [TestMethod]
        public void IndexedRand()
        {
            int seed = 1234;
            SeededRandom rand1 = new SeededRandom(seed);
            SeededRandom rand2 = new SeededRandom(seed);


            Assert.AreEqual(rand1.GetDouble(100), rand2.GetDouble(100));
            Assert.AreEqual(rand1.GetRand(200, 0, 10), rand2.GetRand(200, 0, 10));
        }

    }
}