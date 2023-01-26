﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents;
using SubterfugeCore.Core.GameEvents.NaturalGameEvents.outpost;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeCoreTest
{
	
	[TestClass]
	public class DrillMineTest
	{
		Game _game;
		TimeMachine _tm;
		Player _p;
		Outpost _o1, _o2;
		GameRoomEvent _model1, _model2;
		TestUtils testUtils = new TestUtils();

		[TestInitialize]
		public void Setup()
		{
			_p = new Player("Player 1");
			List<Player> playerlist = new List<Player>();
			playerlist.Add(_p);
			GameConfiguration config = testUtils.GetDefaultGameConfiguration(playerlist);
			config.MapConfiguration.OutpostsPerPlayer = 12;
			_game = new Game(config);
			_tm = _game.TimeMachine;
			_o1 = _tm.GetState().GetPlayerOutposts(_p)[0];
			_o2 = _tm.GetState().GetPlayerOutposts(_p)[1];
			_model1 = new GameRoomEvent()
			{
				GameEventData = new GameEventData()
				{
					OccursAtTick = 10,
					EventData = new DrillMineEventData()
					{
						SourceId = _o1.GetComponent<IdentityManager>().GetId()
					},
				},
				Id = Guid.NewGuid().ToString(),
			};
			_model2 = new GameRoomEvent()
			{
				GameEventData = new GameEventData()
				{
					OccursAtTick = 20,
					EventData = new DrillMineEventData()
					{
						SourceId = _o2.GetComponent<IdentityManager>().GetId()
					},
				},
				Id = Guid.NewGuid().ToString(),
			};
		}

		[TestMethod]
		public void DrillMineEventTest()
		{
			Assert.IsNotNull(_model1);
		}

		[TestMethod]
		public void ExtractFromGameEventModel()
		{
			DrillMineEvent drillMine = new DrillMineEvent(_model1);
			IEntity combatable = _tm.GetState().GetEntity(drillMine.GetEventData().SourceId);
			Assert.IsNotNull(combatable);
			Assert.IsTrue(combatable is Outpost);
			Assert.IsFalse(combatable is Mine);
		}

		[TestMethod]
		public void CannotDrillWithInsufficientDrillers()
		{
			DrillMineEvent drillMine = new DrillMineEvent(_model1);
			_tm.AddEvent(drillMine);
			_tm.Advance(10);
			Assert.IsFalse(drillMine.WasEventSuccessful());
		}

		[TestMethod]
		public void DrillFirstMine()
		{
			_o1.GetComponent<DrillerCarrier>().AddDrillers(50);
			DrillMineEvent drillMine = new DrillMineEvent(_model1);
			_tm.AddEvent(drillMine);
			_tm.Advance(10);
			Assert.IsTrue(drillMine.WasEventSuccessful());
			Assert.IsFalse(_tm.GetState().GetOutposts().Contains(_o1));
		}

		[TestMethod]
		public void CannotDrillMineOrDestroyedOutpost()
		{
			DrillMineEvent drillMine = new DrillMineEvent(_model1);
			DrillMineEvent drillSecondMine = new DrillMineEvent(_model2);
			DrillMineEvent drillMineAgain = new DrillMineEvent(new GameRoomEvent()
			{
				GameEventData = new GameEventData()
				{
					OccursAtTick = 25,
					EventData = new DrillMineEventData()
					{
						SourceId = _o1.GetComponent<IdentityManager>().GetId()
					},
				},
				Id = Guid.NewGuid().ToString(),
			});
			_o1.GetComponent<DrillerCarrier>().AddDrillers(150);
			_o2.GetComponent<DrillerCarrier>().AddDrillers(300);
			_o2.GetComponent<DrillerCarrier>().Destroy();
			_tm.AddEvent(drillMine);
			_tm.AddEvent(drillMineAgain);
			_tm.AddEvent(drillSecondMine);
			_tm.Advance(25);
			Assert.IsFalse(drillMineAgain.WasEventSuccessful());
			Assert.IsFalse(drillSecondMine.WasEventSuccessful());
		}

		[TestMethod]
		public void MiningCostIncreases()
		{
			_o1.GetComponent<DrillerCarrier>().AddDrillers(50);
			_o2.GetComponent<DrillerCarrier>().AddDrillers(50);
			DrillMineEvent drillFirstMine = new DrillMineEvent(_model1);
			DrillMineEvent drillSecondMine = new DrillMineEvent(_model2);
			_tm.AddEvent(drillFirstMine);
			_tm.AddEvent(drillSecondMine);
			_tm.Advance(20);
			Assert.IsTrue(drillFirstMine.WasEventSuccessful());
			Assert.IsFalse(drillSecondMine.WasEventSuccessful());
			_tm.Rewind(20);
			_o2.GetComponent<DrillerCarrier>().AddDrillers(100);
			_tm.Advance(20);
			Assert.IsTrue(drillSecondMine.WasEventSuccessful());
			Assert.AreEqual(200, _o1.GetComponent<DrillerCarrier>().GetOwner().GetRequiredDrillersToMine());
		}

		[TestMethod]
		public void NeptuniumProductionEventCreated()
		{
			_o1.GetComponent<DrillerCarrier>().AddDrillers(50);
			DrillMineEvent drillMine = new DrillMineEvent(_model1);
			_tm.AddEvent(drillMine);
			_tm.Advance(10);
			Assert.IsTrue(_tm.GetQueuedEvents().Exists(IsNeptuniumProductionEvent));
		}

		[TestMethod]
		public void RegularNeptuniumProduction()
		{
			_o1.GetComponent<DrillerCarrier>().AddDrillers(50);
			DrillMineEvent drillMine = new DrillMineEvent(_model1);
			_tm.AddEvent(drillMine);
			_tm.Advance(10 + 1440 / (int)GameTick.MinutesPerTick);
			Assert.AreEqual(12, _o1.GetComponent<DrillerCarrier>().GetOwner().GetNeptunium());
		}

		[TestMethod]
		public void RewindTest()
		{
			_o1.GetComponent<DrillerCarrier>().AddDrillers(50);
			DrillMineEvent drillMine = new DrillMineEvent(_model1);
			_tm.AddEvent(drillMine);
			_tm.Advance(10 + 1440 / (int)GameTick.MinutesPerTick);
			_tm.Rewind(10 + 1440 / (int)GameTick.MinutesPerTick);
			Assert.IsTrue(_tm.GetState().GetOutposts().Contains(_o1));
			Assert.AreEqual(0, _p.GetNeptunium());
			Assert.AreEqual(0, _tm.GetCurrentTick().GetTick());
			Assert.AreEqual(Constants.InitialDrillersPerOutpost + 50, _o1.GetComponent<DrillerCarrier>().GetDrillerCount());
		}

		private bool IsNeptuniumProductionEvent(GameEvent e)
		{
			return e is NeptuniumProductionEvent;
		}
	}
}
