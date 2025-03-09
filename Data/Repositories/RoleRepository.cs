using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data.Repositories
{
    public interface IRoleRepository
    {
        public Task<Role> GetDefaultRoleAsync();
    }

    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Role> GetDefaultRoleAsync()
        {
            return await _context.Roles.FirstAsync(r=>r.Name==Enums.RoleType.User);
        }
    }
}
