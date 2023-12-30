using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MedicalCertificates.Models
{
    public partial class MedicalCertificatesDbContext : DbContext
    {
        public MedicalCertificatesDbContext()
        {
        }

        public MedicalCertificatesDbContext(DbContextOptions<MedicalCertificatesDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CertificatesTable> CertificatesTables { get; set; } = null!;
        public virtual DbSet<CoursesTable> CoursesTables { get; set; } = null!;
        public virtual DbSet<DataGridView> DataGridViews { get; set; } = null!;
        public virtual DbSet<DepartmentsTable> DepartmentsTables { get; set; } = null!;
        public virtual DbSet<GroupsTable> GroupsTables { get; set; } = null!;
        public virtual DbSet<HealthGroupTable> HealthGroupTables { get; set; } = null!;
        public virtual DbSet<PegroupTable> PegroupTables { get; set; } = null!;
        public virtual DbSet<StudentsCertificatesView> StudentsCertificatesViews { get; set; } = null!;
        public virtual DbSet<StudentsGroupArchiveTable> StudentsGroupArchiveTables { get; set; } = null!;
        public virtual DbSet<StudentsTable> StudentsTables { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=MedicalCertificatesDb;Integrated Security=True; Trusted_Connection=True; TrustServerCertificate=true;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CertificatesTable>(entity =>
            {
                entity.HasKey(e => e.CertificateId)
                    .HasName("PK__Certific__BBF8A7C1E6020232");

                entity.ToTable("Certificates_table");

                entity.Property(e => e.IssueDate).HasColumnType("date");

                entity.Property(e => e.PegroupId).HasColumnName("PEGroupId");

                entity.Property(e => e.ValidDate).HasColumnType("date");

                entity.HasOne(d => d.HealthGroup)
                    .WithMany(p => p.CertificatesTables)
                    .HasForeignKey(d => d.HealthGroupId)
                    .HasConstraintName("FK_HealthGroup");

                entity.HasOne(d => d.Pegroup)
                    .WithMany(p => p.CertificatesTables)
                    .HasForeignKey(d => d.PegroupId)
                    .HasConstraintName("FK_PEGroup");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.CertificatesTables)
                    .HasForeignKey(d => d.StudentId)
                    .HasConstraintName("FK_Student");
            });

            modelBuilder.Entity<CoursesTable>(entity =>
            {
                entity.HasKey(e => e.CourseId)
                    .HasName("PK__Courses___C92D71A732E7C0EF");

                entity.ToTable("Courses_table");

                entity.Property(e => e.Number).HasDefaultValueSql("('1')");

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.CoursesTables)
                    .HasForeignKey(d => d.DepartmentId)
                    .HasConstraintName("FK_Department");
            });

            modelBuilder.Entity<DataGridView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("DataGrid_view");

                entity.Property(e => e.BirthDate).HasColumnType("date");

                entity.Property(e => e.FirstName).HasMaxLength(20);

                entity.Property(e => e.HealthGroup).HasMaxLength(20);

                entity.Property(e => e.IssueDate).HasColumnType("date");

                entity.Property(e => e.Pegroup)
                    .HasMaxLength(20)
                    .HasColumnName("PEGroup");

                entity.Property(e => e.SecondName).HasMaxLength(20);

                entity.Property(e => e.ThirdName).HasMaxLength(20);

                entity.Property(e => e.ValidDate).HasColumnType("date");
            });

            modelBuilder.Entity<DepartmentsTable>(entity =>
            {
                entity.HasKey(e => e.DepartmentId)
                    .HasName("PK__Departme__B2079BED5E0D80D8");

                entity.ToTable("Departments_table");

                entity.HasIndex(e => e.Name, "UQ__Departme__737584F6779E1CD7")
                    .IsUnique();

                entity.Property(e => e.MaxCourse).HasDefaultValueSql("('3')");

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<GroupsTable>(entity =>
            {
                entity.HasKey(e => e.GroupId)
                    .HasName("PK__Groups_t__149AF36A5F17C13D");

                entity.ToTable("Groups_table");

                entity.Property(e => e.Name).HasMaxLength(5);

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.GroupsTables)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("FK_Course");
            });

            modelBuilder.Entity<HealthGroupTable>(entity =>
            {
                entity.HasKey(e => e.HealthGroupId)
                    .HasName("PK__HealthGr__041005CFEDA9C869");

                entity.ToTable("HealthGroup_table");

                entity.Property(e => e.HealthGroup).HasMaxLength(20);
            });

            modelBuilder.Entity<PegroupTable>(entity =>
            {
                entity.HasKey(e => e.PegroupId)
                    .HasName("PK__PEGroup___017BF80F4ABC8BF9");

                entity.ToTable("PEGroup_table");

                entity.Property(e => e.PegroupId).HasColumnName("PEGroupId");

                entity.Property(e => e.Pegroup)
                    .HasMaxLength(20)
                    .HasColumnName("PEGroup");
            });

            modelBuilder.Entity<StudentsCertificatesView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("StudentsCertificates_view");

                entity.Property(e => e.BirthDate).HasColumnType("date");

                entity.Property(e => e.FirstName).HasMaxLength(20);

                entity.Property(e => e.HealthGroup).HasMaxLength(20);

                entity.Property(e => e.IssueDate).HasColumnType("date");

                entity.Property(e => e.Pegroup)
                    .HasMaxLength(20)
                    .HasColumnName("PEGroup");

                entity.Property(e => e.SecondName).HasMaxLength(20);

                entity.Property(e => e.ThirdName).HasMaxLength(20);

                entity.Property(e => e.ValidDate).HasColumnType("date");
            });

            modelBuilder.Entity<StudentsGroupArchiveTable>(entity =>
            {
                entity.HasKey(e => e.NoteId)
                    .HasName("PK__Students__EACE355F5C18580F");

                entity.ToTable("StudentsGroupArchive_table");

                entity.Property(e => e.AlterDate)
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<StudentsTable>(entity =>
            {
                entity.HasKey(e => e.StudentId)
                    .HasName("PK__Students__32C52B99B3EDB8E3");

                entity.ToTable("Students_table");

                entity.Property(e => e.BirthDate).HasColumnType("date");

                entity.Property(e => e.FirstName).HasMaxLength(20);

                entity.Property(e => e.SecondName).HasMaxLength(20);

                entity.Property(e => e.ThirdName).HasMaxLength(20);

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.StudentsTables)
                    .HasForeignKey(d => d.GroupId)
                    .HasConstraintName("FK_Group");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
