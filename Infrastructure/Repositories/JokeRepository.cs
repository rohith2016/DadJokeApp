using Domain.Entities;
using Domain.Enum;
using Domain.Helpers;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Infrastructure.Repositories
{
    internal class JokeRepository : IJokeRepository
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;
        private readonly JokesTableHelper _sqlHelper;
        public JokeRepository(IConfiguration config, JokesTableHelper helper)
        { 
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection") ?? "";
            _sqlHelper = helper;
        }

        public async Task<List<Joke>> SearchJokesAsync(string searchTerm, int limit)
        {
            var jokes = new List<Joke>();

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = _sqlHelper.SearchJokesSql();

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@SearchTerm", searchTerm);
            command.Parameters.AddWithValue("@Limit", limit);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                jokes.Add(MapToJoke(reader));
            }

            await reader.CloseAsync();

            if (jokes.Any())
            {
                await UpdateLastAccessedAsync(connection, jokes.Select(j => j.Id).ToList());
            }

            return jokes;
        }

        public async Task SaveJokeAsync(Joke joke)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var checkSql = _sqlHelper.CheckJokeExistsSql();
            using var checkCommand = new NpgsqlCommand(checkSql, connection);
            checkCommand.Parameters.AddWithValue("@JokeId", joke.JokeId);

            var exists = (long)(await checkCommand.ExecuteScalarAsync() ?? 0) > 0;

            if (exists)
            {
                var updateSql = _sqlHelper.UpdateLastAccessedSql();
                using var updateCommand = new NpgsqlCommand(updateSql, connection);
                updateCommand.Parameters.AddWithValue("@LastAccessedAt", DateTime.UtcNow);
                updateCommand.Parameters.AddWithValue("@JokeId", joke.JokeId);
                await updateCommand.ExecuteNonQueryAsync();
            }
            else
            {
                var insertSql = _sqlHelper.InsertJokeSql();

                using var insertCommand = new NpgsqlCommand(insertSql, connection);
                AddJokeParameters(insertCommand, joke);
                await insertCommand.ExecuteNonQueryAsync();
            }
        }

        public async Task SaveJokesBatchAsync(List<Joke> jokes, string searchTerm)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var checkSql = _sqlHelper.CheckExistingJokeIdsSql(jokes);

                var existingJokeIds = new HashSet<string>();
                using (var checkCommand = new NpgsqlCommand(checkSql, connection, transaction))
                {
                    using var reader = await checkCommand.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        existingJokeIds.Add(reader.GetString(0));
                    }
                }

                var newJokes = jokes.Where(j => !existingJokeIds.Contains(j.JokeId)).ToList();

                if (newJokes.Any())
                {
                    var insertSql = _sqlHelper.InsertJokeSql();

                    foreach (var joke in newJokes)
                    {
                        using var insertCommand = new NpgsqlCommand(insertSql, connection, transaction);
                        AddJokeParameters(insertCommand, joke);
                        await insertCommand.ExecuteNonQueryAsync();
                    }
                }

                if (existingJokeIds.Any())
                {
                    var updateSql = _sqlHelper.BulkUpdateLastAccessedSql(jokes);

                    using var updateCommand = new NpgsqlCommand(updateSql, connection, transaction);
                    updateCommand.Parameters.AddWithValue("@LastAccessedAt", DateTime.UtcNow);
                    await updateCommand.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task TrackSearchTermAsync(string searchTerm)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();
            await TrackSearchTermInternalAsync(connection, transaction, searchTerm);
            await transaction.CommitAsync();
        }

        private async Task TrackSearchTermInternalAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            string searchTerm)
        {
            var checkSql = "SELECT Id, SearchCount FROM SearchTerms WHERE LOWER(Term) = LOWER(@Term)";
            using var checkCommand = new NpgsqlCommand(checkSql, connection, transaction);
            checkCommand.Parameters.AddWithValue("@Term", searchTerm);

            Guid? termId = null;
            int currentCount = 0;

            using (var reader = await checkCommand.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    termId = reader.GetGuid(0);
                    currentCount = reader.GetInt32(1);
                }
            }

            if (termId.HasValue)
            {
                var updateSql = @"
                    UPDATE SearchTerms 
                    SET SearchCount = @SearchCount, LastSearchedAt = @LastSearchedAt 
                    WHERE Id = @Id";

                using var updateCommand = new NpgsqlCommand(updateSql, connection, transaction);
                updateCommand.Parameters.AddWithValue("@SearchCount", currentCount + 1);
                updateCommand.Parameters.AddWithValue("@LastSearchedAt", DateTime.UtcNow);
                updateCommand.Parameters.AddWithValue("@Id", termId.Value);
                await updateCommand.ExecuteNonQueryAsync();
            }
            else
            {
                var insertSql = @"
                    INSERT INTO SearchTerms (Id, Term, SearchCount, LastSearchedAt)
                    VALUES (@Id, @Term, @SearchCount, @LastSearchedAt)";

                using var insertCommand = new NpgsqlCommand(insertSql, connection, transaction);
                insertCommand.Parameters.AddWithValue("@Id", Guid.NewGuid());
                insertCommand.Parameters.AddWithValue("@Term", searchTerm.ToLower());
                insertCommand.Parameters.AddWithValue("@SearchCount", 1);
                insertCommand.Parameters.AddWithValue("@LastSearchedAt", DateTime.UtcNow);
                await insertCommand.ExecuteNonQueryAsync();
            }
        }

        private async Task UpdateLastAccessedAsync(NpgsqlConnection connection, List<Guid> jokeIds)
        {
            if (jokeIds == null || jokeIds.Count == 0)
                return;

            var sql =_sqlHelper.UpdateLastAccessedByIdsSql(jokeIds);

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@LastAccessedAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("@Ids", jokeIds);

            await command.ExecuteNonQueryAsync();
        }


        private void AddJokeParameters(NpgsqlCommand command, Joke joke)
        {
            command.Parameters.AddWithValue("@Id", joke.Id);
            command.Parameters.AddWithValue("@JokeId", joke.JokeId);
            command.Parameters.AddWithValue("@JokeText", joke.JokeText);
            command.Parameters.AddWithValue("@WordCount", joke.WordCount);
            command.Parameters.AddWithValue("@JokeLength", joke.JokeLength.ToString());
            command.Parameters.AddWithValue("@CreatedAt", joke.CreatedAt);
            command.Parameters.AddWithValue("@LastAccessedAt", joke.LastAccessedAt);
        }

        private static Joke MapToJoke(NpgsqlDataReader reader)
        {
            return new Joke
            {
                Id = reader.GetGuid(0),
                JokeId = reader.GetString(1),
                JokeText = reader.GetString(2),
                WordCount = reader.GetInt32(3),
                JokeLength = Enum.Parse<JokeLength>(reader.GetString(4)),
                CreatedAt = reader.GetDateTime(5),
                LastAccessedAt = reader.GetDateTime(6)
            };
        }
    }
}