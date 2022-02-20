using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class RftVectorTest
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
            new RftVector(map, firstPointLocation, firstPointLocation);
            
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
            wrappedDimension = (pointDimensions % mapDimensions);

            Rft map = new Rft(mapDimensions, mapDimensions);
            RftVector vectorTest = new RftVector(map, pointDimensions, pointDimensions);
            
            Assert.AreEqual(wrappedDimension, vectorTest.X);
            Assert.AreEqual(wrappedDimension, vectorTest.Y);
            Assert.AreEqual(mapDimensions, map.Height);
            Assert.AreEqual(mapDimensions, map.Width);
            
            RftVector.Map = new Rft(mapDelta, mapDelta);

            wrappedDimension = (pointDimensions % mapDimensions);

            Assert.AreEqual(wrappedDimension, vectorTest.X);
            Assert.AreEqual(wrappedDimension, vectorTest.Y);
            Assert.AreEqual(mapDelta, RftVector.Map.Height);
            Assert.AreEqual(mapDelta, RftVector.Map.Height);
        }

        [TestMethod]
        public void CanGetMagnitude()
        {
            int mapDimension = 100;
            Rft map = new Rft(mapDimension, mapDimension);
            
            RftVector vector = new RftVector(map, 0, 1);
            Assert.AreEqual(1, vector.Magnitude());
            
            for (int i = 0; i < 300; i++)
            {
                vector = new RftVector(map, 0, i);
                float position = vector.Y;
                Assert.AreEqual(Math.Abs(position), vector.Magnitude());
            }

            int somePosition = 152;
            RftVector vectorTwo = new RftVector(somePosition, somePosition);
            float wrappedPosition = vectorTwo.X;
            
            Assert.AreEqual((float)Math.Sqrt(wrappedPosition*wrappedPosition + wrappedPosition*wrappedPosition), vectorTwo.Magnitude());
        }

        [TestMethod]
        public void CanGetVector2()
        {
            int mapDimension = 100;
            Rft map = new Rft(mapDimension, mapDimension);

            for (int i = 0; i < 300; i++)
            {
                RftVector vector = new RftVector(map, 0, i);
            
                float vectorX = vector.X;
                float vectorY = vector.Y;
                Vector2 duplicate = new Vector2(vectorX, vectorY);

                Vector2 derived = vector.ToVector2();
            
                Assert.AreEqual(duplicate.X, derived.X);
                Assert.AreEqual(duplicate.Y, derived.Y);
            }
        }

        [TestMethod]
        public void CanNormalize()
        {
            int mapDimension = 100;
            Rft map = new Rft(mapDimension, mapDimension);
            
            RftVector vector = new RftVector(map, 0, 1);
            Assert.AreEqual(1, vector.Normalize().Length());
            
            for (int i = 0; i < 300; i++)
            {
                vector = new RftVector(map, 0, i);
                Assert.AreEqual(1, vector.Normalize().Length());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CannotMakeVectorWithoutSettingMap()
        {
            RftVector.Map = null;
            new RftVector(123, 123);
        }

        [TestMethod]
        public void CanSubtractVectors()
        {
            int mapDimensions = 100;
            int mapRadius = mapDimensions / 2;
            Rft map = new Rft(mapDimensions, mapDimensions);
            
            // Create a vector at the edge of the map
            RftVector rightEdgeMap = new RftVector(map, mapRadius - 1, 0);
            
            // Create a vector at the other edge of the map
            RftVector leftEdgeMap = new RftVector(-mapRadius + 1, 0);
            
            // Determine the distance between them. Expect 2.
            RftVector difference = rightEdgeMap - leftEdgeMap;
            Assert.AreEqual(2, difference.Magnitude());
            Assert.AreEqual(-2, difference.X);
            Assert.AreEqual(0, difference.Y);
        }

        [TestMethod]
        public void CanAddVectors()
        {
            int mapDimensions = 100;
            int mapRadius = mapDimensions / 2;
            Rft map = new Rft(mapDimensions, mapDimensions);
            
            // Create a vector at the edge of the map
            RftVector rightEdgeMap = new RftVector(map, mapRadius - 1, 0);
            
            // Create a vector that makes the edge go over the boarder
            RftVector forceWrap = new RftVector(2, 0);
            
            // Determine location of new point. Expect opposite map edge.
            RftVector sum = rightEdgeMap + forceWrap;
            Assert.AreEqual(mapRadius - 1, sum.Magnitude());
            Assert.AreEqual(-(mapRadius - 1), sum.X);
            Assert.AreEqual(0, sum.Y);
        }

    }
}