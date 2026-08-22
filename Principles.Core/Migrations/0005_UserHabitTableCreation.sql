PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS UserHabit (
    LocalId INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    LastModified TEXT NULL,
    Id INTEGER NULL,
    Name TEXT NOT NULL,
    Type INTEGER NOT NULL,
    Status INTEGER NOT NULL DEFAULT 0,
    Priority INTEGER NOT NULL DEFAULT 0,
    IsArchived INTEGER NOT NULL DEFAULT 0,
    ArchivingTime TEXT NULL,
    Description TEXT NULL,
    ColorName TEXT NULL,
    GoalLocalId INTEGER NULL,
    Complexity INTEGER NOT NULL,
    FrequencyLocalId INTEGER NOT NULL,
        FOREIGN KEY (GoalLocalId) REFERENCES UserGoal (LocalId) ON DELETE SET NULL,
        FOREIGN KEY (FrequencyLocalId) REFERENCES FrequencyOfHabit (LocalId) ON DELETE RESTRICT ON UPDATE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_UserHabit_GoalLocalId ON UserHabit (GoalLocalId);
CREATE INDEX IF NOT EXISTS IX_UserHabit_FrequencyLocalId ON UserHabit (FrequencyLocalId);
