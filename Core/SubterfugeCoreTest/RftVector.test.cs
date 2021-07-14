﻿using System;
using System.Numerics;
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
            RftVector vectorFailure = new RftVector(123, 123);
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

        [TestMethod]
        public void DistanceAndDirectionCalculationWraps()
        {
            int mapDimensions = 100;
            Rft map = new Rft(mapDimensions, mapDimensions);

            // Regular distance
            RftVector vector1 = new RftVector(map, -20, -20);
            RftVector vector2 = new RftVector(10, 20);
            Assert.AreEqual(vector1.Distance(vector2), 50f);
            Assert.AreEqual(vector2.Distance(vector1), 50f);
            Assert.AreEqual((vector2 - vector1).ToVector2(), new Vector2(30, 40));
            Assert.AreEqual((vector1 - vector2).ToVector2(), new Vector2(-30, -40));

            // Wrap around left/right
            RftVector vector3 = new RftVector(40, -20);
            RftVector vector4 = new RftVector(-30, 20);
            Assert.AreEqual(vector3.Distance(vector4), 50f);
            Assert.AreEqual(vector4.Distance(vector3), 50f);
            Assert.AreEqual((vector4 - vector3).ToVector2(), new Vector2(30, 40));
            Assert.AreEqual((vector3 - vector4).ToVector2(), new Vector2(-30, -40));

            // Wrap around top/bottom
            RftVector vector5 = new RftVector(-20, 30);
            RftVector vector6 = new RftVector(10, -30);
            Assert.AreEqual(vector5.Distance(vector6), 50f);
            Assert.AreEqual(vector6.Distance(vector5), 50f);
            Assert.AreEqual((vector6 - vector5).ToVector2(), new Vector2(30, 40));
            Assert.AreEqual((vector5 - vector6).ToVector2(), new Vector2(-30, -40));

            // Wrap around left/right and top/bottom
            RftVector vector7 = new RftVector(40, 30);
            RftVector vector8 = new RftVector(-30, -30);
            Assert.AreEqual(vector7.Distance(vector8), 50f);
            Assert.AreEqual(vector8.Distance(vector7), 50f);
            Assert.AreEqual((vector8 - vector7).ToVector2(), new Vector2(30, 40));
            Assert.AreEqual((vector7 - vector8).ToVector2(), new Vector2(-30, -40));
        }
    }
}