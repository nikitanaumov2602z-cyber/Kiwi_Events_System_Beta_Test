using Exiled.API.Features;
using System.Linq;
using UnityEngine;

public class InspectCommand
{
    private readonly Plugin _plugin;

    public InspectCommand(Plugin plugin) => _plugin = plugin;

    public void Execute(Player caller, string[] args)
    {
        if (args.Length == 0)
        {
            caller.SendConsoleMessage("Использование: .inspect <игрок>", "red");
            return;
        }

        string targetName = string.Join(" ", args);
        Player target = Player.Get(targetName);
        if (target == null)
        {
            caller.SendConsoleMessage($"Игрок '{targetName}' не найден.", "red");
            return;
        }

        string info = $"=== Инспекция игрока {target.Nickname} ===\n";
        if (_plugin.Config.InspectShowRole)
            info += $"Роль: {target.Role.Type}\n";
        if (_plugin.Config.InspectShowHealth)
            info += $"Здоровье: {target.Health}/{target.MaxHealth}\n";
        if (_plugin.Config.InspectShowItems)
        {
            var items = target.Items.Select(i => i.Type.ToString());
            info += $"Предметы: {(items.Any() ? string.Join(", ", items) : "нет")}\n";
        }
        if (_plugin.Config.InspectShowLocation)
        {
            Vector3 pos = target.Position;
            info += $"Позиция: X={pos.x:F1} Y={pos.y:F1} Z={pos.z:F1}\n";
        }

        caller.SendConsoleMessage(info, "white");
        caller.ShowHint(info, 5f);
    }
}

public class HelpCommand
{
    private readonly Plugin _plugin;

    public HelpCommand(Plugin plugin) => _plugin = plugin;

    public void Execute(Player caller, string[] args)
    {
        string helpText = "=== Доступные команды ===\n" +
                          ".inspect <игрок> - осмотреть игрока (RP-инфа)\n" +
                          ".help - показать эту справку\n" +
                          "(RA команды: ev, ci, disabletesla, disableescape, audioplayer)";
        caller.SendConsoleMessage(helpText, "cyan");
        caller.ShowHint(helpText, 5f);
    }
}
