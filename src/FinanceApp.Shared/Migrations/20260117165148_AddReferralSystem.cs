using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cd_referralCode",
                table: "users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "id_referrer",
                table: "users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nr_indicacoes",
                table: "users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_users_id_referrer",
                table: "users",
                column: "id_referrer");

            migrationBuilder.AddForeignKey(
                name: "FK_users_users_id_referrer",
                table: "users",
                column: "id_referrer",
                principalTable: "users",
                principalColumn: "id_usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_users_id_referrer",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_id_referrer",
                table: "users");

            migrationBuilder.DropColumn(
                name: "cd_referralCode",
                table: "users");

            migrationBuilder.DropColumn(
                name: "id_referrer",
                table: "users");

            migrationBuilder.DropColumn(
                name: "nr_indicacoes",
                table: "users");
        }
    }
}
