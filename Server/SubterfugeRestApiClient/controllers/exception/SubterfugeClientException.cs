using System.Transactions;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeRestApiClient.controllers.exception;

public class SubterfugeClientException : HttpRequestException
{
    public ResponseStatus? response;
    public HttpResponseMessage rawResponse;
    private SubterfugeClientException(string exceptionString, ResponseStatus? response, HttpResponseMessage rawResponse) : base(exceptionString)
    {
        this.response = response;
        this.rawResponse = rawResponse;
    }

    public static async Task<SubterfugeClientException> CreateFromResponseMessage(HttpResponseMessage message)
    {
        var statusCode = message.StatusCode;
        ResponseStatus? responseStatus = await message.Content.ReadAsAsync<ResponseStatus>();

        if (responseStatus != null)
        {
            var exceptionString =
                $"The Subterfuge Server responded with an error. {statusCode} {responseStatus?.ResponseType}: {responseStatus?.Detail}";
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