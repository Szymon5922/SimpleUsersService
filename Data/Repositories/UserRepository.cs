using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<(List<User> users, int totalCount)> GetPaginatedAsync(int page, int limit);
        Task<User> GetByIDAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
    }
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Addresses)
                .ToListAsync();
        }

        public async Task<(List<User> users, int totalCount)> GetPaginatedAsync(int page, int limit)
        {
            var query = _context.Users.AsQueryable();

            int totalCount = await query.CountAsync();

            var users = await query
                .Skip((page-1)*limit)
                .Take(limit)
                .ToListAsync();

            return (users, totalCount);
        }

        public async Task<User> GetByIDAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Addresses)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            foreach (var address in user.Addresses)
                _context.Addresses.Update(address);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(User user)
        {
            _context.Addresses.RemoveRange(user.Addresses);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

    }
}

