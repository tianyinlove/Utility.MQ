using Emapp.Configuration.Model;
using Emapp.MQ.Core.Repositories;
using Emapp.MQ.UnitTest.DataAccess.EfTest;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Emapp.MQ.UnitTest.DataAccess.EfTest
{
    class DemoDbContext : DbContext, IMQDbContext
    {

        private readonly string _connectionString;

        public DemoDbContext()
        {
            _connectionString = "Application Name=MQ;Server=192.168.8.108;Database=MQ;User=sa;Password=emoney.cn;";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString, options =>
            {
                options.UseRowNumberForPaging();
            });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Basket>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });
        }
    }
}
