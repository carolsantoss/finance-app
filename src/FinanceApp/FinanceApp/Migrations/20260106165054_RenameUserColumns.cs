using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "hs_password_hash",
                table: "users",
                newName: "nm_nomeUsuario");

            migrationBuilder.RenameColumn(
                name: "hs_nome",
                table: "users",
                newName: "nm_email");

            migrationBuilder.RenameColumn(
                name: "hs_email",
                table: "users",
                newName: "hs_senha");

            migrationBuilder.RenameColumn(
                name: "hs_id_user",
                table: "users",
                newName: "id_usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "nm_nomeUsuario",
                table: "users",
                newName: "hs_password_hash");

            migrationBuilder.RenameColumn(
                name: "nm_email",
                table: "users",
                newName: "hs_nome");

            migrationBuilder.RenameColumn(
                name: "hs_senha",
                table: "users",
                newName: "hs_email");

            migrationBuilder.RenameColumn(
                name: "id_usuario",
                table: "users",
                newName: "hs_id_user");
        }
    }
}
