using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlBadour.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalReportUploadedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "medical_report_uploaded_at",
                table: "issued_documents",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "medical_report_uploaded_at",
                table: "issued_documents");
        }
    }
}
