using MotoDataLoggerAPI.Models;

namespace MotoDataLoggerAPI.Repository
{
    public interface IMotoDataRepository
    {
        Task<List<MotoData>> GetMotoDataAsync();
        Task<MotoData> AddMotoDataAsync(MotoData motoData);
    }
}
