using System.Data.Entity;
using CustomPageFramework.Database.DbModels;

namespace CustomPageFramework.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            builder.HasDefaultSchema("dbo");
        }

        public DbSet<ServerSetting> ServerSettings { get; set; }
    }
}
