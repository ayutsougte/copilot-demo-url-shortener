using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShortLinkApp.Api.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260302_InitialCreate")]
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Links",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                              .Annotation("Sqlite:Autoincrement", true),
                    ShortCode = table.Column<string>(nullable: false),
                    OriginalUrl = table.Column<string>(nullable: false),
                    CustomAlias = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ExpiresAt = table.Column<DateTime>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Links", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClickEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                              .Annotation("Sqlite:Autoincrement", true),
                    LinkId = table.Column<int>(nullable: false),
                    ClickedAt = table.Column<DateTime>(nullable: false),
                    Referrer = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClickEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClickEvents_Links_LinkId",
                        column: x => x.LinkId,
                        principalTable: "Links",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Links_ShortCode",
                table: "Links",
                column: "ShortCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Links_CustomAlias",
                table: "Links",
                column: "CustomAlias",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClickEvents_LinkId",
                table: "ClickEvents",
                column: "LinkId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ClickEvents");
            migrationBuilder.DropTable(name: "Links");
        }
    }
}
