using Microsoft.EntityFrameworkCore;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    
    public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options) : base(options) { } 
}