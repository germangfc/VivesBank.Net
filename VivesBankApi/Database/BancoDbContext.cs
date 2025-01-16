using Microsoft.EntityFrameworkCore;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Database;

public class BancoDbContext : DbContext
{
    public BancoDbContext(DbContextOptions<BancoDbContext> options) 
        : base(options)
    {
    }
    
    public DbSet<User> Cuentas { get; set; }
    public DbSet<User> Users { get; set; }
}