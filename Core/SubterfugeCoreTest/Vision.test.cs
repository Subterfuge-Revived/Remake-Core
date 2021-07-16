using System;
using System.Collections.Generic;
using System.Text;
using GameEventModels;
using Google.Protobuf;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents;
using SubterfugeCore.Core.GameEvents.NaturalGameEvents.vision;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;
using SubterfugeRemakeService;

namespace SubterfugeCoreTest
{
	[TestClass]
	public class Vision
	{
		private Game game;
		private Rft map;
		private Player player1, player2;

		[TestInitialize]
		public void Setup()
		{
			game = new Game();
			map = new Rft(600, 600);
			player1 = game.TimeMachine.GetState().GetPlayers()[0];
			player2 = new Player("002");
			game.TimeMachine.GetState().GetPlayers().Add(player2);
			game.TimeMachine.GetVisionManager().GetDictionary().Add(player2, new PriorityQueue<IVisionEvent>());
		}

		[TestMethod]
		public void RecalculateVisionEveryTick()
		{
			game.TimeMachine.Advance(100);
			// 101 because vision is also checked at start of game (tick = 0)
			Assert.AreEqual(101, game.TimeMachine.GetVisionManager().GetRecalculateVisionEvents().Count);
		}

		[TestMethod]
		public void NoVisionWhenOutOfRange()
		{
			// Distance between two outposts is 150; standard outpost vision range is 144
			Outpost outpost1 = new Generator(Guid.NewGuid().ToString(), new RftVector(map, 0, 0), player1);
			Outpost outpost2 = new Generator(Guid.NewGuid().ToString(), new RftVector(map, 150, 0), player2);
			game.TimeMachine.GetState().GetOutposts().Add(outpost1);
			game.TimeMachine.GetState().GetOutposts().Add(outpost2);

			Assert.AreEqual(150, outpost1.GetCurrentPosition().Distance(outpost2.GetCurrentPosition()));

			game.TimeMachine.NextTick();

			// Should be 0 vision events as the two outposts cannot see each other
			Assert.AreEqual(0, game.TimeMachine.GetVisionManager().GetRecalculateVisionEvent(1).GetVisionEvents().Count);
			Assert.AreEqual(0, game.TimeMachine.GetVisionManager().GetPlayerVisionEvents(player1).Count);
			Assert.AreEqual(0, game.TimeMachine.GetVisionManager().GetPlayerVisionEvents(player2).Count);
		}

		[TestMethod]
		public void VisionWhenInRange()
		{
			// Distance between two outposts is 140; standard outpost vision range is 144
			Outpost outpost1 = new Generator(Guid.NewGuid().ToString(), new RftVector(map, 0, 0), player1);
			Outpost outpost2 = new Generator(Guid.NewGuid().ToString(), new RftVector(map, 140, 0), player2);
			game.TimeMachine.GetState().GetOutposts().Add(outpost1);
			game.TimeMachine.GetState().GetOutposts().Add(outpost2);

			Assert.AreEqual(140, outpost1.GetCurrentPosition().Distance(outpost2.GetCurrentPosition()));

			game.TimeMachine.NextTick();

			// Should be two vision events as the two outposts can see each other
			Assert.AreEqual(2, game.TimeMachine.GetVisionManager().GetRecalculateVisionEvent(1).GetVisionEvents().Count);
			Assert.AreEqual(1, game.TimeMachine.GetVisionManager().GetPlayerVisionEvents(player1).Count);
			Assert.AreEqual(1, game.TimeMachine.GetVisionManager().GetPlayerVisionEvents(player2).Count);
		}

		[TestMethod]
		public void NoVisionEventBetweenAllyEntities()
		{
			// Distance between two outposts is 140; standard outpost vision range is 144
			Outpost outpost1 = new Generator(Guid.NewGuid().ToString(), new RftVector(map, 0, 0), player1);
			Outpost outpost2 = new Generator(Guid.NewGuid().ToString(), new RftVector(map, 140, 0), player1);
			game.TimeMachine.GetState().GetOutposts().Add(outpost1);
			game.TimeMachine.GetState().GetOutposts().Add(outpost2);game.TimeMachine.NextTick();

			// Should be zero vision events as the two outposts are both owned by player1
			Assert.AreEqual(0, game.TimeMachine.GetVisionManager().GetRecalculateVisionEvent(1).GetVisionEvents().Count);
			Assert.AreEqual(0, game.TimeMachine.GetVisionManager().GetPlayerVisionEvents(player1).Count);
		}

