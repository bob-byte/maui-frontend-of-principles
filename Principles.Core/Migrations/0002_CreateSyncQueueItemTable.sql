CREATE TABLE IF NOT EXISTS SyncQueueItem (
    LocalId INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    EntityId INTEGER NULL,
    EntityLocalId INTEGER NULL,
    HandlerType TEXT NOT NULL,
    Operation TEXT NOT NULL,
    PayloadJson TEXT NULL,
    LastModified INTEGER NULL,
    IsProcessing INTEGER DEFAULT 0,
    IsProcessed INTEGER DEFAULT 0,
    RetryCount INTEGER DEFAULT 0,
    NextRetryAt INTEGER NULL,
    LastRetryAt INTEGER NULL,
    ErrorMessage TEXT NULL,
    ProcessedAt INTEGER NULL,
    IsFailed INTEGER DEFAULT 0
);

CREATE INDEX IF NOT EXISTS IX_SyncQueueItem_EntityLocalId ON SyncQueueItem (EntityLocalId);
CREATE INDEX IF NOT EXISTS IX_SyncQueueItem_IsProcessed ON SyncQueueItem (IsProcessed);
CREATE INDEX IF NOT EXISTS IX_SyncQueueItem_IsFailed ON SyncQueueItem (IsFailed);