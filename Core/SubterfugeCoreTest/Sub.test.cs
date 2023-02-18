using Microsoft.VisualStudio.TestTools.UnitTesting;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Core.Topologies;

namespace Subterfuge.Remake.Test
{
    [TestClass]
    public class SubTest
    {
	    Rft _map;
        RftVector _location;
        Outpost _outpost;
        GameTick _tick;
        Sub _sub;

        [TestInitialize]
        public void Setup()
        {
	        _map = new Rft(3000, 3000);
            _location = new RftVector(_map, 0, 0);
            _outpost = new Generator("0", _location, new Player("1"));
            _tick = new GameTick(10);
            _sub = new Sub("0", _outpost, _outpost, _tick, 0, new Player("1"));
        }

        [TestMethod]
        public void HasDrillerCarrier()
        {
            Assert.IsNotNull(_sub.GetComponent<DrillerCarrier>());
        }
        
        [TestMethod]
        public void HasSpeedManager()
        {
            Assert.IsNotNull(_sub.GetComponent<SpeedManager>());
        }
        
        [TestMethod]
        public void HasPositionManager()
        {
            Assert.IsNotNull(_sub.GetComponent<PositionManager>());
        }
        
        [TestMethod]
        public void HasSpecialistManager()
        {
            Assert.IsNotNull(_sub.GetComponent<SpecialistManager>());
        }
        
        [TestMethod]
        public void HasIdentityManager()
        {
            Assert.IsNotNull(_sub.GetComponent<IdentityManager>());
        }
        
        [TestMethod]
        public void HasShieldManager()
        {
            Assert.IsNotNull(_sub.GetComponent<ShieldManager>());
        }
        
        [TestMethod]
        public void HasSubLauncher()
        {
            Assert.IsNotNull(_sub.GetComponent<SubLauncher>());
        }
        
        [TestMethod]
        public void HasVisionManager()
        {
            Assert.IsNotNull(_sub.GetComponent<VisionManager>());
        }
    }
}
