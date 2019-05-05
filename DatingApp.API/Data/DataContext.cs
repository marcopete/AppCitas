using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DataContext : DbContext //El DataContext hereda DbContext de EntityFramework
    {
        public DataContext(DbContextOptions<DataContext> options) : base (options) {}

        public DbSet <Value> Values { get; set; } //Nombre de tabla en sql
        public DbSet <User> Users { get; set; } //Nombre de tabla en sql
    }
}