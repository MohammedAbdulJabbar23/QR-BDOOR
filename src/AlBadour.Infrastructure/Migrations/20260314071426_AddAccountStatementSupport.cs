using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlBadour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountStatementSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "issued_documents",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "draft",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "draft");

            migrationBuilder.AddColumn<string>(
                name: "account_statement_path",
                table: "issued_documents",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "account_statement_path",
                table: "issued_documents");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "issued_documents",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "draft",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValue: "draft");
        }
    }
}
