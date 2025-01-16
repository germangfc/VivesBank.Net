using Microsoft.EntityFrameworkCore;
using VivesBankApi.Rest.Product.BankAccounts.Models;

namespace VivesBankApi.Database;

public class BancoDbContext : DbContext
{
    public BancoDbContext(DbContextOptions<BancoDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Account> Cuentas { get; set; }
    public DbSet<Account> Clientes { get; set; }
}