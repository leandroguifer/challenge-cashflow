using Cashflow.Core.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cashflow.Entries.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GrantAppAccess : Migration
    {
        private const string AppRole = "entries_app";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RslPolicySql.CreateStoresFunction);

            migrationBuilder.Sql($"GRANT SELECT, INSERT, UPDATE ON cash_registers TO {AppRole};");
            migrationBuilder.Sql($"GRANT SELECT, INSERT ON entries TO {AppRole};");

            foreach (var table in new[] { "cash_registers", "entries" })
            {
                migrationBuilder.Sql(RslPolicySql.Enable(table));
                migrationBuilder.Sql(RslPolicySql.StoreScopedPolicy(table, AppRole));
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var table in new[] { "cash_registers", "entries" })
            {
                migrationBuilder.Sql(RslPolicySql.Disable(table, AppRole));
            }

            migrationBuilder.Sql($"REVOKE ALL ON cash_registers, entries FROM {AppRole};");
            migrationBuilder.Sql(RslPolicySql.DropStoresFunction);
        }
    }
}
