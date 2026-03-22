using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AlBadour.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguagePhysicianAndDocTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "treating_physician_name",
                table: "issued_documents",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "language",
                table: "document_requests",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "document_types",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000010"),
                columns: new[] { "name_ar", "name_en" },
                values: new object[] { "تقرير طبي مع جدول", "Medical Report with Table" });

            migrationBuilder.UpdateData(
                table: "document_types",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000012"),
                columns: new[] { "name_ar", "name_en" },
                values: new object[] { "تقرير طبي مع جدول + كشف حساب", "Medical Report with Table + Account Statement" });

            migrationBuilder.InsertData(
                table: "document_types",
                columns: new[] { "id", "created_at", "description_ar", "description_en", "is_active", "name_ar", "name_en", "template_path", "updated_at" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000013"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, "تقرير طبي بدون جدول", "Medical Report without Table", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000014"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, "تقرير طبي بدون جدول + كشف حساب", "Medical Report without Table + Account Statement", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "document_types",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "document_types",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000014"));

            migrationBuilder.DropColumn(
                name: "treating_physician_name",
                table: "issued_documents");

            migrationBuilder.DropColumn(
                name: "language",
                table: "document_requests");

            migrationBuilder.UpdateData(
                table: "document_types",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000010"),
                columns: new[] { "name_ar", "name_en" },
                values: new object[] { "تقرير طبي", "Medical Report" });

            migrationBuilder.UpdateData(
                table: "document_types",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000012"),
                columns: new[] { "name_ar", "name_en" },
                values: new object[] { "تقرير طبي + كشف حساب", "Medical Report + Account Statement" });
        }
    }
}
