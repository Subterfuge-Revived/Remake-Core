using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents.NaturalGameEvents.outpost;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;
using SubterfugeRemakeService;

/*
namespace SubterfugeCoreTest
{
	[TestClass]
	public class FactoryTest
	{
		Game game;
		TimeMachine tm;
		Factory f = null;
		TestUtils testUtils = new TestUtils();

		[TestInitialize]
		public void Setup()
		{
			List<Player> playerlist = new List<Player>();
			Player player = new Player("Player 1");
			playerlist.Add(player);
			GameConfiguration config = testUtils.GetDefaultGameConfiguration(playerlist);
			config.MapConfiguration.OutpostsPerPlayer = 10;
			game = new Game(config);
			tm = game.TimeMachine;
			foreach (Outpost o in tm.GetState().GetPlayerOutposts(player))
			{
				if (isFactory(o))
				{
					f = (Factory)o;
					break;
				}
			}
			Assert.IsNotNull(f);
		}

		[TestMethod]
		public void InitialProductionEvent()
		{
			Assert.IsTrue(tm.GetQueuedEvents().Exists(isProductionEvent));
		}

		[TestMethod]
		public void ProductionEventSuccess()
		{
			FactoryProduction p = null;
			foreach (GameEvent e in tm.GetQueuedEvents())
			{
				if (e is FactoryProduction)
				{
					p = (FactoryProduction)e;
					break;
				}
			}

			var player = f.GetComponent<DrillerCarrier>().GetOwner();
			var totalDrillers = game.TimeMachine.GetState().getPlayerDrillerCount(player);
			var drillerCap = game.TimeMachine.GetState().getPlayerDrillerCapacity(player);
			
			Assert.IsNotNull(p);
			Assert.IsTrue(totalDrillers < drillerCap);
			tm.GoTo(p.GetOccursAt().Advance(10));
			Assert.IsTrue(tm.GetCurrentTick().GetTick() > p.GetOccursAt().GetTick());
			Assert.IsTrue(p.WasEventSuccessful());
		}

		[TestMethod]
		public void AddDrillers()
		{
			int startingDrillers = f.GetComponent<DrillerCarrier>().GetDrillerCount();
			tm.Advance(Factory.STANDARD_TICKS_PER_PRODUCTION);
			int endingDrillers = f.GetComponent<DrillerCarrier>().GetDrillerCount();
			Assert.AreEqual(endingDrillers - startingDrillers, Constants.BASE_FACTORY_PRODUCTION);
		}

		[TestMethod]
		public void AddFactoryProductions()
		{
			tm.Advance(Factory.STANDARD_TICKS_PER_PRODUCTION);
			Assert.IsTrue(tm.GetQueuedEvents().Exists(isProductionEvent));
		}

		[TestMethod]
		public void TenDrillerProductions1()
		{
			int startingDrillers = f.GetComponent<DrillerCarrier>().GetDrillerCount();
			for (int i = 0; i < 10; i++)
			{
				tm.Advance(Factory.STANDARD_TICKS_PER_PRODUCTION);
				Assert.AreEqual(f.GetComponent<DrillerCarrier>().GetDrillerCount() - startingDrillers, Constants.BASE_FACTORY_PRODUCTION * (i + 1));
			}
		}

		[TestMethod]
		public void TenDrillerProductions2()
		{
			int startingDrillers = f.GetComponent<DrillerCarrier>().GetDrillerCount();
			tm.Advance(Factory.STANDARD_TICKS_PER_PRODUCTION * 10);
			Assert.AreEqual(f.GetComponent<DrillerCarrier>().GetDrillerCount() - startingDrillers, Constants.BASE_FACTORY_PRODUCTION * 10);
		}

		[TestMethod]
		public void RewindProduction()
		{
			int startingDrillers = f.GetComponent<DrillerCarrier>().GetDrillerCount();
			tm.Advance(Factory.STANDARD_TICKS_PER_PRODUCTION);
			tm.Rewind(Factory.STANDARD_TICKS_PER_PRODUCTION);
			Assert.AreEqual(f.GetComponent<DrillerCarrier>().GetDrillerCount(), startingDrillers);
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
*/