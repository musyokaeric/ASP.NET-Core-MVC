﻿using Bulky.WebRazor.Models;
using Microsoft.EntityFrameworkCore;

namespace Bulky.WebRazor.Data
{
    public class ApplicationRazorDbContext : DbContext
    {
        public ApplicationRazorDbContext(DbContextOptions<ApplicationRazorDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            List<Category> categories = new List<Category>
            {
                new Category { Id = 1, Name = "Action", DisplayOrder = 1},
                new Category { Id = 2, Name = "SciFi", DisplayOrder = 2},
                new Category { Id = 3, Name = "History", DisplayOrder = 3},
            };

            modelBuilder.Entity<Category>().HasData(categories);
        }
    }
}
