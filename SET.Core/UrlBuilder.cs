using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core;

public class UrlBuilder : IUrlBuilder
{
    private string? m_baseUrl;
    private string? m_baseApiUrl;
    private string? m_login;
    private string? m_account;
    private string? m_signUp;
    private string? m_profile;
    private string? m_userName;
    private string? m_userMainSlogan;
    private string? m_userMission;
    private string? m_habitsInProgress;
    private string? m_areasOfLife;
    private string? m_progressOfHabit;
    private string? m_habits;
    private string? m_habitsPriorities;
    private string? m_logs;
    private string? m_changePassword;
    private string? m_codeGeneration;
    private string? m_goal;

    public string BaseUrl
    {
        get
        {
            m_baseUrl ??= "https://principles-server.ckwavh.easypanel.host/";
            return m_baseUrl;
        }
    }

    public string BaseApiUrl
    {
        get
        {
            m_baseApiUrl ??= Combine( BaseUrl, "api" );
            return m_baseApiUrl;
        }
    }

    public string ProgressOfHabit
    {
        get
        {
            m_progressOfHabit ??= Combine( BaseApiUrl, "progressesofhabit" );
            return m_progressOfHabit;
        }
    }

    public string Habits
    {
        get
        {
            m_habits ??= Combine( BaseApiUrl, "habits" );
            return m_habits;
        }
    }

    public string HabitsInProgress
    {
        get
        {
            m_habitsInProgress ??= Combine( BaseApiUrl, "habits", "inprogress" );
            return m_habitsInProgress;
        }
    }

    public string HabitsPriorities
    {
        get
        {
            m_habitsPriorities ??= Combine( BaseApiUrl, "habits", "priorities" );
            return m_habitsPriorities;
        }
    }

    public string AreasOfLife
    {
        get
        {
            m_areasOfLife ??= Combine( BaseApiUrl, "areasoflife" );
            return m_areasOfLife;
        }
    }

    public string Login
    {
        get
        {
            m_login ??= Combine( BaseApiUrl, "account", "authorization" );
            return m_login;
        }
    }

    public string SignUp
    {
        get
        {
            m_signUp ??= Combine( BaseApiUrl, "account", "authentication" );
            return m_signUp;
        }
    }

    public string Password
    {
        get
        {
            m_changePassword ??= Combine( BaseApiUrl, "account", "password" );
            return m_changePassword;
        }
    }

    public string CodeGeneration
    {
        get
        {
            m_codeGeneration ??= Combine( BaseApiUrl, "account", "code" );
            return m_codeGeneration;
        }
    }
    public string Account
    {
        get
        {
            m_account ??= Combine( BaseApiUrl, "account" );
            return m_account;
        }
    }

    public string Profile
    {
        get
        {
            m_profile ??= Combine( BaseApiUrl, "profile" );
            return m_profile;
        }
    }

    public string UserName
    {
        get
        {
            m_userName ??= Combine( BaseApiUrl, "profile", "name" );
            return m_userName;
        }
    }

    public string UserMainSlogan
    {
        get
        {
            m_userMainSlogan ??= Combine( BaseApiUrl, "profile", "mainslogan" );
            return m_userMainSlogan;
        }
    }

    public string UserMission
    {
        get
        {
            m_userMission ??= Combine( BaseApiUrl, "profile", "mission" );
            return m_userMission;
        }
    }
    public string Goal
    {
        get
        {
            m_userMission ??= Combine( BaseApiUrl, "goals" );
            return m_userMission;
        }
    }


    public string Logs
    {
        get
        {
            m_logs ??= Combine( BaseApiUrl, "logs" );
            return m_logs;
        }
    }

    public string Combine( params string[] uri )
    {
        uri[0] = uri[0].TrimEnd( '/' );

        var bld = new StringBuilder();
        bld.Append( uri[0] + "/" );

        for (int i = 1; i < uri.Length; i++)
        {
            if (uri[i] == null)
            {
                continue;
            }

            uri[i] = uri[i].TrimStart( '/' ).TrimEnd( '/' );
            if (uri[i] == "")
            {
                continue;
            }

            bld.Append( uri[i] + "/" );
        }

        bld.Remove( startIndex: bld.Length - 1, length: 1 );
        return bld.ToString();
    }
}
