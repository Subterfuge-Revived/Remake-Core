using System.Threading.Tasks;

namespace SubterfugeCore.Models.GameEvents.Api
{
    public interface SubterfugeAccountApi
    {
        Task<AuthorizationResponse> Login(AuthorizationRequest request);
    }
}