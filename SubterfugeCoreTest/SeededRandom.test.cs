using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore.Timing;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class SeededRandomTest
    {

        [TestMethod]
        public void sameSeedTest()
        {
            int seed = 1;
            SeededRandom rand1 = new SeededRandom(seed);
            SeededRandom rand2 = new SeededRandom(seed);

            for(int i = 0; i < 1000; i++)
            {
                Assert.AreEqual(rand1.nextDouble(), rand2.nextDouble());
            }
            for (int i = 0; i < 1000; i++)
            {
                Assert.AreEqual(rand1.nextRand(0, 10), rand2.nextRand(0, 10));
            }
            for (int i = 0; i < 1000; i++)
            {
                Assert.AreEqual(rand1.nextDouble(), rand2.nextDouble());
                Assert.AreEqual(rand1.nextRand(0, 10), rand2.nextRand(0, 10));
            }
        }

        [TestMethod]
        public void indexedRand()
        {
            int seed = 1234;
            SeededRandom rand1 = new SeededRandom(seed);
            SeededRandom rand2 = new SeededRandom(seed);


            Assert.AreEqual(rand1.getDouble(100), rand2.getDouble(100));
            Assert.AreEqual(rand1.getRand(200, 0, 10), rand2.getRand(200, 0, 10));
        }

    }
}
