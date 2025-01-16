using Microsoft.EntityFrameworkCore;
using VivesBankApi.Rest.Clients.Models;
using VivesBankApi.Rest.Product.BankAccounts.Models;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Database;

public class BancoDbContext(DbContextOptions<BancoDbContext> options) : DbContext(options)
{
    
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<User> Users { get; set; }
}