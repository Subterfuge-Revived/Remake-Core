using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class RftVector_test
    {

        [TestMethod]
        public void GettersAndSetters()
        {
            int mapDimensions = 100;
            int pointDimensions = 50;
            
            Rft map = new Rft(mapDimensions, mapDimensions);
            RftVector vectorTest = new RftVector(map, pointDimensions, pointDimensions);
            
            Assert.AreEqual(pointDimensions, vectorTest.X);
            Assert.AreEqual(pointDimensions, vectorTest.Y);
            Assert.AreEqual(mapDimensions, map.Height);
            Assert.AreEqual(mapDimensions, map.Width);
        }

        [TestMethod]
        public void GlobalMapSize()
        {
            int mapSize = 100;
            Rft map = new Rft(mapSize, mapSize);

            int firstPointLocation = 25;
            RftVector vector = new RftVector(map, firstPointLocation, firstPointLocation);
            
            Assert.AreEqual(map, RftVector.Map);
        }

        [TestMethod]
        public void RftWrapsPoints()
        {
            int mapDimensions = 100;
            Rft map = new Rft(mapDimensions, mapDimensions);
            for (int i = 0; i < 300; i++)
            {
                RftVector vectorTest = new RftVector(map, i, i);
                int wrappedDimension;

                if ((i % mapDimensions) > mapDimensions / 2)
                {
                    wrappedDimension = (i % mapDimensions) - (mapDimensions);
                }
                else
                {
                    wrappedDimension = (i % mapDimensions);
                }
                
                Assert.AreEqual(wrappedDimension, vectorTest.X);
                Assert.AreEqual(wrappedDimension, vectorTest.Y);
                Assert.AreEqual(mapDimensions, map.Height);
                Assert.AreEqual(mapDimensions, map.Width);
            }
        }
        
        [TestMethod]
        public void RftCanChangeMapSize()
        {
            int mapDimensions = 100;
            int pointDimensions = 150;
            int mapDelta = 200;

            int wrappedDimension;
            if ((pointDimensions % mapDimensions) > mapDimensions / 2)
            {
                wrappedDimension = (pointDimensions % mapDimensions) - (mapDimensions);
            }
            else
            {
                wrappedDimension = (pointDimensions % mapDimensions);
            }
            
            Rft map = new Rft(mapDimensions, mapDimensions);
            RftVector vectorTest = new RftVector(map, pointDimensions, pointDimensions);
            
            Assert.AreEqual(wrappedDimension, vectorTest.X);
            Assert.AreEqual(wrappedDimension, vectorTest.Y);
            Assert.AreEqual(mapDimensions, map.Height);
            Assert.AreEqual(mapDimensions, map.Width);
            
            RftVector.Map = new Rft(mapDelta, mapDelta);
            
            if ((pointDimensions % mapDimensions) > mapDimensions / 2)
            {
                wrappedDimension = (pointDimensions % mapDimensions) - (mapDimensions);
            }
            else
            {
                wrappedDimension = (pointDimensions % mapDimensions);
            }
            
            Assert.AreEqual(wrappedDimension, vectorTest.X);
            Assert.AreEqual(wrappedDimension, vectorTest.Y);
            Assert.AreEqual(mapDelta, RftVector.Map.Height);
            Assert.AreEqual(mapDelta, RftVector.Map.Height);
        }
        
    }
}