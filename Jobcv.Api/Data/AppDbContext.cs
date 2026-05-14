using JobCv.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobCv.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Cv> Cvs => Set<Cv>();
        public DbSet<CvSkill> CvSkills => Set<CvSkill>();
        public DbSet<CvExperience> CvExperiences => Set<CvExperience>();
        public DbSet<CvEducation> CvEducations => Set<CvEducation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<Cv>()
                .HasOne(x => x.User)
                .WithMany(x => x.Cvs)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CvSkill>()
                .HasOne(x => x.Cv)
                .WithMany(x => x.Skills)
                .HasForeignKey(x => x.CvId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CvExperience>()
                .HasOne(x => x.Cv)
                .WithMany(x => x.Experiences)
                .HasForeignKey(x => x.CvId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CvEducation>()
                .HasOne(x => x.Cv)
                .WithMany(x => x.Educations)
                .HasForeignKey(x => x.CvId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}