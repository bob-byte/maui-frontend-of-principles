CREATE TABLE IF NOT EXISTS User (
    LocalId INTEGER PRIMARY KEY AUTOINCREMENT,
    Id INTEGER,
    Name TEXT,
    MainSlogan TEXT,
    Mission TEXT,
    Email TEXT,
    Gender INTEGER,
    LastModified INTEGER
);