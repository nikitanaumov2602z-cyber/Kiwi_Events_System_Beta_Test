using Exiled.API.Interfaces;
using System.Collections.Generic;

public class Config : IConfig
{
    public bool IsEnabled { get; set; } = true;
    public bool LogToConsole { get; set; } = true;
    public bool BroadcastEvents { get; set; } = true;

    public bool EnableEventSystem { get; set; } = true;
    public bool EnableCustomItems { get; set; } = true;
    public bool EnableInspectCommand { get; set; } = true;
    public bool EnableHelpCommand { get; set; } = true;

    public float HudUpdateInterval { get; set; } = 1.0f;
    public string HudMessageFormat { get; set; } = "🎭 ИДЁТ ИВЕНТ\nПроводящий: {host}\nИвент: {name}\nУровень RP: {level}\nВремя: {time}";

    public bool InspectShowHealth { get; set; } = true;
    public bool InspectShowRole { get; set; } = true;
    public bool InspectShowItems { get; set; } = true;
    public bool InspectShowLocation { get; set; } = true;

    public Dictionary<string, GrandRoleData> GrandRoles { get; set; } = new Dictionary<string, GrandRoleData>();
    public Dictionary<string, RoleData> Roles { get; set; } = new Dictionary<string, RoleData>();
    public string DefaultGunGroup { get; set; } = "Epsilon-11";
}
