using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cd_segredo2FA",
                table: "users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "cd_tokenConfirmacao",
                table: "users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "cd_tokenRecuperacao",
                table: "users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "dt_expiracaoToken",
                table: "users",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "fl_2faHabilitado",
                table: "users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "fl_emailConfirmado",
                table: "users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cd_segredo2FA",
                table: "users");

            migrationBuilder.DropColumn(
                name: "cd_tokenConfirmacao",
                table: "users");

            migrationBuilder.DropColumn(
                name: "cd_tokenRecuperacao",
                table: "users");

            migrationBuilder.DropColumn(
                name: "dt_expiracaoToken",
                table: "users");

            migrationBuilder.DropColumn(
                name: "fl_2faHabilitado",
                table: "users");

            migrationBuilder.DropColumn(
                name: "fl_emailConfirmado",
                table: "users");
        }
    }
}
