using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SubterfugeCore.Core.Network
{
    /// <summary>
    /// Base Network response. All Network responses should have a 'success' variable to determine if the operation
    /// succeeded and a message describing any issues if it didn't
    /// </summary>
    public class NetworkResponse<T>
    {
        public HttpResponseMessage Response { get; set; }
        
        public NetworkError ErrorContent { get; set; }
        
        public string ResponseContent { get; set; }
        public T ResponseObject { get; set; }
        
        public static async Task<NetworkResponse<T>> FromHttpResponse(HttpResponseMessage responseMessage)
        {
            NetworkResponse<T> response = new NetworkResponse<T>();
            response.Response = responseMessage;
            
            // Read the response
            string responseContent = await responseMessage.Content.ReadAsStringAsync();
            response.ResponseContent = responseContent;

            // Deserialize the response if it was successful.
            if (responseMessage.IsSuccessStatusCode)
            {
                T responseTemplate = default(T);
                try
                {
                    responseTemplate = JsonConvert.DeserializeObject<T>(responseContent);
                }
                catch (JsonException e)
                {
                }

                response.ResponseObject = responseTemplate;
            }
            else
            {
                NetworkError error;
                try
                {
                    error = JsonConvert.DeserializeObject<NetworkError>(responseContent);
                }
                catch (JsonException e)
                {
                    error = null;
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
            return Response.IsSuccessStatusCode;
        }
    }
}