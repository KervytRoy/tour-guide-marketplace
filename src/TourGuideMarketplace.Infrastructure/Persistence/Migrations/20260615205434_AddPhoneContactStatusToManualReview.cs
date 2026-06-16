using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourGuideMarketplace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneContactStatusToManualReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "InReviewReason",
                table: "UserVerifications",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PhoneContactStatus",
                table: "UserVerifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE [UserVerifications]
                SET [PhoneContactStatus] = 1
                WHERE [PhoneContactedAt] IS NOT NULL
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "AdminReviewCases",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneContactStatus",
                table: "UserVerifications");

            migrationBuilder.AlterColumn<string>(
                name: "InReviewReason",
                table: "UserVerifications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "AdminReviewCases",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);
        }
    }
}
