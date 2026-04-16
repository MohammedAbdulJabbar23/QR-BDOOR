using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlBadour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMoiInsuranceLetterDocType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "document_types",
                columns: new[] { "id", "created_at", "description_ar", "description_en", "is_active", "name_ar", "name_en", "template_path", "updated_at" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000015"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, "كتاب ضمان وزارة الداخلية", "MOI Insurance Letter", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "document_types",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000015"));
        }
    }
}
