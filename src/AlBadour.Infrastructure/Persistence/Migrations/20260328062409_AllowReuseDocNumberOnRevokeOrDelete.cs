using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlBadour.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AllowReuseDocNumberOnRevokeOrDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_issued_documents_document_number",
                table: "issued_documents");

            migrationBuilder.CreateIndex(
                name: "IX_issued_documents_document_number",
                table: "issued_documents",
                column: "document_number",
                unique: true,
                filter: "is_deleted = FALSE AND status != 'revoked'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_issued_documents_document_number",
                table: "issued_documents");

            migrationBuilder.CreateIndex(
                name: "IX_issued_documents_document_number",
                table: "issued_documents",
                column: "document_number",
                unique: true);
        }
    }
}
