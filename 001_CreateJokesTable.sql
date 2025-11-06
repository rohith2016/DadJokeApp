CREATE TABLE Jokes (
    Id UUID PRIMARY KEY,
    JokeId VARCHAR(50) NOT NULL UNIQUE,
    JokeText TEXT NOT NULL,
    WordCount INT NOT NULL,
    JokeLength VARCHAR(20) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL,
    LastAccessedAt TIMESTAMP NOT NULL
);

-- Create indexes
CREATE INDEX idx_jokes_jokelength ON Jokes(JokeLength);
CREATE INDEX idx_jokes_jokeid ON Jokes(JokeId);
CREATE INDEX idx_jokes_lastaccessedat ON Jokes(LastAccessedAt);

-- Create full-text search index
CREATE INDEX idx_jokes_fulltext ON Jokes USING GIN (to_tsvector('english', JokeText));