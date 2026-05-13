using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QuickBite.Delivery.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryAgents",
                columns: table => new
                {
                    AgentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VehicleType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VehicleNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CurrentLatitude = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    CurrentLongitude = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AvgRating = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    TotalDeliveries = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryAgents", x => x.AgentId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAgents_IsAvailable_IsVerified",
                table: "DeliveryAgents",
                columns: new[] { "IsAvailable", "IsVerified" });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAgents_UserId",
                table: "DeliveryAgents",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryAgents");
        }
    }
}
