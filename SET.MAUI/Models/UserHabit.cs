using System;
namespace SET.MAUI.Models;

public class MauiUserHabit : Core.Models.UserHabit
{
    public Color Color =>
        Color.FromArgb( ColorName );
}

