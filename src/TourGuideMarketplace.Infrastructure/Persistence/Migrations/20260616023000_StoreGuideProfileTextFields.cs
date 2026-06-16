using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourGuideMarketplace.Infrastructure.Persistence.Migrations
{
    [Migration("20260616023000_StoreGuideProfileTextFields")]
    public partial class StoreGuideProfileTextFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Languages",
                table: "GuideProfiles",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Specialties",
                table: "GuideProfiles",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE profile
                SET
                    Languages = COALESCE(languages.Value, ''),
                    Specialties = COALESCE(specialties.Value, '')
                FROM GuideProfiles AS profile
                OUTER APPLY (
                    SELECT STRING_AGG([Name], ', ') WITHIN GROUP (ORDER BY [Name]) AS Value
                    FROM GuideLanguages
                    WHERE GuideProfileId = profile.Id AND IsDeleted = 0
                ) AS languages
                OUTER APPLY (
                    SELECT STRING_AGG([Name], ', ') WITHIN GROUP (ORDER BY [Name]) AS Value
                    FROM GuideSpecialties
                    WHERE GuideProfileId = profile.Id AND IsDeleted = 0
                ) AS specialties;
                """);

            migrationBuilder.DropTable(
                name: "GuideLanguages");

            migrationBuilder.DropTable(
                name: "GuideSpecialties");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuideLanguages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    GuideProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuideLanguages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuideLanguages_GuideProfiles_GuideProfileId",
                        column: x => x.GuideProfileId,
                        principalTable: "GuideProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuideSpecialties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    GuideProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuideSpecialties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuideSpecialties_GuideProfiles_GuideProfileId",
                        column: x => x.GuideProfileId,
                        principalTable: "GuideProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuideLanguages_GuideProfileId_Name",
                table: "GuideLanguages",
                columns: new[] { "GuideProfileId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_GuideSpecialties_GuideProfileId_Name",
                table: "GuideSpecialties",
                columns: new[] { "GuideProfileId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.Sql(
                """
                INSERT INTO GuideLanguages (Id, GuideProfileId, [Name], CreatedAt, UpdatedAt, IsDeleted)
                SELECT NEWID(), Id, LEFT(Languages, 80), CreatedAt, NULL, 0
                FROM GuideProfiles
                WHERE Languages <> '';

                INSERT INTO GuideSpecialties (Id, GuideProfileId, [Name], CreatedAt, UpdatedAt, IsDeleted)
                SELECT NEWID(), Id, LEFT(Specialties, 80), CreatedAt, NULL, 0
                FROM GuideProfiles
                WHERE Specialties <> '';
                """);

            migrationBuilder.DropColumn(
                name: "Languages",
                table: "GuideProfiles");

            migrationBuilder.DropColumn(
                name: "Specialties",
                table: "GuideProfiles");
        }
    }
}
