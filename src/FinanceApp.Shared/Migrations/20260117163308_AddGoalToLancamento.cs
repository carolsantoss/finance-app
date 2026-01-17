using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalToLancamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_goal",
                table: "lancamentos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_lancamentos_id_goal",
                table: "lancamentos",
                column: "id_goal");

            migrationBuilder.AddForeignKey(
                name: "FK_lancamentos_goals_id_goal",
                table: "lancamentos",
                column: "id_goal",
                principalTable: "goals",
                principalColumn: "id_goal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lancamentos_goals_id_goal",
                table: "lancamentos");

            migrationBuilder.DropIndex(
                name: "IX_lancamentos_id_goal",
                table: "lancamentos");

            migrationBuilder.DropColumn(
                name: "id_goal",
                table: "lancamentos");
        }
    }
}
