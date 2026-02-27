using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Infrastructure.Security.Entities;
using Microsoft.AspNetCore.Identity;
using Domain.Entities;

namespace Infrastructure.Data;

public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<audit_log> Audit_logs { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<Item> Items { get; set; }
    public virtual DbSet<User> users { get; set; }
    public virtual DbSet<Security.Entities.RefreshToken> RefreshTokens { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<audit_log>(entity =>
        {
            entity.HasKey(e => e.id).HasName("audit_logs_pkey");

            entity.Property(e => e.id)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.action_type).HasMaxLength(50);

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("GETDATE()")   
                .HasColumnType("datetime");        

            entity.Property(e => e.module).HasMaxLength(50);
        });



        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETDATE()")  
                .HasColumnType("datetime");        

            entity.Property(e => e.Email).HasMaxLength(150);

            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.Property(e => e.LastLogin)
                .HasColumnType("datetime");        

            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.Property(e => e.SecurityAnswer).HasMaxLength(50);

            entity.Property(e => e.SecurityQuestion).HasMaxLength(150);

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETDATE()")  
                .HasColumnType("datetime");        

            entity.Property(e => e.Username).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
