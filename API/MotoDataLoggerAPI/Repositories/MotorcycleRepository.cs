using Microsoft.EntityFrameworkCore;
using MotoDataLoggerAPI.Models;
using MotoDataLoggerAPI.Data;

namespace MotoDataLoggerAPI.Repository
{
    public class MotorcycleRepository : IMotorcycleRepository
    {
        private readonly MotoDataContext _context;

        public MotorcycleRepository(MotoDataContext context)
        {
            _context = context;
        }

        public async Task<List<Motorcycle>> GetMotorcyclesAsync()
        {
            return await _context.Motorcycles.ToListAsync();
        }

        public async Task<Motorcycle?> GetMotorcycleAsync(int id)
        {
            return await _context.Motorcycles.FindAsync(id);
        }

        public async Task<Motorcycle> AddMotorcycleAsync(Motorcycle motorcycle)
        {
            _context.Motorcycles.Add(motorcycle);
            await _context.SaveChangesAsync();
            return motorcycle;
        }

        public async Task<Motorcycle?> UpdateMotorcycleAsync(int id, Motorcycle motorcycle)
        {
            if (id != motorcycle.Id)
            {
                return null;
            }
           var existingMotorcycle = await _context.Motorcycles.FindAsync(id);

            if (existingMotorcycle == null)
            {
                return null;
            }
             _context.Entry(existingMotorcycle).CurrentValues.SetValues(motorcycle);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MotorcycleExists(id))
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }

            return existingMotorcycle;
        }

        public async Task<bool> DeleteMotorcycleAsync(int id)
        {
            var motorcycle = await _context.Motorcycles.FindAsync(id);
            if (motorcycle == null)
            {
                return false;
            }

            _context.Motorcycles.Remove(motorcycle);
            await _context.SaveChangesAsync();
            return true;
        }

        private bool MotorcycleExists(int id)
        {
            return _context.Motorcycles.Any(e => e.Id == id);
        }
    }
}
