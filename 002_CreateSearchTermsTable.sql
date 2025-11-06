CREATE TABLE SearchTerms (
    Id UUID PRIMARY KEY,
    Term VARCHAR(200) NOT NULL UNIQUE,
    SearchCount INT NOT NULL DEFAULT 0,
    LastSearchedAt TIMESTAMP NOT NULL
);

-- Create index
CREATE INDEX idx_searchterms_term ON SearchTerms(Term);