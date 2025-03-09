using Microsoft.EntityFrameworkCore;
using MotoDataLoggerAPI.Models;
using MotoDataLoggerAPI.Data;

namespace MotoDataLoggerAPI.Repository
{
    public class AuthRepository : IUserRepository
    {
        private readonly MotoDataContext _context;

        public AuthRepository(MotoDataContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }
    }
}
