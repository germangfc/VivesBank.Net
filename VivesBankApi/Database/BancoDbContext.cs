using Microsoft.EntityFrameworkCore;

namespace VivesBankApi.Database;

public class BancoDbContext(DbContextOptions<BancoDbContext> options) : DbContext(options)
{
    
}