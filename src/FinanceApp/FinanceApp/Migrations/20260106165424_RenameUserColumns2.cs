using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Migrations
{
    public partial class RenameUserColumns2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PASSO 1 — renomes temporários
            migrationBuilder.RenameColumn(
                name: "nm_email",
                table: "users",
                newName: "tmp_nome");

            migrationBuilder.RenameColumn(
                name: "hs_senha",
                table: "users",
                newName: "tmp_email");

            migrationBuilder.RenameColumn(
                name: "nm_nomeUsuario",
                table: "users",
                newName: "tmp_senha");

            // PASSO 2 — nomes finais corretos
            migrationBuilder.RenameColumn(
                name: "tmp_nome",
                table: "users",
                newName: "nm_nomeUsuario");

            migrationBuilder.RenameColumn(
                name: "tmp_email",
                table: "users",
                newName: "nm_email");

            migrationBuilder.RenameColumn(
                name: "tmp_senha",
                table: "users",
                newName: "hs_senha");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "nm_nomeUsuario",
                table: "users",
                newName: "tmp_nome");

            migrationBuilder.RenameColumn(
                name: "nm_email",
                table: "users",
                newName: "tmp_email");

            migrationBuilder.RenameColumn(
                name: "hs_senha",
                table: "users",
                newName: "tmp_senha");

            migrationBuilder.RenameColumn(
                name: "tmp_nome",
                table: "users",
                newName: "nm_email");

            migrationBuilder.RenameColumn(
                name: "tmp_email",
                table: "users",
                newName: "hs_senha");

            migrationBuilder.RenameColumn(
                name: "tmp_senha",
                table: "users",
                newName: "nm_nomeUsuario");
        }
    }
}
