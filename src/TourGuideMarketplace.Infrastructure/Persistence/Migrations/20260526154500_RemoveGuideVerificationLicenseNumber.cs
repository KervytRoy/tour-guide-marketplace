using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TourGuideMarketplace.Infrastructure.Persistence;

#nullable disable

namespace TourGuideMarketplace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260526154500_RemoveGuideVerificationLicenseNumber")]
    public partial class RemoveGuideVerificationLicenseNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicenseNumber",
                table: "GuideVerifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LicenseNumber",
                table: "GuideVerifications",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);
        }
    }
}
