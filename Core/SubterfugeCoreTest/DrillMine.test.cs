using System;
using System.Collections.Generic;
using System.Text;
using GameEventModels;
using Google.Protobuf;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents;
using SubterfugeCore.Core.GameEvents.NaturalGameEvents.outpost;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;
using SubterfugeRemakeService;

namespace SubterfugeCoreTest
{
	[TestClass]
	public class DrillMineTest
	{
		Game game;
		TimeMachine tm;
		Player p;
		Outpost o1, o2;
		GameEventModel model1, model2;

		[TestInitialize]
		public void Setup()
		{
			p = new Player("Player 1");
			List<Player> playerlist = new List<Player>();
			playerlist.Add(p);
			GameConfiguration config = new GameConfiguration(playerlist);
			config.MapConfiguration.OutpostsPerPlayer = 12;
			game = new Game(config);
			tm = game.TimeMachine;
			o1 = tm.GetState().GetPlayerOutposts(p)[0];
			o2 = tm.GetState().GetPlayerOutposts(p)[1];
			model1 = new GameEventModel()
			{
				EventData = new DrillMineEventData()
				{
					SourceId = o1.GetId()
				}.ToByteString(),
				EventId = Guid.NewGuid().ToString(),
				EventType = EventType.DrillMineEvent,
				OccursAtTick = 100
			};
			model2 = new GameEventModel()
			{
				EventData = new DrillMineEventData()
				{
					SourceId = o2.GetId()
				}.ToByteString(),
				EventId = Guid.NewGuid().ToString(),
				EventType = EventType.DrillMineEvent,
				OccursAtTick = 200
			};
		}

		[TestMethod]
		public void DrillMineEventTest()
		{
			Assert.IsNotNull(model1);
		}

		[TestMethod]
		public void ExtractFromGameEventModel()
		{
			DrillMineEvent drillMine = new DrillMineEvent(model1);
			ICombatable combatable = tm.GetState().GetCombatableById(drillMine.GetEventData().SourceId);
			Assert.IsNotNull(combatable);
			Assert.IsTrue(combatable is Outpost);
			Assert.IsFalse(combatable is Mine);
		}

		[TestMethod]
		public void CannotDrillWithInsufficientDrillers()
		{
			DrillMineEvent drillMine = new DrillMineEvent(model1);
			tm.AddEvent(drillMine);
			tm.Advance(100);
			Assert.IsFalse(drillMine.WasEventSuccessful());
		}

		[TestMethod]
		public void DrillFirstMine()
		{
			o1.AddDrillers(50);
			DrillMineEvent drillMine = new DrillMineEvent(model1);
			tm.AddEvent(drillMine);
			tm.Advance(100);
			Assert.IsTrue(drillMine.WasEventSuccessful());
			Assert.IsFalse(tm.GetState().GetOutposts().Contains(o1));
		}

		[TestMethod]
		public void CannotDrillMineOrDestroyedOutpost()
		{
			DrillMineEvent drillMine = new DrillMineEvent(model1);
			DrillMineEvent drillSecondMine = new DrillMineEvent(model2);
			DrillMineEvent drillMineAgain = new DrillMineEvent(new GameEventModel()
			{
				EventData = new DrillMineEventData()
				{
					SourceId = o1.GetId()
				}.ToByteString(),
				EventId = Guid.NewGuid().ToString(),
				EventType = EventType.DrillMineEvent,
				OccursAtTick = 200
			});
			o1.AddDrillers(150);
			o2.AddDrillers(300);
			o2.destroyOutpost();
			tm.AddEvent(drillMine);
			tm.AddEvent(drillMineAgain);
			tm.AddEvent(drillSecondMine);
			tm.Advance(200);
			Assert.IsFalse(drillMineAgain.WasEventSuccessful());
			Assert.IsFalse(drillSecondMine.WasEventSuccessful());
		}

		[TestMethod]
		public void MiningCostIncreases()
		{
			o1.AddDrillers(50);
			o2.AddDrillers(50);
			DrillMineEvent drillFirstMine = new DrillMineEvent(model1);
			DrillMineEvent drillSecondMine = new DrillMineEvent(model2);
			tm.AddEvent(drillFirstMine);
			tm.AddEvent(drillSecondMine);
			tm.Advance(200);
			Assert.IsTrue(drillFirstMine.WasEventSuccessful());
			Assert.IsFalse(drillSecondMine.WasEventSuccessful());
			tm.Rewind(200);
			o2.AddDrillers(100);
			tm.Advance(200);
			Assert.IsTrue(drillSecondMine.WasEventSuccessful());
			Assert.IsTrue(p.GetRequiredDrillersToMine() == 200);
		}

		[TestMethod]
		public void NeptuniumProductionEventCreated()
		{
			o1.AddDrillers(50);
			DrillMineEvent drillMine = new DrillMineEvent(model1);
			tm.AddEvent(drillMine);
			tm.Advance(100);
			Assert.IsTrue(tm.GetQueuedEvents().Exists(IsNeptuniumProductionEvent));
		}

		[TestMethod]
		public void RegularNeptuniumProduction()
		{
			o1.AddDrillers(50);
			DrillMineEvent drillMine = new DrillMineEvent(model1);
			tm.AddEvent(drillMine);
			tm.Advance(100 + 1440 / (int)GameTick.MINUTES_PER_TICK);
			Assert.IsTrue(p.GetNeptunium() == 12);
		}

		[TestMethod]
		public void RewindTest()
		{
			o1.AddDrillers(50);
			DrillMineEvent drillMine = new DrillMineEvent(model1);
			tm.AddEvent(drillMine);
			tm.Advance(100 + 1440 / (int)GameTick.MINUTES_PER_TICK);
			tm.Rewind(100 + 1440 / (int)GameTick.MINUTES_PER_TICK);
			Assert.IsTrue(tm.GetState().GetOutposts().Contains(o1));
			Assert.IsTrue(p.GetNeptunium() == 0);
			Assert.IsTrue(o1.GetDrillerCount() == 50);
		}

		private bool IsNeptuniumProductionEvent(GameEvent e)
		{
			return e is NeptuniumProductionEvent;
		}
	}
}
