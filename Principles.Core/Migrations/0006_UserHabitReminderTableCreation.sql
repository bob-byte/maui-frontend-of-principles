CREATE TABLE IF NOT EXISTS UserHabitReminder (
    LocalId INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    Id INTEGER NULL,
    LastModified TEXT NULL,
    Title TEXT NOT NULL,
    Description TEXT NULL,
    Time TEXT NOT NULL,
    IsEnabled INTEGER NOT NULL,
    UserHabitLocalId INTEGER NOT NULL,
        FOREIGN KEY (UserHabitLocalId) REFERENCES UserHabit (LocalId) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_UserHabitReminder_UserHabitLocalId ON UserHabitReminder (UserHabitLocalId);