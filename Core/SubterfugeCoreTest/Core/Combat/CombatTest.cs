using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.Entities.Specialists.Heroes;
using Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Core.Topologies;

namespace Subterfuge.Remake.Test.Core.Combat
{
	[TestClass]
    public class CombatTest
    {

	    private Game game = Game.Bare();
	    private Player attackingPlayer;
	    private Outpost attackingOutpost;
	        
	    private Player defendingPlayer;
	    private Outpost defendingOutpost;
	    
	    
        [TestInitialize]
        public void Setup()
        {
	        game = Game.Bare();
	        attackingPlayer = new Player(new SimpleUser() { Id = "1", Username = "Attacker" });
	        attackingOutpost = new Generator("1", new RftVector(0, 0), attackingPlayer, game.TimeMachine);
		        
	        defendingPlayer = new Player(new SimpleUser() { Id = "2", Username = "Defender" });
	        defendingOutpost = new Generator("2", new RftVector(0, 1), defendingPlayer, game.TimeMachine);
	        game.TimeMachine.GetState().GetOutposts().AddRange(new List<Outpost>() { attackingOutpost, defendingOutpost });
	        
        	Assert.AreEqual(2, game.TimeMachine.GetState().GetOutposts().Count);
        	Assert.AreEqual(2, game.TimeMachine.GetState().GetPlayers().Count);
        }
        
	    [TestMethod]
	    public void TestFailingDrillerCombat()
	    {
		    SetupDefence(50, 0, new List<SpecialistTypeId>());
		    Attack(30, new List<SpecialistTypeId>());
		    
		    AssertOutpostOwnedBy(defendingOutpost, defendingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 20);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 0);
	    }

	    [TestMethod]
	    public void TestSuccessfulDrillerCombat()
	    {
		    SetupDefence(50, 0, new List<SpecialistTypeId>());
		    Attack(60, new List<SpecialistTypeId>());
		    
		    AssertOutpostOwnedBy(defendingOutpost, attackingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 10);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 0);
		    
