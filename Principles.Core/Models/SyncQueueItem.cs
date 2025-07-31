namespace Principles.Core.Models;

public class SyncQueueItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string EntityType { get; set; }           // Наприклад, "Habit", "HabitCompletion"
    public string Operation { get; set; }            // Наприклад, "Create", "Update", "Delete"
    public string PayloadJson { get; set; }          // Сериалізований об'єкт
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsProcessing { get; set; } = false;  // Щоб уникнути повторної обробки під час sync
}
