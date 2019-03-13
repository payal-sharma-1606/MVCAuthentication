using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Configuration;


namespace AuthenticationDemo.DataAccess
{
    public class AuthenticationDB : DbContext
    {
        public AuthenticationDB() : base("AuthenticationDB")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasMany(u => u.Roles).WithMany(r => r.Users).Map(m =>
            {
                m.ToTable("UserRoles");
                m.MapLeftKey("UserID");
                m.MapRightKey("RoleID");
            });
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}