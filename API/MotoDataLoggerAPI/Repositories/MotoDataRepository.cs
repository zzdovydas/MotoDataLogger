using Microsoft.EntityFrameworkCore;
using MotoDataLoggerAPI.Models;
using MotoDataLoggerAPI.Data;

namespace MotoDataLoggerAPI.Repository
{
    public class MotoDataRepository : IMotoDataRepository
    {
        private readonly MotoDataContext _context;

        public MotoDataRepository(MotoDataContext context)
        {
            _context = context;
        }

        public async Task<List<MotoData>> GetMotoDataAsync()
        {
            return await _context.MotoDatas.ToListAsync();
        }

        public async Task<MotoData> AddMotoDataByAPIKeyAsync(MotoData motoData)
        {
            if (_context.ApiKeys.Any(a => a.Key == motoData.ApiKey.Key))
            {
                _context.MotoDatas.Add(motoData);
                await _context.SaveChangesAsync();
                return motoData;
            }

            return null;
        }

        public async Task<MotoData> AddMotoDataAsync(MotoData motoData)
        {
            _context.MotoDatas.Add(motoData);
            await _context.SaveChangesAsync();
            return motoData;
        }
    }
}
