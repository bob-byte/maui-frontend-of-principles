using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Models;

public record SignUpResponse( string Message, string Token );
public record LoginResponse( string Message, string Token, long UserId );
public record GoogleAuthResponse( string Message, string Token, long UserId );
public record SaveMainSloganResponse( string Message );
public record RecomendedHabitsResponse(List<RecommendedHabit> Habits);
public record ResponseOfUpdateProgressPost( double PreviousPercentageAchieved, double PercentageAchived );
