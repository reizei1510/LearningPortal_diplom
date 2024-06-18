using LearningPortal.DataBaseTables;
using Microsoft.EntityFrameworkCore;

namespace LearningPortal.Services
{
    public class UserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly ApplicationContext _db;

        public UserService(ILogger<UserService> logger, ApplicationContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<User?> GetUserByName(string name)
        {
            return await _db.Users
                .FirstOrDefaultAsync(u => u.Name == name);
        }

        public async Task CreateUser(string name, string password = "")
        {
            User user = new User()
            {
                Name = name,
                Date = DateTime.UtcNow
            };

            if (password != "")
            {
                string salt = Hasher.GenerateSalt();
                user.Salt = salt;
                string hashedPassword = Hasher.Hash(password, salt);
                user.Password = hashedPassword;
            }

            _logger.LogInformation($"Created user {name}.");

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
        }
    }
}