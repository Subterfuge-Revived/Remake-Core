using System.Transactions;
using Newtonsoft.Json;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeRestApiClient.controllers.exception;

public class SubterfugeClientException : HttpRequestException
{
    public NetworkResponse? response;
    public HttpResponseMessage rawResponse;
    private SubterfugeClientException(string exceptionString, NetworkResponse? response, HttpResponseMessage rawResponse) : base(exceptionString)
    {
        this.response = response;
        this.rawResponse = rawResponse;
    }

    public static async Task<SubterfugeClientException> CreateFromResponseMessage(HttpResponseMessage message)
    {
        var statusCode = message.StatusCode;
        var responseString = await message.Content.ReadAsStringAsync();
        NetworkResponse? responseStatus = JsonConvert.DeserializeObject<NetworkResponse>(responseString);

        if (responseStatus != null)
        {
            var exceptionString =
                $"The Subterfuge Server responded with an error. {statusCode} {responseStatus?.Status.ResponseType}: {responseStatus?.Status.Detail}";
            return new SubterfugeClientException(exceptionString, responseStatus, message);
        }
        else
        {
            var exceptionString =
                $"The Subterfuge Server responded with an error. {statusCode}";
            return new SubterfugeClientException(exceptionString, responseStatus, message);
        }
    }
}