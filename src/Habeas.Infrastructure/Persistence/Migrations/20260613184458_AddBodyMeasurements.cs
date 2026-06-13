using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Habeas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBodyMeasurements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "body_metrics_height_cm",
                table: "users");

            migrationBuilder.DropColumn(
                name: "body_metrics_weight_kg",
                table: "users");

            migrationBuilder.CreateTable(
                name: "body_measurements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    metric = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_body_measurements", x => x.id);
                    table.ForeignKey(
                        name: "fk_body_measurements_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_body_measurements_user_id",
                table: "body_measurements",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "body_measurements");

            migrationBuilder.AddColumn<double>(
                name: "body_metrics_height_cm",
                table: "users",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "body_metrics_weight_kg",
                table: "users",
                type: "double precision",
                nullable: true);
        }
    }
}
