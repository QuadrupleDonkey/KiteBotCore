﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using KiteBotCore;

namespace KiteBotCore.Migrations
{
    [DbContext(typeof(KiteBotDbContext))]
    [Migration("20170210000132_MessagesTypeMigration")]
    partial class MessagesTypeMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752");

            modelBuilder.Entity("KiteBotCore.Channel", b =>
                {
                    b.Property<long>("ChannelId")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("GuildForeignKey");

                    b.Property<string>("Name");

                    b.HasKey("ChannelId");

                    b.HasIndex("GuildForeignKey");

                    b.ToTable("Channels");
                });

            modelBuilder.Entity("KiteBotCore.Guild", b =>
                {
                    b.Property<long>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("GuildId");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("KiteBotCore.Message", b =>
                {
                    b.Property<long>("MessageId")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("ChannelForeignKey");

                    b.Property<string>("Content");

                    b.Property<long?>("UserForeignKey");

                    b.HasKey("MessageId");

                    b.HasIndex("ChannelForeignKey");

                    b.HasIndex("UserForeignKey");

                    b.ToTable("Posts");
                });

            modelBuilder.Entity("KiteBotCore.User", b =>
                {
                    b.Property<long>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("GuildForeignKey");

                    b.Property<DateTimeOffset?>("JoinedAt");

                    b.Property<DateTimeOffset>("LastActivityAt");

                    b.Property<string>("Name");

                    b.HasKey("UserId");

                    b.HasIndex("GuildForeignKey");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("KiteBotCore.Channel", b =>
                {
                    b.HasOne("KiteBotCore.Guild", "Guild")
                        .WithMany("Channels")
                        .HasForeignKey("GuildForeignKey");
                });

            modelBuilder.Entity("KiteBotCore.Message", b =>
                {
                    b.HasOne("KiteBotCore.Channel", "Channel")
                        .WithMany("Messages")
                        .HasForeignKey("ChannelForeignKey");

                    b.HasOne("KiteBotCore.User", "User")
                        .WithMany("Messages")
                        .HasForeignKey("UserForeignKey");
                });

            modelBuilder.Entity("KiteBotCore.User", b =>
                {
                    b.HasOne("KiteBotCore.Guild", "Guild")
                        .WithMany("Users")
                        .HasForeignKey("GuildForeignKey");
                });
        }
    }
}
