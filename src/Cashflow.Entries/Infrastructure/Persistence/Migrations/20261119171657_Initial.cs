using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cashflow.Entries.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cash_registers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<string>(type: "text", nullable: false),
                    business_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    opened_by = table.Column<string>(type: "text", nullable: false),
                    opened_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    closed_by = table.Column<string>(type: "text", nullable: true),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cash_registers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cash_register_id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<string>(type: "text", nullable: false),
                    business_date = table.Column<DateOnly>(type: "date", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_entries_cash_registers_cash_register_id",
                        column: x => x.cash_register_id,
                        principalTable: "cash_registers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_entries_cash_register_id",
                table: "entries",
                column: "cash_register_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entries");

            migrationBuilder.DropTable(
                name: "cash_registers");
        }
    }
}
