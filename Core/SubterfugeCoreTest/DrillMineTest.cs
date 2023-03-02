using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Test
{
	
	[TestClass]
	public class DrillMineTest
	{
		Game _game;
		TimeMachine _tm;
		Player _p = new Player(new SimpleUser() { Id = "1" });
		Outpost _o1, _o2;
		GameRoomEvent _model1, _model2;
		TestUtils testUtils = new TestUtils();

		public List<Player> players;

		[TestInitialize]
		public void Setup()
		{
			players = new List<Player>()
			{
				_p,
			};
			GameConfiguration config = testUtils.GetDefaultGameConfiguration(players);
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
					EventDataType = EventDataType.DrillMineEventData,
					SerializedEventData = JsonConvert.SerializeObject(new DrillMineEventData()
					{
						SourceId = _o1.GetComponent<IdentityManager>().GetId()
					}),
				},
				Id = Guid.NewGuid().ToString(),
			};
			_model2 = new GameRoomEvent()
			{
				GameEventData = new GameEventData()
				{
					OccursAtTick = 20,
					EventDataType = EventDataType.DrillMineEventData,
					SerializedEventData = JsonConvert.SerializeObject(new DrillMineEventData()
					{
						SourceId = _o2.GetComponent<IdentityManager>().GetId()
					}),
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
			_o1.GetComponent<DrillerCarrier>().AlterDrillers(50);
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
					EventDataType = EventDataType.DrillMineEventData,
					SerializedEventData = JsonConvert.SerializeObject(new DrillMineEventData()
					{
						SourceId = _o1.GetComponent<IdentityManager>().GetId()
					}),
				},
				Id = Guid.NewGuid().ToString(),
			});
			_o1.GetComponent<DrillerCarrier>().AlterDrillers(150);
			_o2.GetComponent<DrillerCarrier>().AlterDrillers(300);
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
			_o1.GetComponent<DrillerCarrier>().AlterDrillers(50);
			_o2.GetComponent<DrillerCarrier>().AlterDrillers(50);
			DrillMineEvent drillFirstMine = new DrillMineEvent(_model1);
			DrillMineEvent drillSecondMine = new DrillMineEvent(_model2);
			_tm.AddEvent(drillFirstMine);
			_tm.AddEvent(drillSecondMine);
			_tm.Advance(20);
			Assert.IsTrue(drillFirstMine.WasEventSuccessful());
			Assert.IsFalse(drillSecondMine.WasEventSuccessful());
			_tm.Rewind(20);
			_o2.GetComponent<DrillerCarrier>().AlterDrillers(100);
			_tm.Advance(20);
			Assert.IsTrue(drillSecondMine.WasEventSuccessful());
			Assert.AreEqual(200, _o1.GetComponent<DrillerCarrier>().GetOwner().GetRequiredDrillersToMine());
		}

		[TestMethod]
		public void RegularNeptuniumProduction()
		{
			_o1.GetComponent<DrillerCarrier>().AlterDrillers(50);
			DrillMineEvent drillMine = new DrillMineEvent(_model1);
			_tm.AddEvent(drillMine);
			_tm.Advance(10);
			NeptuniumProducer resourceProducer = drillMine.CreatedMine.GetComponent<NeptuniumProducer>();
			_tm.GoTo(resourceProducer.Producer.GetNextProductionTick());
			Assert.AreEqual(12, _o1.GetComponent<DrillerCarrier>().GetOwner().GetNeptunium());
		}

		[TestMethod]
		public void RewindTest()
		{
			var drillerCarrier = _o1.GetComponent<DrillerCarrier>();
			drillerCarrier.AlterDrillers(50);
			var initialDrillers = drillerCarrier.GetDrillerCount();
			
			DrillMineEvent drillMine = new DrillMineEvent(_model1);
			_tm.AddEvent(drillMine);
			_tm.GoTo(drillMine.OccursAt);
			
			NeptuniumProducer resourceProducer = drillMine.CreatedMine.GetComponent<NeptuniumProducer>();
			_tm.GoTo(resourceProducer.Producer.GetNextProductionTick());
			
			// Rewind to before the mine was drilled
			_tm.GoTo(drillMine.OccursAt.Rewind(5));
			Assert.IsTrue(_tm.GetState().GetOutposts().Contains(_o1));
			Assert.AreEqual(0, _p.GetNeptunium());
			Assert.AreEqual(5, _tm.GetCurrentTick().GetTick());
			Assert.AreEqual(initialDrillers, _o1.GetComponent<DrillerCarrier>().GetDrillerCount());
		}
	}
}
