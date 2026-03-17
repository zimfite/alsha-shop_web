using System.Data;
using Dapper;
using Npgsql;

namespace WebApplication1.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Строка подключения не найдена");
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
        {
            using IDbConnection db = new NpgsqlConnection(_connectionString);
            return await db.QueryAsync<T>(sql, parameters);
        }

        public async Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null)
        {
            using IDbConnection db = new NpgsqlConnection(_connectionString);
            return await db.QuerySingleOrDefaultAsync<T>(sql, parameters);
        }

        public async Task<int> ExecuteAsync(string sql, object? parameters = null)
        {
            using IDbConnection db = new NpgsqlConnection(_connectionString);
            return await db.ExecuteAsync(sql, parameters);
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using IDbConnection db = new NpgsqlConnection(_connectionString);
                await db.ExecuteScalarAsync("SELECT 1");
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<int> GetCountAsync(string tableName)
        {
            using IDbConnection db = new NpgsqlConnection(_connectionString);
            return await db.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {tableName}");
        }
    }
}