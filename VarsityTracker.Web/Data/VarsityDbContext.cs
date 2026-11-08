using Microsoft.EntityFrameworkCore;
using VarsityTracker.Core.Models;

namespace VarsityTracker.Web.Data
{
    public class VarsityDbContext : DbContext
    {
        public VarsityDbContext(DbContextOptions<VarsityDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Faculty> Faculties { get; set; } = null!;
        public DbSet<Sport> Sports { get; set; } = null!;
        public DbSet<Venue> Venues { get; set; } = null!;
        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<Match> Matches { get; set; } = null!;
        public DbSet<MatchScore> MatchScores { get; set; } = null!;
        public DbSet<MatchReminder> MatchReminders { get; set; } = null!;
        public DbSet<VenueFeedback> VenueFeedbacks { get; set; } = null!;
        public DbSet<MatchEvent> MatchEvents { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User MatricNo unique index
            modelBuilder.Entity<User>()
                .HasIndex(u => u.MatricNo)
                .IsUnique();

            // Faculty Code unique index
            modelBuilder.Entity<Faculty>()
                .HasIndex(f => f.FacultyCode)
                .IsUnique();

            // Player -> Faculty
            modelBuilder.Entity<Player>()
                .HasOne(p => p.Faculty)
                .WithMany(f => f.Players)
                .HasForeignKey(p => p.FacultyId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Faculty (Optional)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Faculty)
                .WithMany(f => f.FacultyUsers)
                .HasForeignKey(u => u.FacultyId)
                .OnDelete(DeleteBehavior.SetNull);

            // Match -> HomeFaculty
            modelBuilder.Entity<Match>()
                .HasOne(m => m.HomeFaculty)
                .WithMany()
                .HasForeignKey(m => m.HomeFacultyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Match -> AwayFaculty
            modelBuilder.Entity<Match>()
                .HasOne(m => m.AwayFaculty)
                .WithMany()
                .HasForeignKey(m => m.AwayFacultyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Match -> Sport
            modelBuilder.Entity<Match>()
                .HasOne(m => m.Sport)
                .WithMany(s => s.Matches)
                .HasForeignKey(m => m.SportId)
                .OnDelete(DeleteBehavior.Restrict);

            // Match -> Venue
            modelBuilder.Entity<Match>()
                .HasOne(m => m.Venue)
                .WithMany(v => v.Matches)
                .HasForeignKey(m => m.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            // Match -> Scorekeeper User
            modelBuilder.Entity<Match>()
                .HasOne(m => m.Scorekeeper)
                .WithMany(u => u.AssignedMatches)
                .HasForeignKey(m => m.ScorekeeperId)
                .OnDelete(DeleteBehavior.SetNull);

            // Match -> MatchScores
            modelBuilder.Entity<MatchScore>()
                .HasOne(ms => ms.Match)
                .WithMany(m => m.MatchScores)
                .HasForeignKey(ms => ms.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            // MatchReminder -> User & Match
            modelBuilder.Entity<MatchReminder>()
                .HasOne(mr => mr.User)
                .WithMany(u => u.MatchReminders)
                .HasForeignKey(mr => mr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MatchReminder>()
                .HasOne(mr => mr.Match)
                .WithMany(m => m.MatchReminders)
                .HasForeignKey(mr => mr.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            // VenueFeedback -> User & Venue
            modelBuilder.Entity<VenueFeedback>()
                .HasOne(vf => vf.User)
                .WithMany(u => u.VenueFeedbacks)
                .HasForeignKey(vf => vf.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VenueFeedback>()
                .HasOne(vf => vf.Venue)
                .WithMany(v => v.VenueFeedbacks)
                .HasForeignKey(vf => vf.VenueId)
                .OnDelete(DeleteBehavior.Cascade);

            // MatchEvent -> Match & Faculty
            modelBuilder.Entity<MatchEvent>()
                .HasOne(me => me.Match)
                .WithMany(m => m.Events)
                .HasForeignKey(me => me.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MatchEvent>()
                .HasOne(me => me.Faculty)
                .WithMany()
                .HasForeignKey(me => me.FacultyId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

