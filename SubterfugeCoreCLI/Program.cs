using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Network;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;
using SubterfugeCoreCLI.Response;

namespace SubterfugeCoreCLI
{
    class Program
    {
        public async static Task Main(string[] args)
        {
            if (args.Length > 0)
            {
                var result = Parser.Default.ParseArguments<ValidateEventCommand>(args);
                await result.MapResult(async response =>
                {
                    await HandleParsed(response);
                },
                    errors => Task.FromResult(0));
            }
            else
            {
                // Use this section for debugging purposes.
                // Feel free to remove what is here.
                
                Api api = new Api("http://18.220.154.6");

                NetworkResponse<LoginResponse> loginResponse = await api.Login("asdfg", "asdfg");

                if (loginResponse.IsSuccessStatusCode())
                {
                    Console.WriteLine("Logged in!");


                    Game game = new Game();

                    List<Player> players = new List<Player>()
                    {
                        new Player(1),
                        new Player(2),
                    };
                    NetworkResponse<List<BlockedPlayer>> response = await api.GetBlockList();

                    if (response.IsSuccessStatusCode())
                    {
                        Console.WriteLine("Got blocked palyer list");
                    }
                    else
                    {
                        Console.WriteLine(response.ErrorContent.Message);
                    }
                }
            }
        }

        public async static Task HandleParsed(ValidateEventCommand parsed)
        {
            Api api = new Api();
            api.SetToken(parsed.Token);

            NetworkResponse<List<NetworkGameEvent>> gameEvents = await api.GetGameEvents(Int32.Parse(parsed.GameId));
            Console.WriteLine("Done");
        }
    }
}