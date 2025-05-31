using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace BookSwap.Services
{
    public interface IRoleSeeder
    {
        Task SeedRolesAsync();
    }

    public class RoleSeeder : IRoleSeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleSeeder(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task SeedRolesAsync()
        {
            string[] roles = new[] { "User", "Moderator" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
