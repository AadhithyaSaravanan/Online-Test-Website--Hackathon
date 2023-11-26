using Microsoft.EntityFrameworkCore;

namespace Mark2API.Models
{
    public class OnlineDbContext : DbContext
    {
        public OnlineDbContext(DbContextOptions<OnlineDbContext> options) : base(options)
        {
        }

        // Define your DbSet properties for your entities
        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Question> Questions { get; set; }

        public DbSet<TestResultReport> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure foreign key relationship between Question and Course
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Course)
                .WithMany(c => c.Questions)
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Restrict); // Choose the appropriate delete behavior



            // Configure the unique index for the Email column in the User entity
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

          

            base.OnModelCreating(modelBuilder);
        }
    }
}
