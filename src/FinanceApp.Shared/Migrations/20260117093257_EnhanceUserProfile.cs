using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Shared.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ds_sobre",
                table: "users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "nm_funcao",
                table: "users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "nr_telefone",
                table: "users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ds_sobre",
                table: "users");

            migrationBuilder.DropColumn(
                name: "nm_funcao",
                table: "users");

            migrationBuilder.DropColumn(
                name: "nr_telefone",
                table: "users");
        }
    }
}
