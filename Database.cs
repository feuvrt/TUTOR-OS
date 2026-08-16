using Npgsql;

namespace TutorOS;
public static class Database {
    private const string ConnectionString =
        "Host=localhost;Port=5432;Username=postgres;Password=postgres123;Database=tutor_db";

    public static NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(ConnectionString);
    }
}