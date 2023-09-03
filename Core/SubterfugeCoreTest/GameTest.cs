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
using Subterfuge.Remake.Core.Topologies;

namespace Subterfuge.Remake.Test
{
	
	[TestClass]
	public class GameTest
	{
		[TestMethod]
		public void BareGameDoesNotGenerateMap()
		{
			Game game = Game.Bare();
			Assert.AreEqual(game.TimeMachine.GetState().GetOutposts().Count, 0);
		}
		
		[TestMethod]
		public void CanAddOutpostsToBareGame()
		{
			Game game = Game.Bare();
			Assert.AreEqual(game.TimeMachine.GetState().GetOutposts().Count, 0);
			game.TimeMachine.GetState().GetOutposts().Add(new Factory("1", new RftVector(0, 0), game.TimeMachine));
			Assert.AreEqual(game.TimeMachine.GetState().GetOutposts().Count, 1);
		}

		[TestMethod]
		public void GameCanLoadFromConfiguration()
		{
			GameConfiguration config = new GameConfiguration();
			Game game = Game.FromGameConfiguration(config);
			Assert.AreEqual(game.TimeMachine.GetState().GetPlayers().Count, config.PlayersInLobby.Count);
		}

		[TestMethod]
		public void GameCreatedFromConfigGeneratesMap()
		{
			GameConfiguration config = new GameConfiguration();
			Game game = Game.FromGameConfiguration(config);
			Assert.AreEqual(game.TimeMachine.GetState().GetPlayers().Count, config.PlayersInLobby.Count);
			Assert.IsTrue(game.TimeMachine.GetState().GetOutposts().Count > 0);
		}
	}
}
