using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TourGuideMarketplace.Infrastructure.Persistence;

#nullable disable

namespace TourGuideMarketplace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260615120000_AddManualGuideReviewPhase")]
    public partial class AddManualGuideReviewPhase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeclaredCity",
                table: "UserVerifications",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeclaredCountry",
                table: "UserVerifications",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DeclaredDataReviewed",
                table: "UserVerifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DeclaredDocumentNumberLast4",
                table: "UserVerifications",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeclaredDocumentType",
                table: "UserVerifications",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeclaredLegalName",
                table: "UserVerifications",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EvidenceReceivedAt",
                table: "UserVerifications",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvidenceNotes",
                table: "UserVerifications",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EvidenceReviewStatus",
                table: "UserVerifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EvidenceReviewedAt",
                table: "UserVerifications",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EvidenceReviewedByUserId",
                table: "UserVerifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ManualDeclarationAcceptedAt",
                table: "UserVerifications",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManualInterviewChannel",
                table: "UserVerifications",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ManualInterviewCompletedAt",
                table: "UserVerifications",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManualInterviewNotes",
                table: "UserVerifications",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManualInterviewReference",
                table: "UserVerifications",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManualInterviewResult",
                table: "UserVerifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ManualInterviewReviewedByUserId",
                table: "UserVerifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ManualInterviewScheduledAt",
                table: "UserVerifications",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManualInterviewStatus",
                table: "UserVerifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ManualReviewCompletedAt",
                table: "UserVerifications",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManualReviewCompletedByUserId",
                table: "UserVerifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ManualReviewSubmittedAt",
                table: "UserVerifications",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PhoneContactedAt",
                table: "UserVerifications",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PhoneContactedByUserId",
                table: "UserVerifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneContactNotes",
                table: "UserVerifications",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ProfileCoherent",
                table: "UserVerifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReferencesReviewed",
                table: "UserVerifications",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DeclaredCity", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "DeclaredCountry", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "DeclaredDataReviewed", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "DeclaredDocumentNumberLast4", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "DeclaredDocumentType", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "DeclaredLegalName", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "EvidenceReceivedAt", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "EvidenceNotes", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "EvidenceReviewStatus", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "EvidenceReviewedAt", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "EvidenceReviewedByUserId", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "ManualDeclarationAcceptedAt", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "ManualInterviewChannel", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "ManualInterviewCompletedAt", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "ManualInterviewNotes", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "ManualInterviewReference", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "ManualInterviewResult", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "ManualInterviewReviewedByUserId", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "ManualInterviewScheduledAt", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "ManualInterviewStatus", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "ManualReviewCompletedAt", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "ManualReviewCompletedByUserId", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "ManualReviewSubmittedAt", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "PhoneContactedAt", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "PhoneContactedByUserId", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "PhoneContactNotes", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "ProfileCoherent", table: "UserVerifications");
            migrationBuilder.DropColumn(name: "ReferencesReviewed", table: "UserVerifications");
        }
    }
}
