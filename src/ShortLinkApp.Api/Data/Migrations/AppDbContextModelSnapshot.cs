using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace ShortLinkApp.Api.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.0");

            modelBuilder.Entity("ShortLinkApp.Api.Data.Link", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<string>("CustomAlias")
                    .HasColumnType("TEXT");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("TEXT");

                b.Property<DateTime?>("ExpiresAt")
                    .HasColumnType("TEXT");

                b.Property<bool>("IsActive")
                    .HasColumnType("INTEGER");

                b.Property<string>("OriginalUrl")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("ShortCode")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("ShortCode")
                    .IsUnique();

                b.HasIndex("CustomAlias")
                    .IsUnique();

                b.ToTable("Links");
            });

            modelBuilder.Entity("ShortLinkApp.Api.Data.ClickEvent", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<DateTime>("ClickedAt")
                    .HasColumnType("TEXT");

                b.Property<int>("LinkId")
                    .HasColumnType("INTEGER");

                b.Property<string>("Referrer")
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("LinkId");

                b.ToTable("ClickEvents");
            });

            modelBuilder.Entity("ShortLinkApp.Api.Data.ClickEvent", b =>
            {
                b.HasOne("ShortLinkApp.Api.Data.Link")
                    .WithMany("ClickEvents")
                    .HasForeignKey("LinkId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });
        }
    }
}
