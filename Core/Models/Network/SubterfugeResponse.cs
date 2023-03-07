using System;

namespace Subterfuge.Remake.Api.Network
{

    public class SubterfugeResponse<T>
    {
        public ResponseStatus ResponseDetail;
        public T data;
        
        public void Get(Action<T> success, Action<ResponseStatus> failure)
        {
            if (ResponseDetail.IsSuccess)
            {
                success(data);
            }
            else
            {
                failure(ResponseDetail);
            }
        }
        
        public TResponse Get<TResponse>(Func<T, TResponse> success, Func<ResponseStatus, TResponse> failure)
        {
            if (ResponseDetail.IsSuccess)
            {
                return success(data);
            }
            else
            {
                return failure(ResponseDetail);
            }
        }

        public bool IsSuccess()
        {
            return ResponseDetail.IsSuccess;
        }

        public T GetOrThrow()
        {
            if (ResponseDetail.IsSuccess)
            {
                return data;
            }

            throw new NullReferenceException($"Attempted to collect data from HTTP response, but the request was an error: {ResponseDetail.Detail}");
        }
        
        public static SubterfugeResponse<T> OfFailure(ResponseType type, string detail)
        {
            return new SubterfugeResponse<T>()
            {
                ResponseDetail = new ResponseStatus()
                {
                    IsSuccess = false,
                    Detail = detail,
                    ResponseType = type,
                },
            };
        }

        public static SubterfugeResponse<T> OfSuccess(T data)
        {
            return new SubterfugeResponse<T>()
            {
                ResponseDetail = new ResponseStatus() { IsSuccess = true, ResponseType = ResponseType.SUCCESS},
                data = data,
            };
        }
    }
}