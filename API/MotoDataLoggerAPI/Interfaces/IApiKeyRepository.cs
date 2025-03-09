using MotoDataLoggerAPI.Models;

namespace MotoDataLoggerAPI.Repository
{
    public interface IApiKeyRepository
    {
        Task<List<ApiKey>> GetApiKeysAsync();
        Task<ApiKey?> GetApiKeyAsync(int id);
        Task<ApiKey?> GetApiKeyByKeyAsync(string key);
        Task<ApiKey> AddApiKeyAsync(ApiKey apiKey);
        Task<ApiKey?> UpdateApiKeyAsync(int id, ApiKey apiKey);
        Task<bool> DeleteApiKeyAsync(int id);
    }
}
