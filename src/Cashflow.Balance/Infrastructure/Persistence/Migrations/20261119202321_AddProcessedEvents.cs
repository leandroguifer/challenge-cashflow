using System;
using Cashflow.Core.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cashflow.Balance.Infrastructure.Persistence.Migrations
{
    public partial class AddProcessedEvents : Migration
    {
        private const string DailyBalances = "daily_balances";
        private const string ProcessedEvents = "processed_events";
        private const string AppRole = "balance_app";
        private const string ConsumerRole = "balance_consumer";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "processed_events",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_events", x => x.event_id);
                });

            migrationBuilder.Sql(RslPolicySql.CreateStoresFunction);

            migrationBuilder.Sql($"GRANT SELECT ON {DailyBalances} TO {AppRole};");
            migrationBuilder.Sql($"GRANT SELECT, INSERT, UPDATE ON {DailyBalances} TO {ConsumerRole};");
            migrationBuilder.Sql($"GRANT SELECT, INSERT ON {ProcessedEvents} TO {ConsumerRole};");

            migrationBuilder.Sql(RslPolicySql.Enable(DailyBalances));
            migrationBuilder.Sql(RslPolicySql.StoreScopedPolicy(DailyBalances, AppRole));
            migrationBuilder.Sql(RslPolicySql.UnrestrictedPolicy(DailyBalances, ConsumerRole));

            migrationBuilder.Sql(RslPolicySql.Enable(ProcessedEvents));
            migrationBuilder.Sql(RslPolicySql.UnrestrictedPolicy(ProcessedEvents, ConsumerRole));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RslPolicySql.Disable(ProcessedEvents, ConsumerRole));
            migrationBuilder.Sql(RslPolicySql.Disable(DailyBalances, AppRole));
            migrationBuilder.Sql(RslPolicySql.Disable(DailyBalances, ConsumerRole));
            migrationBuilder.Sql($"REVOKE ALL ON {DailyBalances}, {ProcessedEvents} FROM {AppRole}, {ConsumerRole};");
            migrationBuilder.Sql(RslPolicySql.DropStoresFunction);

            migrationBuilder.DropTable(
                name: "processed_events");
        }
    }
}
