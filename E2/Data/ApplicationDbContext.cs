using E2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;


namespace E2.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<PaymentProduct>()
        .HasOne(p => p.Buyer)
        .WithMany()
        .HasForeignKey(p => p.BuyerId)
        .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<User>()
          .HasOne(u => u.RefreshToken)
          .WithOne(t => t.User)
          .HasForeignKey<RefreshToken>(t => t.UserId)
          .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<PaymentProduct>()
        .HasOne(p => p.User)
        .WithMany()
        .HasForeignKey(p => p.SellerId)
        .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<PaymentProduct>()
        .HasOne(p => p.Product)
        .WithMany()
        .HasForeignKey(p => p.ProductId)
        .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<PaymentProduct>()
        .HasOne(p => p.Payment)
        .WithMany()
        .HasForeignKey(p => p.PaymentId)
        .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Comment>()
        .HasOne(p => p.Product)
        .WithMany()
        .HasForeignKey(p => p.ProductId)
        .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Comment>()
        .HasOne(p => p.User)
        .WithMany()
        .HasForeignKey(p => p.UserId)
        .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Product>()
        .HasOne(p => p.User)
        .WithMany()
        .HasForeignKey(p => p.UserId)
        .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<ProductRate>()
        .HasOne(p => p.User)
        .WithMany()
        .HasForeignKey(p => p.UserId)
        .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<ProductRate>()
        .HasOne(p => p.Product)
        .WithMany()
        .HasForeignKey(p => p.ProductId)
        .OnDelete(DeleteBehavior.NoAction);
        builder.Ignore<IdentityUser>();
        builder.Ignore<IdentityUserClaim<string>>();
        builder.Ignore<IdentityUserRole<string>>();
        builder.Ignore<IdentityUserLogin<string>>();
        builder.Ignore<IdentityUserToken<string>>();
        builder.Ignore<IdentityRoleClaim<string>>();
        builder.Ignore<IdentityRole>();
        builder.Ignore<IdentityUser<int>>();
        builder.Ignore<IdentityUserClaim<int>>();
        builder.Ignore<IdentityUserRole<int>>();
        builder.Ignore<IdentityUserLogin<int>>();
        builder.Ignore<IdentityUserToken<int>>();
        builder.Ignore<IdentityRole<int>>();
        builder.Ignore<IdentityRoleClaim<int>>();

        builder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.Property(u => u.FirstName).HasColumnName("FirstName");
            entity.Property(u => u.LastName).HasColumnName("LastName");
            entity.Property(u => u.Email).HasColumnName("Email");
            entity.Property(u => u.PhoneNumber).HasColumnName("PhoneNumber");
});
    }
    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<ProductRate> ProductRates { get; set; }
    public DbSet<PaymentProduct> PaymentProducts { get; set; }
    public DbSet<PassowrdReset> PassowrdReset { get; set;}
    public DbSet<DeviceToken> DeviceTokens { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}
