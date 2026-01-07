using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Migrations
{
    /// <inheritdoc />
    public partial class TabelaLancamentoNovaColunaMudancaNome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "nr_parcelasRestantes",
                table: "lancamentos",
                newName: "nr_parcelaInicial");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "nr_parcelaInicial",
                table: "lancamentos",
                newName: "nr_parcelasRestantes");
        }
    }
}
