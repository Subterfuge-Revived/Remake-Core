using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class GamestateTest
    {
        GameState _state;
        Rft _map;
        Player _player1;
        Outpost _outpost;
        Sub _tempSub;


        [TestInitialize]
        public void Setup()
        {
            _player1 = new Player("1");
            List<Player> players = new List<Player>();
            
            GameConfiguration config = new GameConfiguration(players, DateTime.Now, new MapConfiguration(players));
            Assert.IsNotNull(config);
            
            _state = new GameState(config);
            _map = new Rft(300,300);
            _outpost = new Outpost("0",new RftVector(_map, 0, 0), _player1, OutpostType.Generator);
            _outpost.AddDrillers(10);
            _tempSub = new Sub("1", _outpost, _outpost, new GameTick(), 10, _player1);
        }

        [TestMethod]
        public void Constructor()
        {
            Assert.IsNotNull(_state);
        }

        [TestMethod]
        public void GetCurrentTick()
        {
            Assert.AreEqual(0, _state.GetCurrentTick().GetTick());
        }

        [TestMethod]
        public void GetStartTick()
        {
            Assert.AreEqual(0, _state.GetStartTick().GetTick());
        }

        [TestMethod]
        public void GetSubList()
        {
            //Ensure the sub list is empty
            Assert.AreEqual(0, _state.GetSubList().Count);
        }

        [TestMethod]
        public void GetOutposts()
        {
            // Ensure that no outposts are generated from a default state
            Assert.IsTrue(_state.GetOutposts().Count == 0);
        }

        [TestMethod]
        public void GetPlayers()
        {
            // Ensure that no players are added in a default state
            Assert.IsTrue(_state.GetPlayers().Count == 0);
        }

        [TestMethod]
        public void AddSub()
        {
            //Ensure the sub list is empty
            Assert.AreEqual(0, _state.GetSubList().Count);

            _state.AddSub(this._tempSub);

            //Ensure the sub was added
            Assert.AreEqual(1, _state.GetSubList().Count);
            Assert.AreEqual(_tempSub, _state.GetSubList()[0]);
        }

        [TestMethod]
        public void RemoveSub()
        {
            //Ensure the sub list is empty
            AddSub();

            _state.RemoveSub(_tempSub);

            //Ensure the sub list is empty
            Assert.AreEqual(0, _state.GetSubList().Count);
        }

        [TestMethod]
        public void SubExists()
        {
            //Ensure the sub is not in the list
            Assert.IsTrue(!_state.SubExists(_tempSub));

            //Ensure the sub list is empty
            AddSub();


            //Ensure the sub is in the list
            Assert.IsTrue(_state.SubExists(_tempSub));

            _state.RemoveSub(_tempSub);

            //Ensure the sub is not in the list
            Assert.IsTrue(!_state.SubExists(_tempSub));
        }

        [TestMethod]
        public void getSubsOnPath()
        {
            //Ensure the sub list is empty
            AddSub();


            //Ensure the sub is on the path between the outposts.
            Assert.AreEqual(_tempSub, _state.getSubsOnPath(_outpost, _outpost)[0]);
        }

        [TestMethod]
        public void GetPlayerSubs()
        {
            //Ensure the sub list is empty
            Assert.AreEqual(0, _state.GetPlayerSubs(_player1).Count);

            // Add a sub
            AddSub();

            //Ensure the sub is returned.
            Assert.AreEqual(_tempSub, _state.GetPlayerSubs(_player1)[0]);
        }

        [TestMethod]
        public void GetPlayerOutposts()
        {
            // No current way to test this.
        }

    }
}
