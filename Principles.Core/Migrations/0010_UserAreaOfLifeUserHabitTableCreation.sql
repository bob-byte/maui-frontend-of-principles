CREATE TABLE IF NOT EXISTS UserAreaOfLifeUserHabit (
    LocalId INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    UserAreaOfLifeLocalId INTEGER NOT NULL,
    UserHabitLocalId INTEGER NOT NULL,
        FOREIGN KEY (UserAreaOfLifeLocalId) REFERENCES UserAreaOfLife (LocalId) ON DELETE CASCADE ON UPDATE CASCADE,
        FOREIGN KEY (UserHabitLocalId) REFERENCES UserHabit (LocalId) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_UserAreaOfLifeUserHabit_Area ON UserAreaOfLifeUserHabit (UserAreaOfLifeLocalId);
CREATE INDEX IF NOT EXISTS IX_UserAreaOfLifeUserHabit_Habit ON UserAreaOfLifeUserHabit (UserHabitLocalId);
