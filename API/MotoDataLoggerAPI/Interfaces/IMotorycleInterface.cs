using MotoDataLoggerAPI.Models;

namespace MotoDataLoggerAPI.Repository
{
    public interface IMotorcycleRepository
    {
        Task<List<Motorcycle>> GetMotorcyclesAsync();
        Task<Motorcycle?> GetMotorcycleAsync(int id);
        Task<Motorcycle> AddMotorcycleAsync(Motorcycle motorcycle);
        Task<Motorcycle?> UpdateMotorcycleAsync(int id, Motorcycle motorcycle);
        Task<bool> DeleteMotorcycleAsync(int id);
    }
}
