using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.Handlers;
using System;
using RemoteAdmin;
using CommandSystem;

public class Plugin : Plugin<Config>
{
    private EventHandlers _eventHandlers;
    private EventManager _eventManager;
    private CustomItemManager _customItemManager;
    private GrandRoleManager _grandRoleManager;
    private InspectCommand _inspectCommand;
    private HelpCommand _helpCommand;

    public override string Name => "KiwiEvents";
    public override string Author => "Kiwi Team";
    public override string Prefix => "KE";
    public override Version Version => new Version(1, 0, 0);
    public override Version RequiredExiledVersion => new Version(8, 0, 0);

    public EventManager EventManager => _eventManager;
    public CustomItemManager CustomItemManager => _customItemManager;
    public GrandRoleManager GrandRoleManager => _grandRoleManager;
    public InspectCommand InspectCommand => _inspectCommand;
    public HelpCommand HelpCommand => _helpCommand;

    public bool TeslaDisabled { get; set; } = false;
    public bool EscapeDisabled { get; set; } = false;

    public override void OnEnabled()
    {
        _eventHandlers = new EventHandlers(this);
        _eventManager = new EventManager(this);
        _customItemManager = new CustomItemManager(this);
        _grandRoleManager = new GrandRoleManager(this);

        RegisterEvents();
        RegisterRemoteAdminCommands();
        RegisterChatCommands();

        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        UnregisterEvents();
        UnregisterRemoteAdminCommands();
        UnregisterChatCommands();

        _eventManager?.Dispose();
        _eventHandlers = null;
        _eventManager = null;
        _customItemManager = null;
        _grandRoleManager = null;
        _inspectCommand = null;
        _helpCommand = null;

        base.OnDisabled();
    }

    private void RegisterEvents()
    {
        Player.Joined += _eventHandlers.OnPlayerJoined;
        Player.Died += _eventHandlers.OnPlayerDied;
        Server.RoundStarted += _eventHandlers.OnRoundStarted;
        Server.RoundEnded += _eventHandlers.OnRoundEnded;
        Player.ChangedRole += _eventHandlers.OnPlayerRoleChanged;

        if (Config.EnableCustomItems)
            Player.UsingItem += _customItemManager.OnUsingItem;

        if (Config.EnableInspectCommand || Config.EnableHelpCommand)
            Player.SendingChatMessage += _eventHandlers.OnSendingChatMessage;
    }

    private void UnregisterEvents()
    {
        Player.Joined -= _eventHandlers.OnPlayerJoined;
        Player.Died -= _eventHandlers.OnPlayerDied;
        Server.RoundStarted -= _eventHandlers.OnRoundStarted;
        Server.RoundEnded -= _eventHandlers.OnRoundEnded;
        Player.ChangedRole -= _eventHandlers.OnPlayerRoleChanged;
        Player.UsingItem -= _customItemManager.OnUsingItem;
        Player.SendingChatMessage -= _eventHandlers.OnSendingChatMessage;
    }

    private void RegisterRemoteAdminCommands()
    {
        CommandProcessor.Commands.AddCommand(new EvCommand(this));
        CommandProcessor.Commands.AddCommand(new CiCommand(this));
        CommandProcessor.Commands.AddCommand(new DisableTeslaCommand(this));
        CommandProcessor.Commands.AddCommand(new DisableEscapeCommand(this));
        CommandProcessor.Commands.AddCommand(new AudioPlayerCommand());
    }

    private void UnregisterRemoteAdminCommands()
    {
        CommandProcessor.Commands.RemoveCommand("ev");
        CommandProcessor.Commands.RemoveCommand("ci");
        CommandProcessor.Commands.RemoveCommand("disabletesla");
        CommandProcessor.Commands.RemoveCommand("disableescape");
        CommandProcessor.Commands.RemoveCommand("audioplayer");
    }

    private void RegisterChatCommands()
    {
        if (Config.EnableInspectCommand)
            _inspectCommand = new InspectCommand(this);
        if (Config.EnableHelpCommand)
            _helpCommand = new HelpCommand(this);
    }

    private void UnregisterChatCommands()
    {
        _inspectCommand = null;
        _helpCommand = null;
    }

    public void ReloadConfig()
    {
        var newConfig = Exiled.Loader.ConfigManager.Load<Config>(this);
        if (newConfig != null)
            Config = newConfig;
        Log.Info("Конфиг Перезагружен.");
    }

    public void ToggleTesla() => TeslaDisabled = !TeslaDisabled;
    public void ToggleEscape() => EscapeDisabled = !EscapeDisabled;
}
