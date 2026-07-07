using Exiled.API.Features;
using System;
using System.Collections.Generic;
using MEC;

public class EventManager : IDisposable
{
    private readonly Plugin _plugin;
    private bool _isActive = false;
    private bool _isPrepared = false;
    private string _eventName = "";
    private string _rpLevel = "";
    private string _host = "";
    private DateTime _startTime;
    private CoroutineHandle _hudCoroutine;

    private static readonly HashSet<string> ValidLevels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "NonRP", "LRP", "LRP+", "MRP", "MRP+", "HardRP", "FullRP", "FullRP+"
    };

    public bool IsActive => _isActive;
    public bool IsPrepared => _isPrepared;

    public EventManager(Plugin plugin)
    {
        _plugin = plugin;
    }

    public void Prepare(string host)
    {
        if (_isActive) { Log.Warn("Ивент уже активен."); return; }
        _isPrepared = true;
        _host = host;
        Map.Broadcast(5, $"<color=yellow>Подготовка к ивенту! Проводящий: {host}</color>");
        Log.Info($"Подготовка инициирована проводящим {host}.");
    }

    public bool StartEvent(string name, string level, string host)
    {
        if (_isActive) { Log.Warn("Ивент уже активен."); return false; }
        if (!_isPrepared)
        {
            Log.Warn("Сначала подготовьте ивент через ev pod.");
            return false;
        }

        if (!ValidLevels.Contains(level))
        {
            Log.Warn($"Недопустимый уровень RP: {level}. Допустимые: {string.Join(", ", ValidLevels)}");
            return false;
        }

        _isPrepared = false;
        _eventName = name;
        _rpLevel = level;
        _host = string.IsNullOrEmpty(host) ? "Сервер" : host;
        _startTime = DateTime.Now;
        _isActive = true;

        _hudCoroutine = Timing.RunCoroutine(UpdateHudCoroutine());

        Log.Info($"Ивент '{name}' (уровень {_rpLevel}) запущен проводящим {_host}.");
        Map.Broadcast(5, $"<color=yellow>Ивент '{name}' начался! Проводящий: {_host}, уровень: {_rpLevel}</color>");
        return true;
    }

    public void StopEvent()
    {
        if (!_isActive) { Log.Warn("Ивент не активен."); return; }

        _isActive = false;
        _eventName = "";
        _rpLevel = "";
        _host = "";
        if (_hudCoroutine.IsRunning) Timing.KillCoroutines(_hudCoroutine);
        foreach (var player in Player.List) player.ClearHints();
        Log.Info("Ивент остановлен.");
        Map.Broadcast(5, "<color=red>Ивент завершён.</color>");
    }

    private IEnumerator<float> UpdateHudCoroutine()
    {
        while (_isActive)
        {
            TimeSpan elapsed = DateTime.Now - _startTime;
            string timeStr = $"{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";

            string hudText = _plugin.Config.HudMessageFormat
                .Replace("{name}", _eventName)
                .Replace("{level}", _rpLevel)
                .Replace("{time}", timeStr)
                .Replace("{host}", _host);

            foreach (var player in Player.List)
                player.ShowHint(hudText, _plugin.Config.HudUpdateInterval + 0.5f);

            yield return Timing.WaitForSeconds(_plugin.Config.HudUpdateInterval);
        }
    }

    public void Dispose() => StopEvent();
}
