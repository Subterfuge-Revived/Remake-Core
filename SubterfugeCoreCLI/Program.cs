using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SubterfugeCoreCLI
{
    class Program
    {
        public static void Main(string[] args)
        {
            getEvents(args).GetAwaiter().GetResult();
        }
        
        
        public static async Task getEvents(string[] args){
            HttpClient httpClient = new HttpClient();
            try
            {
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("session_id", args[0]),
                    new KeyValuePair<string, string>("type", "get_events"),
                    new KeyValuePair<string, string>("room_id", args[1]),
                });

                HttpResponseMessage response = await httpClient.PostAsync("http://localhost/subterfuge-backend/sandbox/event_exec.php", formContent);
                // Read the response
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}