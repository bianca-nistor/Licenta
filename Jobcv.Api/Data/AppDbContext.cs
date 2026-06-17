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

        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<Application> Applications => Set<Application>();
        public DbSet<CvProject> CvProjects => Set<CvProject>();
        public DbSet<CvLanguage> CvLanguages => Set<CvLanguage>();
        public DbSet<CvCertification> CvCertifications => Set<CvCertification>();
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<UploadedCvFile> UploadedCvFiles { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
     .Property(u => u.Email)
     .HasMaxLength(256)
     .IsRequired();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.FullName)
                .HasMaxLength(150);
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

            modelBuilder.Entity<Application>()
     .HasOne(x => x.User)
     .WithMany()
     .HasForeignKey(x => x.UserId)
     .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Application>()
                .HasOne(x => x.Job)
                .WithMany(x => x.Applications)
                .HasForeignKey(x => x.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Application>()
                .HasOne(x => x.Cv)
                .WithMany()
                .HasForeignKey(x => x.CvId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<CvProject>()
              .HasOne(x => x.Cv)
               .WithMany(x => x.Projects)
              .HasForeignKey(x => x.CvId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CvLanguage>()
                .HasOne(x => x.Cv)
                .WithMany(x => x.Languages)
                .HasForeignKey(x => x.CvId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CvCertification>()
                .HasOne(x => x.Cv)
                .WithMany(x => x.Certifications)
                .HasForeignKey(x => x.CvId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cv>()
            .HasOne(x => x.ParentCv)
            .WithMany()
            .HasForeignKey(x => x.ParentCvId)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cv>()
                .HasOne(x => x.TargetJob)
                .WithMany()
                .HasForeignKey(x => x.TargetJobId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}