using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bricks.Models;
using Microsoft.EntityFrameworkCore;

namespace Bricks.Data
{
    public class AppDBContext:DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options): base(options)
        {
            
        }
        public DbSet<Production> production {get; set;}
        public DbSet<Sales> buyers { get; set;}
        public DbSet<Login> login{get;set;}
        public DbSet<Unit> Unit {get; set;}
        public DbSet<Catagory> Catagory { get; set;}
        public DbSet<Customer> Customer { get; set;}
        public DbSet<Product> product { get; set;}
    }
}