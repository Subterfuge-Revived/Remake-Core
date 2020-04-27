using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class ShieldManager_test
    {
        private Game _game;
        private ShieldManager _shieldManager;

        [TestInitialize]
        public void Setup()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            players.Add(new Player(2));

            GameConfiguration config = new GameConfiguration(players);
            config.Seed = 1234;
            config.OutpostsPerPlayer = 1;
            config.DormantsPerPlayer = 1;
            config.MaxiumumOutpostDistance = 100;
            config.MinimumOutpostDistance = 10;

            _game = new Game(config);
            _shieldManager = new ShieldManager();
        }

        [TestMethod]
        public void CanCombatShields()
        {
            _shieldManager.SetShields(10);
            int initialShields = _shieldManager.GetShields();
            _shieldManager.CombatShields(5);
            Assert.AreEqual(initialShields - 5, _shieldManager.GetShields());
        }
        
        [TestMethod]
        public void CanAddShields()
        {
            int initialShield = _shieldManager.GetShields();
            _shieldManager.SetShields(_shieldManager.GetShields() + 1);
            Assert.AreEqual(initialShield + 1, _shieldManager.GetShields());
        }
        
        [TestMethod]
        public void CanSetShields()
        {
            
            _shieldManager.SetShields(1);
            Assert.AreEqual(1, _shieldManager.GetShields());
        }
        
        [TestMethod]
        public void CannotHaveOverShieldCapacity()
        {
            _shieldManager.SetShieldCapacity(100);
            _shieldManager.SetShields(105);
            
            Assert.AreEqual(_shieldManager.GetShieldCapacity(), _shieldManager.GetShields());
        }
        
        [TestMethod]
        public void CannotHaveNegativeShield()
        {
            _shieldManager.SetShields(10);
            int initialShields = _shieldManager.GetShields();
            _shieldManager.CombatShields(15);
            Assert.AreEqual(0, _shieldManager.GetShields());
        }

        [TestMethod]
        public void CanToggleSheilds()
        {
            bool initialState = _shieldManager.IsShieldActive();
            _shieldManager.ToggleShield();
            Assert.AreEqual(!initialState, _shieldManager.IsShieldActive());
        }

        [TestMethod]
        public void CombatsIgnoreShieldWhenDisabled()
        {
            if (_shieldManager.IsShieldActive())
            {
                _shieldManager.ToggleShield();
            }

            _shieldManager.SetShieldCapacity(100);
            _shieldManager.SetShields(100);
            bool didWin = _shieldManager.CombatShields(205);
            Assert.AreEqual(100, _shieldManager.GetShields());
            Assert.AreEqual(false, _shieldManager.IsActive);
            Assert.AreEqual(true, didWin);
        }


        [TestMethod]
        public void CombatReturnsFalseWhenLosing()
        {
            _shieldManager.SetShields(10);
            int initialShields = _shieldManager.GetShields();
            bool didWin = _shieldManager.CombatShields(5);
            Assert.AreEqual(5, _shieldManager.GetShields());
            Assert.AreEqual(false, didWin);
        }

        [TestMethod]
        public void ShieldsRecharge()
        {
            _shieldManager.SetShields(0);
            _shieldManager.SetShieldCapacity(100);
            Game.TimeMachine.Advance(20);
            Assert.IsTrue(_shieldManager.GetShields() > 0);
        }

        [TestMethod]
        public void ShieldRechargeRateOfZero()
        {
            _shieldManager.SetShields(0);
            _shieldManager.SetShieldCapacity(100);
            _shieldManager.RechargeRate = 0;
            Game.TimeMachine.Advance(20);
            Assert.AreEqual(0, _shieldManager.GetShields());
        }

        [TestMethod]
        public void GoingBackInTimeRevertsShields()
        {
            _shieldManager.SetShields(10);
            Game.TimeMachine.Advance(5);
            int initialShields = _shieldManager.GetShields();
            bool didWin = _shieldManager.CombatShields(5);
            int finalShields = _shieldManager.GetShields();
            Assert.AreEqual(false, didWin);
            Assert.AreEqual(5, initialShields - finalShields);
            Game.TimeMachine.Rewind(1);
            Assert.AreEqual(true, _shieldManager.GetShields() > finalShields);
        }
    }
}