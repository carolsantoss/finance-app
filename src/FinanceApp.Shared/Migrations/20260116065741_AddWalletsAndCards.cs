using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletsAndCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_credit_card",
                table: "lancamentos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_wallet",
                table: "lancamentos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "wallets",
                columns: table => new
                {
                    id_wallet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nm_nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nm_tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nr_saldo_inicial = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    id_usuario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallets", x => x.id_wallet);
                    table.ForeignKey(
                        name: "FK_wallets_users_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "users",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "creditCards",
                columns: table => new
                {
                    id_credit_card = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nm_nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nm_bandeira = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nr_limite = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    nr_dia_fechamento = table.Column<int>(type: "int", nullable: false),
                    nr_dia_vencimento = table.Column<int>(type: "int", nullable: false),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    id_wallet_pagamento = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_creditCards", x => x.id_credit_card);
                    table.ForeignKey(
                        name: "FK_creditCards_users_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "users",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_creditCards_wallets_id_wallet_pagamento",
                        column: x => x.id_wallet_pagamento,
                        principalTable: "wallets",
                        principalColumn: "id_wallet");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_lancamentos_id_credit_card",
                table: "lancamentos",
                column: "id_credit_card");

            migrationBuilder.CreateIndex(
                name: "IX_lancamentos_id_wallet",
                table: "lancamentos",
                column: "id_wallet");

            migrationBuilder.CreateIndex(
                name: "IX_creditCards_id_usuario",
                table: "creditCards",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_creditCards_id_wallet_pagamento",
                table: "creditCards",
                column: "id_wallet_pagamento");

            migrationBuilder.CreateIndex(
                name: "IX_wallets_id_usuario",
                table: "wallets",
                column: "id_usuario");

            migrationBuilder.AddForeignKey(
                name: "FK_lancamentos_creditCards_id_credit_card",
                table: "lancamentos",
                column: "id_credit_card",
                principalTable: "creditCards",
                principalColumn: "id_credit_card");

            migrationBuilder.AddForeignKey(
                name: "FK_lancamentos_wallets_id_wallet",
                table: "lancamentos",
                column: "id_wallet",
                principalTable: "wallets",
                principalColumn: "id_wallet");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lancamentos_creditCards_id_credit_card",
                table: "lancamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_lancamentos_wallets_id_wallet",
                table: "lancamentos");

            migrationBuilder.DropTable(
                name: "creditCards");

            migrationBuilder.DropTable(
                name: "wallets");

            migrationBuilder.DropIndex(
                name: "IX_lancamentos_id_credit_card",
                table: "lancamentos");

            migrationBuilder.DropIndex(
                name: "IX_lancamentos_id_wallet",
                table: "lancamentos");

            migrationBuilder.DropColumn(
                name: "id_credit_card",
                table: "lancamentos");

            migrationBuilder.DropColumn(
                name: "id_wallet",
                table: "lancamentos");
        }
    }
}
