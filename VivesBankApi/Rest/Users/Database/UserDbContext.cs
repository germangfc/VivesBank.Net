using Microsoft.EntityFrameworkCore;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Database;

public class UserDbContext(DbContextOptions<UserDbContext> options)
    : DbContext(options) 
{
    public DbSet<User> Users { get; set; }
}