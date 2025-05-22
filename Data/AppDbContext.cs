﻿using System;
using System.Collections.Generic;
using EduSyncWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSyncWebAPI.Data
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Assessment> Assessments { get; set; }
        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<Result> Results { get; set; }
        public virtual DbSet<User> Users { get; set; }

        // Added DbSets for Questions and Options
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<Option> Options { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer("Data Source=MINX_MONIKA;Initial Catalog=EduSyncDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Assessment>(entity =>
            {
                entity.HasKey(e => e.AssessmentId).HasName("PK__Assessme__3D2BF81E52B482AE");

                entity.Property(e => e.AssessmentId).ValueGeneratedNever();
                entity.Property(e => e.Title).HasMaxLength(100);

                entity.HasOne(d => d.Course).WithMany(p => p.Assessments)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("FK__Assessmen__Cours__3D5E1FD2");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.CourseId).HasName("PK__Courses__C92D71A7FB54E308");

                entity.Property(e => e.CourseId).ValueGeneratedNever();
                entity.Property(e => e.Title).HasMaxLength(100);

                entity.HasOne(d => d.Instructor).WithMany(p => p.Courses)
                    .HasForeignKey(d => d.InstructorId)
                    .HasConstraintName("FK__Courses__Instruc__3A81B327");
            });

            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasKey(e => e.ResultId).HasName("PK__Results__9769020818F10D08");

                entity.Property(e => e.ResultId).ValueGeneratedNever();
                entity.Property(e => e.AttemptDate).HasColumnType("datetime");

                entity.HasOne(d => d.Assessment).WithMany(p => p.Results)
                    .HasForeignKey(d => d.AssessmentId)
                    .HasConstraintName("FK__Results__Assessm__403A8C7D");

                entity.HasOne(d => d.User).WithMany(p => p.Results)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Results__UserId__412EB0B6");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C07A2A1FD");

                entity.HasIndex(e => e.Email, "UQ__Users__A9D10534977B39F7").IsUnique();

                entity.Property(e => e.UserId).ValueGeneratedNever();
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Role).HasMaxLength(50);
            });

            // New: Configure Question entity
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.QuestionId);

                entity.Property(e => e.Text).HasMaxLength(500);

                entity.HasOne(q => q.Assessment)
                    .WithMany(a => a.Questions)
                    .HasForeignKey(q => q.AssessmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // New: Configure Option entity
            modelBuilder.Entity<Option>(entity =>
            {
                entity.HasKey(e => e.OptionId);

                entity.Property(e => e.Text).HasMaxLength(300);

                entity.HasOne(o => o.Question)
                    .WithMany(q => q.Options)
                    .HasForeignKey(o => o.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
