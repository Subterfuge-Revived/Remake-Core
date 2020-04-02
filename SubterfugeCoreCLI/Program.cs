using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json;
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
                    await handleParsed(response);
                },
                    errors => Task.FromResult(0));
            }
        }

        public async static Task handleParsed(ValidateEventCommand parsed)
        {
            string response = await getEvents(parsed.token, parsed.gameId);
            if (response != null)
            {
                try
                {
                    List<NetworkGameEvent> gameEvents = JsonConvert.DeserializeObject<List<NetworkGameEvent>>(response);
                    Console.WriteLine(new ValidateEventResponse(response, false));
                }
                catch (Exception e)
                {
                    NetworkResponse networkResponse = JsonConvert.DeserializeObject<NetworkResponse>(response);
                    Console.WriteLine(new FailureResponse(networkResponse.message));
                }
            }
        }
        
        public static async Task<string> getEvents(string token, string gameId){
            HttpClient httpClient = new HttpClient();
            try
            {
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("session_id", token),
                    new KeyValuePair<string, string>("type", "get_events"),
                    new KeyValuePair<string, string>("room_id", gameId),
                });

                HttpResponseMessage response = await httpClient.PostAsync("http://localhost/subterfuge-backend/sandbox/event_exec.php", formContent);
                // Read the response
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(new FailureResponse(e.Message).ToString());
                return null;
            }
        }
    }
}