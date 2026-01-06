using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Migrations
{
    public partial class RenameLancamentoColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove FK antiga
            migrationBuilder.DropForeignKey(
                name: "FK_lancamentos_users_hs_id_user",
                table: "lancamentos");

            // Remove índice antigo
            migrationBuilder.DropIndex(
                name: "IX_lancamentos_hs_id_user",
                table: "lancamentos");

            // Renomeia colunas
            migrationBuilder.RenameColumn(
                name: "hs_id_lancamento",
                table: "lancamentos",
                newName: "id_lancamento");

            migrationBuilder.RenameColumn(
                name: "hs_id_user",
                table: "lancamentos",
                newName: "id_usuario");

            migrationBuilder.RenameColumn(
                name: "hs_tipo",
                table: "lancamentos",
                newName: "nm_tipo");

            migrationBuilder.RenameColumn(
                name: "hs_descricao",
                table: "lancamentos",
                newName: "nm_descricao");

            migrationBuilder.RenameColumn(
                name: "hs_valor",
                table: "lancamentos",
                newName: "nr_valor");

            migrationBuilder.RenameColumn(
                name: "hs_data",
                table: "lancamentos",
                newName: "dt_dataLancamento");

            // Cria novo índice com nome novo
            migrationBuilder.CreateIndex(
                name: "IX_lancamentos_id_usuario",
                table: "lancamentos",
                column: "id_usuario");

            // Cria nova FK com nome novo
            migrationBuilder.AddForeignKey(
                name: "FK_lancamentos_users_id_usuario",
                table: "lancamentos",
                column: "id_usuario",
                principalTable: "users",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lancamentos_users_id_usuario",
                table: "lancamentos");

            migrationBuilder.DropIndex(
                name: "IX_lancamentos_id_usuario",
                table: "lancamentos");

            migrationBuilder.RenameColumn(
                name: "id_lancamento",
                table: "lancamentos",
                newName: "hs_id_lancamento");

            migrationBuilder.RenameColumn(
                name: "id_usuario",
                table: "lancamentos",
                newName: "hs_id_user");

            migrationBuilder.RenameColumn(
                name: "nm_tipo",
                table: "lancamentos",
                newName: "hs_tipo");

            migrationBuilder.RenameColumn(
                name: "nm_descricao",
                table: "lancamentos",
                newName: "hs_descricao");

            migrationBuilder.RenameColumn(
                name: "nr_valor",
                table: "lancamentos",
                newName: "hs_valor");

            migrationBuilder.RenameColumn(
                name: "dt_dataLancamento",
                table: "lancamentos",
                newName: "hs_data");

            migrationBuilder.CreateIndex(
                name: "IX_lancamentos_hs_id_user",
                table: "lancamentos",
                column: "hs_id_user");

            migrationBuilder.AddForeignKey(
                name: "FK_lancamentos_users_hs_id_user",
                table: "lancamentos",
                column: "hs_id_user",
                principalTable: "users",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
