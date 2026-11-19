namespace Cashflow.Core.Data;

public static class RslPolicySql
{
    public const string CreateStoresFunction = """
        CREATE OR REPLACE FUNCTION app_stores() RETURNS text[]
        LANGUAGE sql STABLE AS $$
          SELECT string_to_array(NULLIF(current_setting('app.stores', true), ''), ',')
        $$;
        """;

    public const string DropStoresFunction = "DROP FUNCTION IF EXISTS app_stores();";

    public static string Enable(string table) => $"""
        ALTER TABLE {table} ENABLE ROW LEVEL SECURITY;
        ALTER TABLE {table} FORCE ROW LEVEL SECURITY;
        """;

    public static string StoreScopedPolicy(string table, string role) => $"""
        CREATE POLICY {role}_store_scope ON {table} FOR ALL TO {role}
          USING (store_id = ANY (app_stores()))
          WITH CHECK (store_id = ANY (app_stores()));
        """;

    public static string UnrestrictedPolicy(string table, string role) => $"""
        CREATE POLICY {role}_unrestricted ON {table} FOR ALL TO {role}
          USING (true) WITH CHECK (true);
        """;

    public static string Disable(string table, string role) => $"""
        DROP POLICY IF EXISTS {role}_store_scope ON {table};
        DROP POLICY IF EXISTS {role}_unrestricted ON {table};
        ALTER TABLE {table} NO FORCE ROW LEVEL SECURITY;
        ALTER TABLE {table} DISABLE ROW LEVEL SECURITY;
        """;
}