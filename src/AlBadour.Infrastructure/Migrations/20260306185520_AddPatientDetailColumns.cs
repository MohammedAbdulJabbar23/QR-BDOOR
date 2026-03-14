using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlBadour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientDetailColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "admission_date",
                table: "issued_documents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "discharge_date",
                table: "issued_documents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "leave_granted",
                table: "issued_documents",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "patient_age",
                table: "issued_documents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "patient_gender",
                table: "issued_documents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "patient_profession",
                table: "issued_documents",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "admission_date",
                table: "issued_documents");

            migrationBuilder.DropColumn(
                name: "discharge_date",
                table: "issued_documents");

            migrationBuilder.DropColumn(
                name: "leave_granted",
                table: "issued_documents");

            migrationBuilder.DropColumn(
                name: "patient_age",
                table: "issued_documents");

            migrationBuilder.DropColumn(
                name: "patient_gender",
                table: "issued_documents");

            migrationBuilder.DropColumn(
                name: "patient_profession",
                table: "issued_documents");
        }
    }
}
