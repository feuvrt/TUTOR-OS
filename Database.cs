using Npgsql;

namespace TutorOS;
public static class Database
{
    public static NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(DatabaseConfig.ConnectionString);
    }
}