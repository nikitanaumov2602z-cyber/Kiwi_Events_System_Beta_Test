using CommandSystem;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class EvCommand : ICommand
{
    public string Command => "ev";
    public string[] Aliases => new[] { "event" };
    public string Description => "Управление ивентами: ev pod | ev start [уровень] [название] | ev stop";

    private readonly Plugin _plugin;
    public EvCommand(Plugin plugin) => _plugin = plugin;

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count == 0)
        {
            response = "Использование: ev pod, ev start [уровень] [название], ev stop";
            return false;
        }

        string sub = arguments.At(0).ToLower();
        string host = Player.Get(sender)?.Nickname ?? "Сервер";

        if (sub == "pod")
        {
            _plugin.EventManager.Prepare(host);
            response = "Подготовка выполнена. Теперь можно запустить ev start.";
            return true;
        }
        else if (sub == "start")
        {
            if (!_plugin.EventManager.IsPrepared)
            {
                response = "Сначала подготовьте ивент через ev pod.";
                return false;
            }
            if (arguments.Count < 3)
            {
                response = "Укажите уровень и название: ev start [уровень] \"Название\"";
                return false;
            }
            string level = arguments.At(1);
            string name = string.Join(" ", arguments.Segment(2));
            bool started = _plugin.EventManager.StartEvent(name, level, host);
            if (started)
                response = $"Ивент '{name}' (уровень {level}) запущен проводящим {host}.";
            else
                response = "Не удалось запустить ивент. Проверьте уровень RP.";
            return started;
        }
        else if (sub == "stop")
        {
            _plugin.EventManager.StopEvent();
            response = "Ивент остановлен.";
            return true;
        }
        else
        {
            response = "Неизвестная подкоманда. Доступно: pod, start, stop.";
            return false;
        }
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class CiCommand : ICommand
{
    public string Command => "ci";
    public string[] Aliases => new[] { "customitem" };
    public string Description => "Управление предметами: ci list | ci give [ID] | ci remote";

    private readonly Plugin _plugin;
    public CiCommand(Plugin plugin) => _plugin = plugin;

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count == 0)
        {
            response = "Используйте: ci list, ci give [ID], ci remote";
            return false;
        }

        string sub = arguments.At(0).ToLower();
        switch (sub)
        {
            case "list":
                response = _plugin.CustomItemManager.GetItemList();
                return true;
            case "give":
                if (arguments.Count < 2)
                {
                    response = "Укажите ID предмета.";
                    return false;
                }
                if (!int.TryParse(arguments.At(1), out int id))
                {
                    response = "ID должен быть числом.";
                    return false;
                }
                var player = Player.Get(sender);
                if (player == null)
                {
                    response = "Команда только для игроков.";
                    return false;
                }
                _plugin.CustomItemManager.GiveItem(player, id);
                response = $"Выдан предмет {id}.";
                return true;
            case "remote":
                _plugin.ReloadConfig();
                _plugin.EventManager.StopEvent();
                response = "Сброс выполнен. Конфиг перезагружен.";
                return true;
            default:
                response = "Неизвестная команда. Доступно: list, give, remote.";
                return false;
        }
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class DisableTeslaCommand : ICommand
{
    public string Command => "disabletesla";
    public string[] Aliases => new[] { "dtesla" };
    public string Description => "Отключить/включить теслы.";

    private readonly Plugin _plugin;
    public DisableTeslaCommand(Plugin plugin) => _plugin = plugin;

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        _plugin.ToggleTesla();
        response = _plugin.TeslaDisabled ? "Теслы отключены." : "Теслы включены.";
        return true;
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class DisableEscapeCommand : ICommand
{
    public string Command => "disableescape";
    public string[] Aliases => new[] { "descape" };
    public string Description => "Отключить/включить побег.";

    private readonly Plugin _plugin;
    public DisableEscapeCommand(Plugin plugin) => _plugin = plugin;

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        _plugin.ToggleEscape();
        response = _plugin.EscapeDisabled ? "Побег отключён." : "Побег включён.";
        return true;
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class AudioPlayerCommand : ICommand
{
    public string Command => "audioplayer";
    public string[] Aliases => new[] { "audio" };
    public string Description => "Управление аудиоплеером: audioplayer play [ID] | audioplayer list";

    private readonly Dictionary<string, string> _soundList = new Dictionary<string, string>
    {
        { "1", "Прибытие МОГ Эпсилон-11 [Рус]" },
        { "2", "Прибытие МОГ Eta-10 [Рус]" },
        { "3", "SCP: Containment Breach (сирена/тревога) [Рус]" },
        { "4", "Протокол Мёртвая Рука [Рус]" },
        { "5", "Внимание! МОГ Эпсилон-11 была убита SCP [Рус]" },
        { "6", "Внимание, всем сотрудникам! Зафиксировано нарушение содержания [Рус]" },
        { "7", "Внимание! SCP-106 покинул зону содержания [Рус]" },
        { "8", "Внимание! SCP-682 проявляет признаки агрессии [Рус]" },
        { "9", "Всем сотрудникам! Немедленно проследовать в убежища [Рус]" },
        { "10", "Внимание! Зафиксирован неавторизованный доступ к системе [Рус]" },
        { "11", "МОГ Эпсилон-11 вошла в комплекс [Рус]" },
        { "12", "Внимание! В зоне Б зафиксировано присутствие SCP-106 [Рус]" },
        { "13", "Протокол Альфа-1 активирован [Рус]" },
        { "14", "Внимание! Обнаружена утечка биологически опасных материалов [Рус]" },
        { "15", "Прибытие МОГ Альфа-1 [Рус]" },
        { "16", "Внимание! МОГ Альфа-1 зачищает зону [Рус]" },
        { "17", "Прибытие МОГ Бета-7 [Рус]" },
        { "18", "Внимание! МОГ Бета-7 зачищает зону [Рус]" },
        { "19", "Прибытие МОГ Ню-7 [Рус]" },
        { "20", "Внимание! МОГ Ню-7 зачищает зону [Рус]" },
        { "21", "Обычная музыка из SCP: Containment Breach" },
        { "22", "Внимание! SCP-173 сбежал из зоны содержания [Рус]" },
        { "23", "Внимание! SCP-049 покинул зону содержания [Рус]" },
        { "24", "Внимание! SCP-096 сбежал из зоны содержания [Рус]" },
        { "25", "Внимание! Зафиксирована утечка SCP-008 [Рус]" },
        { "26", "Внимание! SCP-914 активирован [Рус]" },
        { "27", "Внимание! SCP-079 взломал систему безопасности [Рус]" },
        { "28", "Протокол Омега-7 активирован [Рус]" },
        { "29", "Внимание! Все невооружённые сотрудники должны немедленно покинуть зону [Рус]" },
        { "30", "Внимание! Требуется подкрепление в зоне Б [Рус]" },
        { "31", "День пиццы (объявление) [Рус]" },
        { "32", "Внимание! D-Класс захватил контроль [Рус]" },
        { "33", "Blue Feather (эмбиент из SCP: Containment Breach)" }
    };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count == 0)
        {
            response = "Использование: audioplayer play [ID] или audioplayer list";
            return false;
        }

        string sub = arguments.At(0).ToLower();
        switch (sub)
        {
            case "play":
                if (arguments.Count < 2)
                {
                    response = "Укажите ID звука: audioplayer play [ID]";
                    return false;
                }
                string id = arguments.At(1);
                try
                {
                    var args = new ArraySegment<string>(new[] { "play", id });
                    ICommand cmd = CommandProcessor.Commands.GetCommand("audioplayer");
                    if (cmd != null)
                    {
                        bool result = cmd.Execute(args, sender, out string cmdResponse);
                        response = cmdResponse;
                        return result;
                    }
                    else
                    {
                        response = "Плагин AudioPlayer не найден. Убедитесь, что он установлен.";
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    response = $"Ошибка: {ex.Message}";
                    return false;
                }

            case "list":
                if (_soundList.Count == 0)
                {
                    response = "Список звуков пуст.";
                    return true;
                }
                response = "=== Доступные звуки ===\n" +
                           string.Join("\n", _soundList.Select(kv => $"{kv.Key} - {kv.Value}"));
                return true;

            default:
                response = "Неизвестная Команда. Доступно: play, list.";
                return false;
        }
    }
}


