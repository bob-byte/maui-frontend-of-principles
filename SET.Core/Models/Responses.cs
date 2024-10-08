namespace SET.Core.Models;

public record LoginResponse( string Token );
public record GoogleAuthResponse( string Token );
public record RecomendedHabitsResponse( List<RecommendedHabit> Habits );
