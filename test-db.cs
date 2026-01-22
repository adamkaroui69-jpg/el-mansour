using System;
using Npgsql;

try {
    string connString = "Host=db.kawppmcjxxbcosfyitfx.supabase.co;Port=5432;Database=postgres;User Id=postgres;Password=sykcYExRhdnrjgQk;SSL Mode=Require;Trust Server Certificate=true";
    Console.WriteLine($"Testing connection to: {connString}");
    using var conn = new NpgsqlConnection(connString);
    conn.Open();
    Console.WriteLine("SUCCESS: Connected to PostgreSQL!");
} catch (Exception ex) {
    Console.WriteLine($"FAILURE: {ex.Message}");
    if (ex.InnerException != null) Console.WriteLine($"Detail: {ex.InnerException.Message}");
}
