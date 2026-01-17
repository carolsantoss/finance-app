using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinanceApp.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_plan",
                table: "users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "features",
                columns: table => new
                {
                    id_feature = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nm_key = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nm_label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ds_description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_features", x => x.id_feature);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "plans",
                columns: table => new
                {
                    id_plan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nm_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ds_description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nr_price = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    fl_isDefault = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plans", x => x.id_plan);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "planFeatures",
                columns: table => new
                {
                    id_planFeature = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_plan = table.Column<int>(type: "int", nullable: false),
                    id_feature = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planFeatures", x => x.id_planFeature);
                    table.ForeignKey(
                        name: "FK_planFeatures_features_id_feature",
                        column: x => x.id_feature,
                        principalTable: "features",
                        principalColumn: "id_feature",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_planFeatures_plans_id_plan",
                        column: x => x.id_plan,
                        principalTable: "plans",
                        principalColumn: "id_plan",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "features",
                columns: new[] { "id_feature", "ds_description", "nm_key", "nm_label" },
                values: new object[,]
                {
                    { 1, "Permite exportar transações e relatórios em JSON/CSV", "export_data", "Exportar Dados" },
                    { 2, "Crie quantas metas financeiras desejar", "unlimited_goals", "Metas Ilimitadas" },
                    { 3, "Gerencie contas de diferentes bancos", "multiple_wallets", "Múltiplas Carteiras" }
                });

            migrationBuilder.InsertData(
                table: "plans",
                columns: new[] { "id_plan", "ds_description", "fl_isDefault", "nm_name", "nr_price" },
                values: new object[,]
                {
                    { 1, "Plano básico para começar", true, "Gratuito", 0m },
                    { 2, "Acesso completo a todas as funcionalidades", false, "Premium", 29.90m }
                });

            migrationBuilder.InsertData(
                table: "planFeatures",
                columns: new[] { "id_planFeature", "id_feature", "id_plan" },
                values: new object[,]
                {
                    { 1, 1, 2 },
                    { 2, 2, 2 },
                    { 3, 3, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_id_plan",
                table: "users",
                column: "id_plan");

            migrationBuilder.CreateIndex(
                name: "IX_planFeatures_id_feature",
                table: "planFeatures",
                column: "id_feature");

            migrationBuilder.CreateIndex(
                name: "IX_planFeatures_id_plan",
                table: "planFeatures",
                column: "id_plan");

            migrationBuilder.AddForeignKey(
                name: "FK_users_plans_id_plan",
                table: "users",
                column: "id_plan",
                principalTable: "plans",
                principalColumn: "id_plan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_plans_id_plan",
                table: "users");

            migrationBuilder.DropTable(
                name: "planFeatures");

            migrationBuilder.DropTable(
                name: "features");

            migrationBuilder.DropTable(
                name: "plans");

            migrationBuilder.DropIndex(
                name: "IX_users_id_plan",
                table: "users");

            migrationBuilder.DropColumn(
                name: "id_plan",
                table: "users");
        }
    }
}
