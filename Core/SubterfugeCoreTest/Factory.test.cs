using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
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

		private readonly List<Player> players = new List<Player>()
		{
			new Player(new SimpleUser() { Id = "1" })
		};

		[TestInitialize]
		public void Setup()
		{
			GameConfiguration config = testUtils.GetDefaultGameConfiguration(players);
			config.MapConfiguration.OutpostsPerPlayer = 10;
			_game = new Game(config);
			_tm = _game.TimeMachine;
			foreach (Outpost o in _tm.GetState().GetPlayerOutposts(players[0]))
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
		public void ProductionEventSuccess()
		{
			var player = _f.GetComponent<DrillerCarrier>().GetOwner();
			var totalDrillers = _game.TimeMachine.GetState().GetPlayerDrillerCount(player);
			var drillerCap = _game.TimeMachine.GetState().GetPlayerDrillerCapacity(player);
			var playerFactoryCount = _game.TimeMachine.GetState()
				.GetPlayerOutposts(player)
				.Count(it => it.GetOutpostType() == OutpostType.Factory);

			var drillerProducer = _f.GetComponent<DrillerProducer>().Producer;
			ProductionEventArgs eventArgs = null;
			drillerProducer.OnResourceProduced += (sender, productionEvent) =>
			{
				eventArgs = productionEvent;
			};
			
			Assert.IsNotNull(drillerProducer);
			Assert.IsTrue(totalDrillers < drillerCap);
			_tm.GoTo(drillerProducer.GetNextProductionTick());
			
			Assert.AreEqual(totalDrillers + (eventArgs.ProductionEvent.ValueProduced * playerFactoryCount), _game.TimeMachine.GetState().GetPlayerDrillerCount(player));
		}

		[TestMethod]
		public void AddDrillers()
		{
			int startingDrillers = _f.GetComponent<DrillerCarrier>().GetDrillerCount();
			
			var drillerProducer = _f.GetComponent<DrillerProducer>();
			ProductionEventArgs eventArgs = null;
			drillerProducer.Producer.OnResourceProduced += (sender, productionEvent) =>
			{
				eventArgs = productionEvent;
			};
			
			_tm.GoTo(drillerProducer.Producer.GetNextProductionTick());
			int endingDrillers = _f.GetComponent<DrillerCarrier>().GetDrillerCount();
			Assert.AreEqual(endingDrillers - startingDrillers, eventArgs.ProductionEvent.ValueProduced);
		}

		[TestMethod]
		public void FiveDrillerProductions()
		{
			int startingDrillers = _f.GetComponent<DrillerCarrier>().GetDrillerCount();
			var drillerProducer = _f.GetComponent<DrillerProducer>();

			int lastDrillerCount = 0;
			for (int i = 0; i < 5; i++)
			{
				_tm.GoTo(drillerProducer.Producer.GetNextProductionTick());
				var player = _f.GetComponent<DrillerCarrier>().GetOwner();
				var totalDrillers = _game.TimeMachine.GetState().GetPlayerDrillerCount(player);
				var drillerCap = _game.TimeMachine.GetState().GetPlayerDrillerCapacity(player);
				if (totalDrillers < drillerCap)
				{
					lastDrillerCount = _f.GetComponent<DrillerCarrier>().GetDrillerCount() - startingDrillers;
					Assert.AreEqual(Constants.BaseFactoryProductionAmount * (i + 1),
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
			var drillerProducer = _f.GetComponent<DrillerProducer>().Producer;
			_tm.Advance(drillerProducer.GetTicksPerProductionCycle());
			_tm.GoTo(new GameTick(0));
			Assert.AreEqual(_f.GetComponent<DrillerCarrier>().GetDrillerCount(), startingDrillers);
		}

		private bool isFactory(Outpost o)
		{
			return o.GetOutpostType() == OutpostType.Factory;
		}
	}
}