		[TestMethod]
		public void WatchTowerVision()
		{
			// Distance between two outposts is 200; standard outpost vision range is 144, watchtower is 144 * 1.5 = 216
			Outpost outpost1 = new Generator(Guid.NewGuid().ToString(), new RftVector(map, 0, 0), player1);
			Outpost outpost2 = new Watchtower(Guid.NewGuid().ToString(), new RftVector(map, 200, 0), player2);
			game.TimeMachine.GetState().GetOutposts().Add(outpost1);
			game.TimeMachine.GetState().GetOutposts().Add(outpost2); game.TimeMachine.NextTick();

			// Should be one vision event as only the watchtower of player 2 can see the outpost of player 1
			Assert.AreEqual(1, game.TimeMachine.GetVisionManager().GetRecalculateVisionEvent(1).GetVisionEvents().Count);
			Assert.AreEqual(player2, game.TimeMachine.GetVisionManager().GetRecalculateVisionEvent(1).GetVisionEvents()[0].GetPlayer());
			Assert.AreEqual(0, game.TimeMachine.GetVisionManager().GetPlayerVisionEvents(player1).Count);
			Assert.AreEqual(1, game.TimeMachine.GetVisionManager().GetPlayerVisionEvents(player2).Count);
		}

		[TestMethod]
		public void SubEntersOutpostVision()
		{
			Outpost outpost1 = new Generator(Guid.NewGuid().ToString(), new RftVector(map, 0, 0), player1);
			Outpost outpost2 = new Generator(Guid.NewGuid().ToString(), new RftVector(map, 288, 0), player2);
			game.TimeMachine.GetState().GetOutposts().Add(outpost1);
			game.TimeMachine.GetState().GetOutposts().Add(outpost2);
			outpost1.AddDrillers(10);
			LaunchEvent launch = new LaunchEvent(new GameEventModel()
			{
				EventData = new LaunchEventData()
				{
					DestinationId = outpost2.GetId(),
					DrillerCount = 5,
					SourceId = outpost1.GetId(),
				}.ToByteString(),
				EventId = "1680",
				EventType = EventType.LaunchEvent,
				OccursAtTick = 1,
			});
			game.TimeMachine.AddEvent(launch);
			game.TimeMachine.Advance(97); 
			// Launches tick 1, should take 96 ticks to enter vision
			Assert.AreEqual(1, game.TimeMachine.GetVisionManager().GetRecalculateVisionEvent(97).GetVisionEvents().Count);
			Assert.AreEqual(0, game.TimeMachine.GetVisionManager().GetPlayerVisionEvents(player1).Count);
			Assert.AreEqual(1, game.TimeMachine.GetVisionManager().GetPlayerVisionEvents(player2).Count);
		}

