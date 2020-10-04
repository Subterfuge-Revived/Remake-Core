using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class ProduceNeptuniumEvent_test
    {
        Player owner;
        Outpost mine;

        [TestInitialize]
        public void Setup()
        {
            RftVector.Map = new Rft(100, 100);
            owner = new Player(1);
            mine = new Outpost(new RftVector(0, 0), owner, OutpostType.Mine);
        }

        [TestMethod]
        public void NoNeptuniumAtStart()
        {
            Assert.AreEqual(0, owner.getNeptunium());
        }
        
        [TestMethod]
        public void CanCreateEvent()
        {
            GameTick tick = new GameTick();
            ProduceNeptuniumEvent npEvent = new ProduceNeptuniumEvent(mine, tick);
            Assert.IsNotNull(npEvent);
            Assert.AreEqual(tick, npEvent.GetTick());
        }
        
        [TestMethod]
        public void PlayerObtainsNeptunium()
        {
            GameTick tick = new GameTick();
            ProduceNeptuniumEvent npEvent = new ProduceNeptuniumEvent(mine, tick);
            Assert.IsNotNull(npEvent);
            Assert.AreEqual(tick, npEvent.GetTick());
            npEvent.ForwardAction();
            
            // player should not have 0 NP
            Assert.IsFalse(0 == owner.getNeptunium());
        }
        
        [TestMethod]
        public void CanUndoEvent()
        {
            GameTick tick = new GameTick();
            ProduceNeptuniumEvent npEvent = new ProduceNeptuniumEvent(mine, tick);
            Assert.IsNotNull(npEvent);
            Assert.AreEqual(tick, npEvent.GetTick());
            npEvent.ForwardAction();
            
            // player should not have 0 NP
            Assert.IsFalse(0 == owner.getNeptunium());

            npEvent.BackwardAction();
            
            // player should have 0 NP
            Assert.IsTrue(0 == owner.getNeptunium());
        }
    }
}