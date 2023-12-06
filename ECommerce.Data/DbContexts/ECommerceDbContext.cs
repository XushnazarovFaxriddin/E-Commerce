using ECommerce.Data.Helpers;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Data.DbContexts;

public class ECommerceDbContext : DbContext
{
    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options)
    {
    }

    #region User Tables
    public DbSet<User> Users { get; set; }
    public DbSet<UserComment> UserComments { get; set; }
    #endregion

    #region Branch Tables
    public DbSet<Branch> Branches { get; set; }
    #endregion
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        QueryFilterHelper.AddQueryFilters(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }
}