		[TestMethod]
		public void SubExitsOutpostVision()
		{
			Outpost outpost1 = new Generator(Guid.NewGuid().ToString(), new RftVector(map, 0, 0), player1);
			Outpost outpost2 = new Generator(Guid.NewGuid().ToString(), new RftVector(map, 72, 0), player2);
			Outpost outpost3 = new Generator(Guid.NewGuid().ToString(), new RftVector(map, 200, 0), player2);
			game.TimeMachine.GetState().GetOutposts().Add(outpost1);
			game.TimeMachine.GetState().GetOutposts().Add(outpost2);
			game.TimeMachine.GetState().GetOutposts().Add(outpost3);
			outpost2.AddDrillers(10);
			LaunchEvent launch = new LaunchEvent(new GameEventModel()
			{
				EventData = new LaunchEventData()
				{
					DestinationId = outpost3.GetId(),
					DrillerCount = 5,
					SourceId = outpost2.GetId(),
				}.ToByteString(),
				EventId = "1680",
				EventType = EventType.LaunchEvent,
				OccursAtTick = 1,
			});
			game.TimeMachine.AddEvent(launch);
			game.TimeMachine.Advance(50);
			// Launches tick 1, should take 48 + 1 ticks to exit vision
			Assert.AreEqual(1, game.TimeMachine.GetVisionManager().GetRecalculateVisionEvent(50).GetVisionEvents().Count);
			// player1 has three vision events: sees outpost2, sub "enters" vision when created, sub exits
			Assert.AreEqual(3, game.TimeMachine.GetVisionManager().GetPlayerVisionEvents(player1).Count);
			// player2 has one vision event: sees outpost1
			Assert.AreEqual(1, game.TimeMachine.GetVisionManager().GetPlayerVisionEvents(player2).Count);
		}

		[TestMethod]
		public void SubVision()
		{
			Outpost outpost1 = new Generator(Guid.NewGuid().ToString(), new RftVector(map, 0, 0), player1);
			Outpost outpost2 = new Generator(Guid.NewGuid().ToString(), new RftVector(map, 288, 0), player2);
			game.TimeMachine.GetState().GetOutposts().Add(outpost1);
			game.TimeMachine.GetState().GetOutposts().Add(outpost2);
			outpost1.AddDrillers(10);
			LaunchEvent launch = new LaunchEvent(new GameEventModel()
			{
				EventData = new LaunchEventData()
				{
					DestinationId = outpost2.GetId(),
					DrillerCount = 5,
					SourceId = outpost1.GetId(),
				}.ToByteString(),
				EventId = "1680",
				EventType = EventType.LaunchEvent,
				OccursAtTick = 1,
			});
			game.TimeMachine.AddEvent(launch);
			game.TimeMachine.Advance(174);
			// Launches tick 1, distance is 2 * standard outpost radius so takes ceiling of 96 * 1.8 ticks for outpost to be in sub vision = tick 174
			Assert.AreEqual(1, game.TimeMachine.GetVisionManager().GetRecalculateVisionEvent(174).GetVisionEvents().Count);
			Assert.AreEqual(1, game.TimeMachine.GetVisionManager().GetPlayerVisionEvents(player1).Count);
			Assert.AreEqual(1, game.TimeMachine.GetVisionManager().GetPlayerVisionEvents(player2).Count);
		}

		[TestMethod]
		public void NewOwnerCreatesVisionEvent()
		{
			Outpost outpost1 = new Generator(Guid.NewGuid().ToString(), new RftVector(map, 0, 0), player1);
			// Outposts 1 and 3 have vision of each other
			Outpost outpost2 = new Generator(Guid.NewGuid().ToString(), new RftVector(map, 288, 0), player2);
			Outpost outpost3 = new Generator(Guid.NewGuid().ToString(), new RftVector(map, 0, 100), player1);
			game.TimeMachine.GetState().GetOutposts().Add(outpost1);
			game.TimeMachine.GetState().GetOutposts().Add(outpost2);
			game.TimeMachine.GetState().GetOutposts().Add(outpost3);

			game.TimeMachine.NextTick();

			outpost1.SetOwner(player2);
			game.TimeMachine.NextTick();

			// Outposts 1 & 3 are now visible to player 2
			Assert.AreEqual(2, game.TimeMachine.GetVisionManager().GetRecalculateVisionEvent(2).GetVisionEvents().Count);

			outpost3.SetOwner(player2);

			game.TimeMachine.NextTick();

			// Outposts 1 & 3 are no longer visible to player 1
			Assert.AreEqual(2, game.TimeMachine.GetVisionManager().GetRecalculateVisionEvent(3).GetVisionEvents().Count);

			Assert.AreEqual(2, game.TimeMachine.GetVisionManager().GetPlayerVisionEvents(player1).Count);
			Assert.AreEqual(2, game.TimeMachine.GetVisionManager().GetPlayerVisionEvents(player2).Count);
		}
	}
}
