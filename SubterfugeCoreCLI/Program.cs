using System;
using System.Threading.Tasks;
using CommandLine;
using SubterfugeCore.Core.Network;

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
                    NetworkResponse<GameRoomResponse> rooms = await api.GetOpenRooms();
                    if (rooms.IsSuccessStatusCode())
                    {
                        
                    }
                }
            }
        }

        public async static Task HandleParsed(ValidateEventCommand parsed)
        {
            Api api = new Api();
            // api.SetToken(parsed.Token);

            NetworkResponse<GameEventResponse> gameEvents = await api.GetGameEvents(Int32.Parse(parsed.GameId));
            Console.WriteLine("Done");
        }
    }
}