using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore;
using SubterfugeCore.Core.Entities.Locations;
using SubterfugeCore.Entities;
using SubterfugeCore.Players;
using SubterfugeCore.Timing;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class GamestateTest
    {
        GameState state;
        Player player1;
        Outpost outpost;
        Sub tempSub;


        [TestInitialize]
        public void setup()
        {
            state = new GameState();
            player1 = new Player(1);
            outpost = new Outpost(new Vector2(0, 0), player1, OutpostType.GENERATOR);
            outpost.addDrillers(10);
            tempSub = new Sub(outpost, outpost, new GameTick(), 10, player1);
        }

        [TestMethod]
        public void constructor()
        {
            Assert.IsNotNull(state);
        }

        [TestMethod]
        public void getCurrentTick()
        {
            Assert.AreEqual(0, state.getCurrentTick().getTick());
        }

        [TestMethod]
        public void getStartTick()
        {
            Assert.AreEqual(0, state.getStartTick().getTick());
        }

        [TestMethod]
        public void getSubList()
        {
            //Ensure the sub list is empty
            Assert.AreEqual(0, state.getSubList().Count);
        }

        [TestMethod]
        public void getOutposts()
        {
            // Ensure that no outposts are generated from a default state
            Assert.IsTrue(state.getOutposts().Count == 0);
        }

        [TestMethod]
        public void getPlayers()
        {
            // Ensure that no players are added in a default state
            Assert.IsTrue(state.getPlayers().Count == 0);
        }

        [TestMethod]
        public void addSub()
        {
            //Ensure the sub list is empty
            Assert.AreEqual(0, state.getSubList().Count);

            state.addSub(this.tempSub);

            //Ensure the sub was added
            Assert.AreEqual(1, state.getSubList().Count);
            Assert.AreEqual(tempSub, state.getSubList()[0]);
        }

        [TestMethod]
        public void removeSub()
        {
            //Ensure the sub list is empty
            addSub();

            state.removeSub(tempSub);

            //Ensure the sub list is empty
            Assert.AreEqual(0, state.getSubList().Count);
        }

        [TestMethod]
        public void subExists()
        {
            //Ensure the sub is not in the list
            Assert.IsTrue(!state.subExists(tempSub));

            //Ensure the sub list is empty
            addSub();


            //Ensure the sub is in the list
            Assert.IsTrue(state.subExists(tempSub));

            state.removeSub(tempSub);

            //Ensure the sub is not in the list
            Assert.IsTrue(!state.subExists(tempSub));
        }

        [TestMethod]
        public void getSubsOnPath()
        {
            //Ensure the sub list is empty
            addSub();


            //Ensure the sub is on the path between the outposts.
            Assert.AreEqual(tempSub, state.getSubsOnPath(outpost, outpost)[0]);
        }

        [TestMethod]
        public void getPlayerSubs()
        {
            //Ensure the sub list is empty
            Assert.AreEqual(0, state.getPlayerSubs(player1).Count);

            // Add a sub
            addSub();

            //Ensure the sub is returned.
            Assert.AreEqual(tempSub, state.getPlayerSubs(player1)[0]);
        }

        [TestMethod]
        public void getPlayerOutposts()
        {
            // No current way to test this.
        }

    }
}
