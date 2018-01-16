using Microsoft.EntityFrameworkCore;
using System;

namespace CashDesk {
    public class MemberContext : DbContext {

        public DbSet<Member> Members { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<Deposit> Deposits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Member>().HasKey(m => m.LastName);
            modelBuilder.Entity<Member>().HasMany<Membership>().WithOne().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Membership>().HasMany<Deposit>().WithOne().OnDelete(DeleteBehavior.Cascade);

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            if (!optionsBuilder.IsConfigured) {
                optionsBuilder.UseInMemoryDatabase("MembersDB" + Guid.NewGuid());
            }
        }
    }
}
