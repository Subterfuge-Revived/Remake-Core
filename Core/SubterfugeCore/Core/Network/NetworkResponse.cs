using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SubterfugeCore.Core.Network
{
    /// <summary>
    /// Base Network response. All Network responses should have a 'success' variable to determine if the operation
    /// succeeded and a message describing any issues if it didn't
    /// </summary>
    public class NetworkResponse<T> where T : BaseNetworkResponse
    {
        /// <summary>
        /// The HttpResponse object that comes directly from the HttpClient
        /// </summary>
        public HttpResponseMessage HttpResponse { get; set; }
        
        /// <summary>
        /// If the request resulted in an error, this variable gets populated with the error content.
        /// </summary>
        public NetworkError ErrorContent { get; set; }
        
        /// <summary>
        /// The raw string of the HttpResponse. This may be an error string or a valid JSON string.
        /// </summary>
        public string ResponseContent { get; set; }
        
        /// <summary>
        /// A templated object that holds the parsed JSON response if the request was successful.
        /// </summary>
        public T Response { get; set; }
        
        /// <summary>
        /// Static method to create a new Network Response.
        /// </summary>
        /// <param name="responseMessage">The HttpResponseMessage that is recieved from the HttpClient</param>
        /// <returns>A new instance of a NetworkResponse</returns>
        public static async Task<NetworkResponse<T>> FromHttpResponse(HttpResponseMessage responseMessage)
        {
            NetworkResponse<T> response = new NetworkResponse<T>();
            response.HttpResponse = responseMessage;
            
            // Read the response
            string responseContent = await responseMessage.Content.ReadAsStringAsync();
            response.ResponseContent = responseContent;

            // Deserialize the response if it was successful.
            if (responseMessage.IsSuccessStatusCode)
            {
                T responseTemplate = default(T);
                if (responseContent.StartsWith("["))
                {
                    // Response is an array, convert it to an object with a key.
                    responseContent = $"{{ 'array': {responseContent} }}";
                }
                
                // At this point, the JSON deserialization can throw an error.
                // However, if there is a successful network response but it cannot parse the JSON
                // then this is a fault of the developer.
                // Thus, letting the error be thrown here is fine.
                responseTemplate = JsonConvert.DeserializeObject<T>(responseContent);

                response.Response = responseTemplate;
            }
            else
            {
                NetworkError error;
                try
                {
                    Console.WriteLine("Request returned error: " + responseContent);
                    error = JsonConvert.DeserializeObject<NetworkError>(responseContent);
                }
                catch (JsonException e)
                {
                    error = new NetworkError();
                    error.Message = responseContent;
                    Console.WriteLine(e);
                }

                response.ErrorContent = error;
            }

            return response;
        }

        /// <summary>
        /// Determines if the network request was successful.
        /// </summary>
        /// <returns>True if the request was successful, false if not.</returns>
        public bool IsSuccessStatusCode()
        {
            return HttpResponse.IsSuccessStatusCode;
        }
    }
}