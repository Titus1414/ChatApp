using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ChatApp.Models
{
    public partial class ChatAppDbContext : DbContext
    {
        public ChatAppDbContext()
        {
        }

        public ChatAppDbContext(DbContextOptions<ChatAppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ChatUser> ChatUsers { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UsersMessage> UsersMessages { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=DESKTOP-NNNJB7B;Database=ChatAppDb;Trusted_Connection=True;MultipleActiveResultSets=true;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatUser>(entity =>
            {
                entity.Property(e => e.Avatar)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.ConnId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastMessage)
                    .HasMaxLength(550)
                    .IsUnicode(false);

                entity.Property(e => e.Ufrom).HasColumnName("UFrom");

                entity.Property(e => e.UserName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Uto).HasColumnName("UTo");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Email)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Fname)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("FName");

                entity.Property(e => e.Image)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Lname)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("LName");

                entity.Property(e => e.Mname)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("MName");

                entity.Property(e => e.Passwrod)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Phone)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UsersMessage>(entity =>
            {
                entity.Property(e => e.ConnFrom)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ConnTo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Message).IsUnicode(false);

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UidFrom).HasColumnName("UIdFrom");

                entity.Property(e => e.UidTo).HasColumnName("UIdTo");

                entity.HasOne(d => d.Chat)
                    .WithMany(p => p.UsersMessages)
                    .HasForeignKey(d => d.ChatId)
                    .HasConstraintName("FK_UsersMessages_ChatUsers");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
