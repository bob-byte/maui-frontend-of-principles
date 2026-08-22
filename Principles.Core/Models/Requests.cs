using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Models;

public record SignUpRequest( string name, string email, string password, int gender, string mainSlogan, string mission );
public record LoginRequest( string email, string password );
public record SaveUserNameRequest( string UserName, DateTime LastModified = default );
public record SaveMainSloganRequest( string MainSlogan, DateTime LastModified = default );
public record SaveMissionRequest( string Mission, DateTime LastModified = default );
public record SaveLogRequest( string DeviceOs, string DeviceModelName, string DeviceType, string DeviceManufacturer, string AppVersion, string LogType, string LogMessage, string? StackTrace );
