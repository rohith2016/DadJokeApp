# Dad Joke API

A .NET-based API service that fetches, caches, and serves dad jokes from [icanhazdadjoke.com](https://icanhazdadjoke.com). Jokes are categorized by length, searchable, and persisted in PostgreSQL for offline access.

## Features

- **Random Jokes**: Fetch random dad jokes from the external API
- **Search**: Find jokes by keyword with term highlighting
- **Smart Caching**: DB-first strategy - searches prioritize cached jokes, falling back to API
- **Search Analytics**: Find out which search terms are trending and how frequently they are being searched.
- **Length Categorization**: Jokes grouped as Short (<10 words), Medium (<20 words), Long (≥20 words)
- **Persistence**: PostgreSQL storage with full text search capabilities
- **RateLimiting**: Usage of Polly to handle rate limiting for any api.
- **Production Patterns**: Repository pattern, service layer, DTOs, dependency injection

## Tech Stack

- .NET 8.0
- ASP.NET Core Web API
- PostgreSQL 18+

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL 18+ (local or Docker)
- Any REST client (Postman, curl, browser). An angular app that connects with this server.

## Database Setup

### PostgreSQL Installation

**macOS (Homebrew)**
```bash
brew install postgresql@16
brew services start postgresql@16
```

**Windows**
Download from [postgresql.org](https://www.postgresql.org/download/windows/)

**Docker**
```bash
docker run --name postgres-dadjoke -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:16
```

### Create Database

```bash
psql -U postgres
CREATE DATABASE dadjokes;
\q
```


### Create Tables
Refer to Database folder for the scripts

## Configuration

Update `appsettings.json` with your PostgreSQL credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=dadjokes;Username=postgres;Password=yourpassword"
  }
}
```

## Installation & Running

### Restore Dependencies
```bash
dotnet restore
```


```

### Run the Application
```bash
dotnet run
```

**Default Port**: `https://localhost:7001` (HTTPS) or `http://localhost:5001` (HTTP)

Check `Properties/launchSettings.json` to customize ports.

### Swagger UI

Navigate to: `https://localhost:7001/swagger`

## API Endpoints

### GET `/api/jokes/random`
Fetches a random dad joke.

**Response**
```json
{
  "id": "abc123",
  "joke": "Why don't scientists trust atoms? Because they make up everything!",
  "length": "Medium"
}
```

### GET `/api/jokes/search?term=cat&saveToDb=true`
Searches for jokes containing the term. Prioritizes DB, then falls back to external API.

**Parameters**
- `term` (required): Search keyword
- `saveToDb` (optional, default=true): Whether to persist results

**Response**
```json
{
  "totalJokes": 2,
  "classifiedJokes": [
    {
      "lengthCategory": "Medium",
      "count": 2,
      "jokes": [
        {
          "id": "8UnrHe2T0g",
          "text": "‘Put the <b>cat</b> out’ … ‘I didn’t realize it was on fire",
          "length": "Medium"
        },
        {
          "id": "daaUfibh",
          "text": "Why was the big <b>cat</b> disqualified from the race? Because it was a cheetah.",
          "length": "Medium"
        }
      ]
    }
  ]
```

Search terms are **emphasized in bold** in the response.

## Project Structure

```
DadJokeApp/
│
├── DadJokeApp.Api/                          # Presentation Layer
│   ├── Controllers/
│   │   └── JokesController.cs               # API endpoints
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs   # Global error handling
│   ├── Program.cs                           # Application entry point
│   ├── appsettings.json                     # Configuration
│   └── DadJokeApp.Api.csproj
│
├── DadJokeApp.Application/                  # Application Layer
│   ├── DTOs/
│   ├── Interfaces/
│   │   ├── IJokeService.cs                  # Service contracts
│   │   └── IJokeSearchService.cs
│   ├── Services/
│   │   ├── JokeService.cs                   # Business logic
│   │   └── JokeSearchService.cs             # Search orchestration
│   ├── DependencyInjection.cs               # Service registration
│   └── DadJokeApp.Application.csproj
│
├── DadJokeApp.Domain/                       # Domain Layer
│   ├── Entities/
│   ├── Enums/
│   │   └── JokeLength.cs                    # Short/Medium/Long
│   ├── Models/
│   ├── Interfaces/
│   │   ├── IJokeRepository.cs               # Repository contracts
│   │   ├── IJokeApiClient.cs                # External API contract
│   │   ├── IJokeLengthClassifier.cs         # Domain service
│   │   └── IJokeHighlighter.cs              # Domain service
│   ├── Services/
│   │   ├── JokeLengthClassifier.cs          # Word count logic
│   │   └── JokeHighlighter.cs               # Term emphasis logic
│   └── DadJokeApp.Domain.csproj
│
├── DadJokeApp.Infrastructure/               # Infrastructure Layer
│   ├── Repositories/
│   │   └── JokeRepository.cs                # EF Core data access
│   ├── ExternalServices/
│   │   └── DadJokeApiClient.cs              # HTTP client wrapper
│   ├── DependencyInjection.cs               # Infrastructure registration
│   └── DadJokeApp.Infrastructure.csproj
│
├── DadJokeApp.Tests/
│   ├── DadJokeApp.UnitTests/
│       ├── Domain/
│       │   ├── JokeLengthClassifierTests.cs
│       │   └── JokeHighlighterTests.cs
│       ├── Application/
│       │   └── JokeSearchServiceTests.cs
│       └── DadJokeApp.UnitTests.csproj
└── DadJokeApp.sln
```

## Design Patterns

- **Repository Pattern**: Abstracts data access (`IJokeRepository`)
- **Service Layer**: Encapsulates business rules (`JokeService`)
- **DTOs**: Separate API models from database entities
- **Dependency Injection**: Constructor injection for testability
- **Factory Pattern**: Joke length categorization logic

## Testing

Run unit tests:
```bash
dotnet test
```

**Test Coverage**
- Service layer logic (mocked repositories)
- Length categorization
- Search term highlighting
- Database query validation

## Performance Considerations

- DB connection pooling
- Asynchronous I/O throughout (`async/await`)
- GIN indexes for text search in PostgreSQL
- External API rate limiting handled by cache-first strategy and maxRequest limit

## Further Enhancements

### Short-Term
- [ ] Pagination for search results (currently limited to 30)
- [ ] Joke voting/favorites system
- [ ] Category tags (puns, knock-knock, etc.)

### long-Term
- [ ] Redis caching layer for frequently accessed jokes
- [ ] Background jobs using RabbitMQ/Kafka instead of a Tasks
- [ ] User accounts
- [ ] Joke recommendation engine based on user history
- [ ] Improved analytics dashboard
- [ ] Microservices architecture (separate joke fetcher service)

## Troubleshooting

**Port Already in Use**
```bash
# Find process on port 7001
lsof -i :7001
kill -9 <PID>
```

```

**Connection Refused**
- Verify PostgreSQL is running: `pg_isready`
- Check connection string in `appsettings.json`
- Ensure firewall allows port 5432

## License

MIT

## Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/joke-categories`)
3. Commit changes (`git commit -am 'Add joke categories'`)
4. Push to branch (`git push origin feature/joke-categories`)
5. Open Pull Request

## Contact

For issues or questions, open a GitHub issue or contact the maintainers.

---

**Built with ☕ and dad jokes**