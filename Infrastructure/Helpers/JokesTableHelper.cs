using Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Helpers
{
    internal class JokesTableHelper
    {
        private readonly string _jokesTable;
        private readonly string _searchTermsTable;

        public JokesTableHelper(IConfiguration configuration)
        {
            _jokesTable = configuration["JokesTable"] ?? "Jokes";
            _searchTermsTable = configuration["SearchTermsTable"] ?? "SearchTerms";
        }

        public string SearchJokesSql()
        {
            return @$"
                SELECT Id, JokeId, JokeText, WordCount, JokeLength, CreatedAt, LastAccessedAt
                FROM {_jokesTable}
                WHERE to_tsvector('english', JokeText) @@ plainto_tsquery('english', @SearchTerm)
                ORDER BY LastAccessedAt
                LIMIT @Limit";
        }
        public string CheckJokeExistsSql()
        {
            return $"SELECT COUNT(1) FROM {_jokesTable} WHERE JokeId = @JokeId";
        }

        public string UpdateLastAccessedSql()
        {
            return $"UPDATE {_jokesTable} SET LastAccessedAt = @LastAccessedAt WHERE JokeId = @JokeId";
        }

        public string InsertJokeSql()
        {
            return $@"
                INSERT INTO {_jokesTable} (Id, JokeId, JokeText, WordCount, JokeLength, CreatedAt, LastAccessedAt)
                VALUES (@Id, @JokeId, @JokeText, @WordCount, @JokeLength, @CreatedAt, @LastAccessedAt)";
        }

        public string CheckExistingJokeIdsSql(List<Joke> jokes)
        {
            var ids = string.Join(",", jokes.Select(j => $"'{j.JokeId}'"));
            return $"SELECT JokeId FROM {_jokesTable} WHERE JokeId IN ({ids})";
        }

        public string BulkUpdateLastAccessedSql(List<Joke> jokes)
        {
            var ids = string.Join(",", jokes.Select(j => $"'{j.JokeId}'"));
            return $"UPDATE {_jokesTable} SET LastAccessedAt = @LastAccessedAt WHERE JokeId IN ({ids})";
        }

        public string UpdateLastAccessedByIdsSql(List<Guid> ids)
        {
            var idsList = string.Join(",", ids.Select(id => $"'{id}'"));
            return $"UPDATE {_jokesTable} SET LastAccessedAt = @LastAccessedAt WHERE Id IN ({idsList})";
        }

        // SearchTerms queries
        public string CheckSearchTermExistsSql()
        {
            return $"SELECT Id, SearchCount FROM {_searchTermsTable} WHERE LOWER(Term) = LOWER(@Term)";
        }

        public string UpdateSearchTermSql()
        {
            return $@"
                UPDATE {_searchTermsTable} 
                SET SearchCount = @SearchCount, LastSearchedAt = @LastSearchedAt 
                WHERE Id = @Id";
        }

        public string InsertSearchTermSql()
        {
            return $@"
                INSERT INTO {_searchTermsTable} (Id, Term, SearchCount, LastSearchedAt)
                VALUES (@Id, @Term, @SearchCount, @LastSearchedAt)";
        }

    }
}
