using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace _3_dataaccess;

public partial class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("messages_pkey");

            entity.ToTable("messages", "chat");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Chatmessage).HasColumnName("chatmessage");
            entity.Property(e => e.Roomid).HasColumnName("roomid");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Room).WithMany(p => p.Messages)
                .HasForeignKey(d => d.Roomid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("messages_roomid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Messages)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("messages_userid_fkey");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("rooms_pkey");

            entity.ToTable("rooms", "chat");

            entity.HasIndex(e => e.Chatname, "rooms_chatname_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Chatname).HasColumnName("chatname");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_pkey");

            entity.ToTable("user", "chat");

            entity.HasIndex(e => e.Username, "user_username_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat).HasColumnName("createdat");
            entity.Property(e => e.Passwordhash).HasColumnName("passwordhash");
            entity.Property(e => e.Username).HasColumnName("username");

            entity.HasMany(d => d.Rooms).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "Userroom",
                    r => r.HasOne<Room>().WithMany()
                        .HasForeignKey("Roomid")
                        .HasConstraintName("userrooms_roomid_fkey"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("Userid")
                        .HasConstraintName("userrooms_userid_fkey"),
                    j =>
                    {
                        j.HasKey("Userid", "Roomid").HasName("userrooms_pkey");
                        j.ToTable("userrooms", "chat");
                        j.IndexerProperty<string>("Userid").HasColumnName("userid");
                        j.IndexerProperty<string>("Roomid").HasColumnName("roomid");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
