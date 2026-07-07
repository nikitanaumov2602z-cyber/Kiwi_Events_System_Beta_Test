using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using System.Linq;

public class EventHandlers
{
    private readonly Plugin _plugin;

    public EventHandlers(Plugin plugin)
    {
        _plugin = plugin;
    }

    public void OnPlayerJoined(JoinedEventArgs ev)
    {
        if (_plugin.Config.LogToConsole) Log.Info($"{ev.Player.Nickname} подключился.");
        if (_plugin.Config.BroadcastEvents) ev.Player.Broadcast(3, "Добро пожаловать!");
    }

    public void OnPlayerDied(DiedEventArgs ev)
    {
        if (_plugin.Config.LogToConsole) Log.Info($"{ev.Player.Nickname} убит {ev.Killer?.Nickname ?? "окружением"}.");
        if (_plugin.Config.BroadcastEvents) Map.Broadcast(5, $"<color=red>{ev.Player.Nickname}</color> погиб.");
    }

    public void OnRoundStarted()
    {
        if (_plugin.Config.LogToConsole) Log.Info("Раунд начался.");
        if (_plugin.Config.BroadcastEvents) Map.Broadcast(10, "Раунд начался!");
    }

    public void OnRoundEnded(RoundEndedEventArgs ev)
    {
        if (_plugin.Config.LogToConsole) Log.Info($"Раунд завершён. Победитель: {ev.LeadingTeam}.");
        if (_plugin.Config.BroadcastEvents) Map.Broadcast(10, $"Победила команда {ev.LeadingTeam}.");
    }

    public void OnPlayerRoleChanged(ChangedRoleEventArgs ev)
    {
        if (_plugin.Config.LogToConsole) Log.Info($"{ev.Player.Nickname} стал {ev.NewRole}.");
    }

    public void OnSendingChatMessage(SendingChatMessageEventArgs ev)
    {
        if (ev.Message.StartsWith("."))
        {
            ev.IsAllowed = false;
            string[] parts = ev.Message.Substring(1).Split(' ');
            string command = parts[0].ToLower();
            string[] args = parts.Length > 1 ? parts.Skip(1).ToArray() : new string[0];

            if (command == "inspect" && _plugin.Config.EnableInspectCommand)
                _plugin.InspectCommand?.Execute(ev.Player, args);
            else if (command == "help" && _plugin.Config.EnableHelpCommand)
                _plugin.HelpCommand?.Execute(ev.Player, args);
            else
                ev.Player.SendConsoleMessage($"Неизвестная команда: .{command}", "red");
        }
    }
}
