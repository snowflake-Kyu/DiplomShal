using Microsoft.EntityFrameworkCore;
using SchoolApi.Models;

namespace SchoolApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<Auditorium> Auditoriums => Set<Auditorium>();
    public DbSet<ClubActivity> ClubActivities => Set<ClubActivity>();
    public DbSet<Discipline> Disciplines => Set<Discipline>();
    public DbSet<EducationalProgram> EducationalPrograms => Set<EducationalProgram>();
    public DbSet<GraduatingClass> GraduatingClasses => Set<GraduatingClass>();
    public DbSet<Rate> Rates => Set<Rate>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<SchoolClass> SchoolClasses => Set<SchoolClass>();
    public DbSet<StudentGroup> StudentGroups => Set<StudentGroup>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<TeachingAssigmentHistory> TeachingAssigmentHistories => Set<TeachingAssigmentHistory>();
    public DbSet<TeachingAssignment> TeachingAssignments => Set<TeachingAssignment>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Vote> Votes => Set<Vote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AcademicYear>(entity =>
        {
            entity.ToTable("AcademicYear");
            entity.HasKey(e => e.AcademicYearId).HasName("PK_AcademicYear");
            entity.Property(e => e.AcademicYearId).HasColumnName("AcademicYearID").ValueGeneratedNever();
            entity.Property(e => e.Year).HasColumnName("Year").IsRequired();
            entity.Property(e => e.Comment).HasMaxLength(100);
        });

        modelBuilder.Entity<Auditorium>(entity =>
        {
            entity.ToTable("Auditorium");
            entity.HasKey(e => e.AuditoriumId);
            entity.Property(e => e.AuditoriumId).HasColumnName("AuditoriumID").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<ClubActivity>(entity =>
        {
            entity.ToTable("ClubActivity");
            entity.HasKey(e => e.ClubActivityId);
            entity.Property(e => e.ClubActivityId).HasColumnName("ClubActivityID").ValueGeneratedOnAdd();
            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");
            entity.Property(e => e.Date).HasColumnType("date");
            entity.HasOne<StudentGroup>().WithMany().HasForeignKey(e => e.GroupId);
            entity.HasOne<Teacher>().WithMany().HasForeignKey(e => e.TeacherId);
        });

        modelBuilder.Entity<Discipline>(entity =>
        {
            entity.ToTable("Discipline");
            entity.HasKey(e => e.DisciplineId);
            entity.Property(e => e.DisciplineId).HasColumnName("DisciplineID").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ShortDescription).HasMaxLength(255);
        });

        modelBuilder.Entity<EducationalProgram>(entity =>
        {
            entity.ToTable("EducationalProgram");
            entity.HasKey(e => e.ProgramId);
            entity.Property(e => e.ProgramId).HasColumnName("ProgramID").ValueGeneratedOnAdd();
            entity.Property(e => e.DisciplineId).HasColumnName("DisciplineID");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.ShortDescription).HasMaxLength(255);
            entity.Property(e => e.Quarter1Hours).HasDefaultValue(0);
            entity.Property(e => e.Quarter2Hours).HasDefaultValue(0);
            entity.Property(e => e.Quarter3Hours).HasDefaultValue(0);
            entity.Property(e => e.Quarter4Hours).HasDefaultValue(0);
            entity.HasOne<Discipline>().WithMany().HasForeignKey(e => e.DisciplineId);
            entity.HasOne<SchoolClass>().WithMany().HasForeignKey(e => e.ClassId);
        });

        modelBuilder.Entity<GraduatingClass>(entity =>
        {
            entity.ToTable("GraduatingClass");
            entity.HasKey(e => e.GraduatingClassId).HasName("PK_GraduatingClass");
            entity.Property(e => e.GraduatingClassId).HasColumnName("GraduatingClassID").ValueGeneratedNever();
            entity.Property(e => e.ClassName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AcademicYearId).HasColumnName("AcademicYearID");
            entity.HasOne<AcademicYear>().WithMany().HasForeignKey(e => e.AcademicYearId).HasConstraintName("FK_GraduatingClass_AcademicYear");
        });

        modelBuilder.Entity<Rate>(entity =>
        {
            entity.ToTable("Rate");
            entity.HasKey(e => e.RateId).HasName("PK_Rate");
            entity.Property(e => e.RateId).HasColumnName("RateID").ValueGeneratedNever();
            entity.Property(e => e.RateName).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");
            entity.HasKey(e => e.RoleId).HasName("PK_Role");
            entity.Property(e => e.RoleId).HasColumnName("RoleID").ValueGeneratedNever();
            entity.Property(e => e.RoleName).HasColumnName("Role").HasMaxLength(30).IsFixedLength().IsRequired();
            entity.Property(e => e.Description).HasMaxLength(10).IsFixedLength();
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.ToTable("Schedule");
            entity.HasKey(e => e.ScheduleId);
            entity.Property(e => e.ScheduleId).HasColumnName("ScheduleID").ValueGeneratedOnAdd();
            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.DisciplineId).HasColumnName("DisciplineID");
            entity.Property(e => e.TimeSlotId).HasColumnName("TimeSlotID");
            entity.Property(e => e.AssignmentId).HasColumnName("AssignmentID");
            entity.HasOne<SchoolClass>().WithMany().HasForeignKey(e => e.ClassId);
            entity.HasOne<Discipline>().WithMany().HasForeignKey(e => e.DisciplineId);
            entity.HasOne<TimeSlot>().WithMany().HasForeignKey(e => e.TimeSlotId);
            entity.HasOne<TeachingAssignment>().WithMany().HasForeignKey(e => e.AssignmentId).HasConstraintName("FK_Schedule_Assignment");
        });

        modelBuilder.Entity<SchoolClass>(entity =>
        {
            entity.ToTable("SchoolClass");
            entity.HasKey(e => e.ClassId);
            entity.Property(e => e.ClassId).HasColumnName("ClassID").ValueGeneratedOnAdd();
            entity.Property(e => e.ClassName).HasMaxLength(50);
        });

        modelBuilder.Entity<StudentGroup>(entity =>
        {
            entity.ToTable("StudentGroup");
            entity.HasKey(e => e.GroupId);
            entity.Property(e => e.GroupId).HasColumnName("GroupID").ValueGeneratedOnAdd();
            entity.Property(e => e.GroupName).HasMaxLength(100);
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.ToTable("Teacher");
            entity.HasKey(e => e.TeacherId);
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID").ValueGeneratedOnAdd();
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
        });

        modelBuilder.Entity<TeachingAssigmentHistory>(entity =>
        {
            entity.ToTable("TeachingAssigmentHistory");
            entity.HasKey(e => e.TeachingAssigmentHistoryId).HasName("PK_TeachingAssigmentHistory");
            entity.Property(e => e.TeachingAssigmentHistoryId).HasColumnName("TeachingAssigmentHistoryID").ValueGeneratedNever();
            entity.Property(e => e.FullName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.DisciplineName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AcademicYear).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Comment).HasMaxLength(100);
        });

        modelBuilder.Entity<TeachingAssignment>(entity =>
        {
            entity.ToTable("TeachingAssignment");
            entity.HasKey(e => e.AssignmentId);
            entity.Property(e => e.AssignmentId).HasColumnName("AssignmentID").ValueGeneratedOnAdd();
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID").IsRequired();
            entity.Property(e => e.DisciplineId).HasColumnName("DisciplineID").IsRequired();
            entity.Property(e => e.AuditoriumId).HasColumnName("AuditoriumID");
            entity.Property(e => e.Comment).HasMaxLength(200);
            entity.HasOne<Teacher>().WithMany().HasForeignKey(e => e.TeacherId).HasConstraintName("FK_TA_Teacher");
            entity.HasOne<Discipline>().WithMany().HasForeignKey(e => e.DisciplineId).HasConstraintName("FK_TA_Discipline");
            entity.HasOne<Auditorium>().WithMany().HasForeignKey(e => e.AuditoriumId).HasConstraintName("FK_TA_Auditorium");
        });

        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.ToTable("TimeSlot");
            entity.HasKey(e => e.TimeSlotId);
            entity.Property(e => e.TimeSlotId).HasColumnName("TimeSlotID").ValueGeneratedOnAdd();
            entity.Property(e => e.TimeStart).HasColumnType("time(7)");
            entity.Property(e => e.TimeEnd).HasColumnType("time(7)");
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(e => e.UserId).HasName("PK_User");
            entity.Property(e => e.UserId).HasColumnName("UserID").ValueGeneratedNever();
            entity.Property(e => e.Login).HasMaxLength(30).IsFixedLength().IsRequired();
            entity.Property(e => e.Password).HasMaxLength(8).IsFixedLength().IsRequired();
            entity.Property(e => e.RoleId).HasColumnName("RoleID").IsRequired();
            entity.HasOne<Role>().WithMany().HasForeignKey(e => e.RoleId).HasConstraintName("FK_User_Role1");
        });

        modelBuilder.Entity<Vote>(entity =>
        {
            entity.ToTable("Vote");
            entity.HasKey(e => e.VoteId);
            entity.Property(e => e.VoteId).HasColumnName("VoteID").ValueGeneratedOnAdd();
            entity.Property(e => e.DisciplineId).HasColumnName("DisciplineID").IsRequired();
            entity.Property(e => e.RateId).HasColumnName("RateID").IsRequired();
            entity.HasOne<Discipline>().WithMany().HasForeignKey(e => e.DisciplineId).HasConstraintName("FK_Vote_Discipline");
            entity.HasOne<Rate>().WithMany().HasForeignKey(e => e.RateId).HasConstraintName("FK_Vote_Rate");
        });
    }
}
