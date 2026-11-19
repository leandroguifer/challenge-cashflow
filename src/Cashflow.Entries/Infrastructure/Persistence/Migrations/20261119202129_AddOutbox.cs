using System;
using Cashflow.Core.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cashflow.Entries.Infrastructure.Persistence.Migrations
{
    public partial class AddOutbox : Migration
    {
        private const string Table = "outbox_messages";
        private const string AppRole = "entries_app";
        private const string PublisherRole = "entries_publisher";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    trace_parent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_pending",
                table: "outbox_messages",
                column: "created_at",
                filter: "published_at is null");

            migrationBuilder.Sql($"GRANT INSERT ON {Table} TO {AppRole};");
            migrationBuilder.Sql($"GRANT SELECT, UPDATE ON {Table} TO {PublisherRole};");
            migrationBuilder.Sql(RslPolicySql.Enable(Table));
            migrationBuilder.Sql(RslPolicySql.UnrestrictedPolicy(Table, AppRole));
            migrationBuilder.Sql(RslPolicySql.UnrestrictedPolicy(Table, PublisherRole));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RslPolicySql.Disable(Table, AppRole));
            migrationBuilder.Sql(RslPolicySql.Disable(Table, PublisherRole));
            migrationBuilder.Sql($"REVOKE ALL ON {Table} FROM {AppRole}, {PublisherRole};");

            migrationBuilder.DropTable(
                name: "outbox_messages");
        }
    }
}
