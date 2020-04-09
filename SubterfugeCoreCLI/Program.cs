using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Network;
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
        }

        public async static Task HandleParsed(ValidateEventCommand parsed)
        {
            Api api = new Api();
            api.SetToken(parsed.Token);

            List<GameEvent> gameEvents = await api.GetGameEvents(Int32.Parse(parsed.GameId));
            Console.WriteLine("Done");
        }
    }
}