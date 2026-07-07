using CommandSystem;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using LabAPI.Features.Audio;
using UnityEngine;

[CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
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
        { "33", "Blue Feather (эмбиент из SCP: Containment Breach)" },
        { "34", "Подопытный D-9341, явитесь к ближайшему подразделению мобильной оперативной группы для извлечения нескольких особо важных объектов." },
        { "35", "AlphaWarheadsFail.ogg" }
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
                    string filePath = _soundList.ContainsKey(id)
                        ? _soundList[id]
                        : $"Звук с ID {id}";

                    AudioController.PlaySound(filePath, AudioPriority.High, 1f, true);

                    response = $"✅ Звук '{filePath}' проигран всем игрокам!";
                    Log.Info($"[KiwiEvents] AudioPlayer: Проигран звук {id} - {filePath}");
                    return true;
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
                response = "Неизвестная команда. Доступно: play, list.";
                return false;
        }
    }
}