		    game.TimeMachine.GoTo(new GameTick(0));
		    AssertOutpostOwnedBy(defendingOutpost, defendingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 50);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 60);
	    }
	    
	    [TestMethod]
	    public void TestAttackFailsIfTie()
	    {
		    SetupDefence(50, 0, new List<SpecialistTypeId>());
		    Attack(50, new List<SpecialistTypeId>());
		    
		    AssertOutpostOwnedBy(defendingOutpost, defendingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 0);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 0);
		    
		    game.TimeMachine.GoTo(new GameTick(0));
		    AssertOutpostOwnedBy(defendingOutpost, defendingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 50);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 50);
	    }
	    
	    [TestMethod]
	    public void TestDrillersAttackingLargerShields()
	    {
		    SetupDefence(20, 50, new List<SpecialistTypeId>());
		    Attack(20, new List<SpecialistTypeId>());
		    
		    AssertOutpostOwnedBy(defendingOutpost, defendingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 20);
		    AssertOutpostShields(defendingOutpost, 30);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 0);
		    
		    game.TimeMachine.GoTo(new GameTick(0));
		    AssertOutpostOwnedBy(defendingOutpost, defendingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 20);
		    AssertOutpostShields(defendingOutpost, 50);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 20);
	    }
	    
	    [TestMethod]
	    public void TestAttackDrainingAllShieldsButNotDrillers()
	    {
		    SetupDefence(20, 50, new List<SpecialistTypeId>());
		    Attack(50, new List<SpecialistTypeId>());
		    
		    AssertOutpostOwnedBy(defendingOutpost, defendingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 20);
		    AssertOutpostShields(defendingOutpost, 0);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 0);
		    
		    game.TimeMachine.GoTo(new GameTick(0));
		    AssertOutpostOwnedBy(defendingOutpost, defendingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 20);
		    AssertOutpostShields(defendingOutpost, 50);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 50);
	    }
	    
	    [TestMethod]
	    public void TestSuccessfulAttackAgainstShields()
	    {
		    SetupDefence(20, 50, new List<SpecialistTypeId>());
		    Attack(71, new List<SpecialistTypeId>());
		    
		    AssertOutpostOwnedBy(defendingOutpost, attackingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 1);
		    AssertOutpostShields(defendingOutpost, 0);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 0);
		    
		    game.TimeMachine.GoTo(new GameTick(0));
		    AssertOutpostOwnedBy(defendingOutpost, defendingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 20);
		    AssertOutpostShields(defendingOutpost, 50);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 71);
	    }
	    
	    [TestMethod]
	    public void TestAttackingWithInfiltrator()
	    {
		    SetupDefence(20, 50, new List<SpecialistTypeId>());
		    Attack(10, new List<SpecialistTypeId>() { SpecialistTypeId.Infiltrator });
		    
		    AssertOutpostOwnedBy(defendingOutpost, defendingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 20);
		    AssertOutpostShields(defendingOutpost, 50 - 10 - 15);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 0);
		    
		    game.TimeMachine.GoTo(new GameTick(0));
		    AssertOutpostOwnedBy(defendingOutpost, defendingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 20);
		    AssertOutpostShields(defendingOutpost, 50);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 10);
	    }
	    
	    [TestMethod]
	    public void CanBustShieldsWithInfiltrator()
	    {
		    SetupDefence(0, 15, new List<SpecialistTypeId>());
		    Attack(10, new List<SpecialistTypeId>() { SpecialistTypeId.Infiltrator });
		    
		    AssertOutpostOwnedBy(defendingOutpost, attackingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 10);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 0);
		    
		    game.TimeMachine.GoTo(new GameTick(0));
		    AssertOutpostOwnedBy(defendingOutpost, defendingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 0);
		    AssertOutpostShields(defendingOutpost, 15);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 10);
	    }
	    
	    [TestMethod]
	    public void MartyrExplodesOnCombat()
	    {
		    SetupDefence(900, 15, new List<SpecialistTypeId>() { SpecialistTypeId.Martyr });
		    Attack(10, new List<SpecialistTypeId>());
		    
		    AssertOutpostOwnedBy(defendingOutpost, defendingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 0);
		    Assert.AreEqual(defendingOutpost.GetComponent<DrillerCarrier>().IsDestroyed(), true);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 0);
		    Assert.AreEqual(attackingOutpost.GetComponent<DrillerCarrier>().IsDestroyed(), true);
		    
		    
		    game.TimeMachine.GoTo(new GameTick(0));
		    AssertOutpostOwnedBy(defendingOutpost, defendingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 900);
		    AssertOutpostShields(defendingOutpost, 15);
		    Assert.AreEqual(defendingOutpost.GetComponent<DrillerCarrier>().IsDestroyed(), false);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 10);
		    Assert.AreEqual(attackingOutpost.GetComponent<DrillerCarrier>().IsDestroyed(), false);
	    }
	    
	    [TestMethod]
	    public void TheifStealsDrillersOnCombat()
	    {
		    SetupDefence(100, 0, new List<SpecialistTypeId>());
		    Attack(100, new List<SpecialistTypeId>() { SpecialistTypeId.Theif });
		    
		    AssertOutpostOwnedBy(defendingOutpost, attackingPlayer);
		    // Steal 10 so enemy loses 10, we gain 10 = 90 vs. 110
		    AssertOutpostDrillers(defendingOutpost, 20);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 0);
		    
		    game.TimeMachine.GoTo(new GameTick(0));
		    AssertOutpostOwnedBy(defendingOutpost, defendingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 100);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 100);
	    }
	    
	    [TestMethod]
	    public void VeteranDestroysDrillersOnCombat()
	    {
		    SetupDefence(100, 0, new List<SpecialistTypeId>());
		    Attack(100, new List<SpecialistTypeId>() { SpecialistTypeId.Veteran });
		    
		    AssertOutpostOwnedBy(defendingOutpost, attackingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 10);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 0);
		    
		    game.TimeMachine.GoTo(new GameTick(0));
		    AssertOutpostOwnedBy(defendingOutpost, defendingPlayer);
		    AssertOutpostDrillers(defendingOutpost, 100);
		    
		    AssertOutpostOwnedBy(attackingOutpost, attackingPlayer);
		    AssertOutpostDrillers(attackingOutpost, 100);
	    }

	    private void SetupDefence(
		    int drillerCount = 10,
		    int shieldCount = 0,
		    List<SpecialistTypeId>? defendingSpecialists = null
	    )
	    {
		    defendingOutpost.GetComponent<ShieldManager>().SetShieldCapacity(shieldCount);
		    defendingOutpost.GetComponent<ShieldManager>().SetShields(shieldCount);
		    defendingOutpost.GetComponent<DrillerCarrier>().SetDrillerCount(drillerCount);
		    
		    defendingOutpost.GetComponent<SpecialistManager>().AllowHireFromLocation();
		    defendingSpecialists?.ForEach(spec =>
		    {
			    defendingOutpost.GetComponent<SpecialistManager>().HireSpecialist(
				    SpecialistFactory.CreateSpecialist(spec, 1, defendingPlayer), game.TimeMachine);
		    });
	    }
	    
	    private void Attack(
		    int drillerCount = 10,
		    List<SpecialistTypeId> attackingSpecialists = null
	    )
	    {
		    attackingOutpost.GetComponent<DrillerCarrier>().SetDrillerCount(drillerCount);
		    
		    attackingOutpost.GetComponent<SpecialistManager>().AllowHireFromLocation();
		    attackingSpecialists?.ForEach(spec =>
		    {
			    attackingOutpost.GetComponent<SpecialistManager>().HireSpecialist(
				    SpecialistFactory.CreateSpecialist(spec, 1, attackingPlayer), game.TimeMachine);
		    });

		    var launchEventData = new LaunchEventData()
		    {
			    DestinationId = "2",
			    DrillerCount = drillerCount,
			    SourceId = "1",
			    SpecialistIds = attackingSpecialists?.ConvertAll(it => it.ToString())
		    };
		    var launchEvent = new LaunchEvent(new GameRoomEvent()
		    {
			    GameEventData = new GameEventData()
			    {
				    OccursAtTick = 1,
				    EventDataType = EventDataType.LaunchEventData,
				    SerializedEventData = JsonConvert.SerializeObject(launchEventData),
			    },
			    Id = "asdf",
			    IssuedBy = attackingPlayer.PlayerInstance,
			    RoomId = "1",
			    TimeIssued = DateTime.UtcNow,
		    });
		    game.TimeMachine.AddEvent(launchEvent);
		    game.TimeMachine.GoTo(launchEvent);
		    game.TimeMachine.Advance(1);
	    }

	    private void AssertOutpostOwnedBy(Outpost outpost, Player owner)
	    {
		    Assert.AreEqual(owner.PlayerInstance.Username, outpost.GetComponent<DrillerCarrier>().GetOwner().PlayerInstance.Username);
	    }

	    private void AssertOutpostDrillers(Outpost outpost, int expectedDrillers)
	    {
		    Assert.AreEqual(expectedDrillers, outpost.GetComponent<DrillerCarrier>().GetDrillerCount());
	    }
	    
	    private void AssertOutpostShields(Outpost outpost, int expectedShields)
	    {
		    Assert.AreEqual(expectedShields, outpost.GetComponent<ShieldManager>().GetShields());
	    }
    }
}