using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.Config;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.NaturalGameEvents.outpost;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Test
{
	[TestClass]
	public class FactoryTest
	{
		Game _game;
		TimeMachine _tm;
		Factory _f;
		TestUtils testUtils = new TestUtils();

		[TestInitialize]
		public void Setup()
		{
			List<Player> playerlist = new List<Player>();
			Player player = new Player("Player 1");
			playerlist.Add(player);
			GameConfiguration config = testUtils.GetDefaultGameConfiguration(playerlist);
			config.MapConfiguration.OutpostsPerPlayer = 10;
			_game = new Game(config);
			_tm = _game.TimeMachine;
			foreach (Outpost o in _tm.GetState().GetPlayerOutposts(player))
			{
				if (isFactory(o))
				{
					_f = (Factory)o;
					break;
				}
			}
			Assert.IsNotNull(_f);
		}

		[TestMethod]
		public void InitialProductionEvent()
		{
			Assert.IsTrue(_tm.GetQueuedEvents().Exists(isProductionEvent));
		}

		[TestMethod]
		public void ProductionEventSuccess()
		{
			FactoryProduction p = null;
			foreach (GameEvent e in _tm.GetQueuedEvents())
			{
				if (e is FactoryProduction)
				{
					p = (FactoryProduction)e;
					break;
				}
			}

			var player = _f.GetComponent<DrillerCarrier>().GetOwner();
			var totalDrillers = _game.TimeMachine.GetState().GetPlayerDrillerCount(player);
			var drillerCap = _game.TimeMachine.GetState().GetPlayerDrillerCapacity(player);
			
			Assert.IsNotNull(p);
			Assert.IsTrue(totalDrillers < drillerCap);
			_tm.GoTo(p.GetOccursAt().Advance(10));
			Assert.IsTrue(_tm.GetCurrentTick().GetTick() > p.GetOccursAt().GetTick());
			Assert.IsTrue(p.WasEventSuccessful());
		}

		[TestMethod]
		public void AddDrillers()
		{
			int startingDrillers = _f.GetComponent<DrillerCarrier>().GetDrillerCount();
			_tm.Advance(Factory.STANDARD_TICKS_PER_PRODUCTION);
			int endingDrillers = _f.GetComponent<DrillerCarrier>().GetDrillerCount();
			Assert.AreEqual(endingDrillers - startingDrillers, Constants.BaseFactoryProduction);
		}

		[TestMethod]
		public void AddFactoryProductions()
		{
			_tm.Advance(Factory.STANDARD_TICKS_PER_PRODUCTION);
			Assert.IsTrue(_tm.GetQueuedEvents().Exists(isProductionEvent));
		}

		[TestMethod]
		public void FiveDrillerProductions()
		{
			int startingDrillers = _f.GetComponent<DrillerCarrier>().GetDrillerCount();
			int lastDrillerCount = 0;
			for (int i = 0; i < 5; i++)
			{
				_tm.Advance(Factory.STANDARD_TICKS_PER_PRODUCTION);
				var player = _f.GetComponent<DrillerCarrier>().GetOwner();
				var totalDrillers = _game.TimeMachine.GetState().GetPlayerDrillerCount(player);
				var drillerCap = _game.TimeMachine.GetState().GetPlayerDrillerCapacity(player);
				if (totalDrillers < drillerCap)
				{
					lastDrillerCount = _f.GetComponent<DrillerCarrier>().GetDrillerCount() - startingDrillers;
					Assert.AreEqual(Constants.BaseFactoryProduction * (i + 1),
						_f.GetComponent<DrillerCarrier>().GetDrillerCount() - startingDrillers);
				}
				else
				{
					Assert.AreEqual(lastDrillerCount,_f.GetComponent<DrillerCarrier>().GetDrillerCount() - startingDrillers);
				}
			}
		}

		[TestMethod]
		public void RewindProduction()
		{
			int startingDrillers = _f.GetComponent<DrillerCarrier>().GetDrillerCount();
			_tm.Advance(Factory.STANDARD_TICKS_PER_PRODUCTION);
			_tm.Rewind(Factory.STANDARD_TICKS_PER_PRODUCTION);
			Assert.AreEqual(_f.GetComponent<DrillerCarrier>().GetDrillerCount(), startingDrillers);
		}

		private bool isFactory(Outpost o)
		{
			return o.GetOutpostType() == OutpostType.Factory;
		}

		private bool isProductionEvent(GameEvent g) 
		{
			return g is FactoryProduction;
		}
	}
}