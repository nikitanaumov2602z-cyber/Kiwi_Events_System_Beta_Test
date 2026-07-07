using Exiled.API.Features;
using Exiled.API.Enums;
using Exiled.Events.EventArgs.Player;
using UnityEngine;

public class CustomItemManager
{
    private readonly Plugin _plugin;

    public CustomItemManager(Plugin plugin)
    {
        _plugin = plugin;
    }

    public void GiveItem(Player player, int id)
    {
        switch (id)
        {
            case 1:
                player.AddItem(ItemType.NightVisionGoggles);
                player.SendConsoleMessage("Выдали ПНВ (зелёное ночное зрение).", "green");
                break;
            case 2:
                player.AddItem(ItemType.Tazer);
                player.SendConsoleMessage("Выдали Тайзер.", "green");
                break;
            case 3:
                player.AddItem(ItemType.SCP268);
                player.SendConsoleMessage("Выдали Стяжки (кепка). Используйте на игрока для надевания наручников.", "green");
                break;
            case 4:
                player.AddItem(ItemType.GunCOM15);
                player.SendConsoleMessage("Выдали TollGan (COM-15, пистолет выдачи ролей). Наведите на игрока и стреляйте.", "green");
                break;
            default:
                player.SendConsoleMessage($"Неизвестный ID: {id}. Используйте 'ci list'.", "red");
                break;
        }
    }

    public string GetItemList()
    {
        return "1 - ПНВ (зелёное НВ)\n2 - Тайзер\n3 - Стяжки (кепка 268)\n4 - TollGan (COM-15, пистолет выдачи ролей)";
    }

    public void OnUsingItem(UsingItemEventArgs ev)
    {
        if (ev.Player.CurrentItem == null) return;

        // ПНВ
        if (ev.Player.CurrentItem.Type == ItemType.NightVisionGoggles)
        {
            if (ev.Player.IsNightVisionEnabled)
            {
                ev.Player.IsNightVisionEnabled = false;
                ev.Player.NightVisionColor = Color.white;
                ev.Player.ShowHint("Ночное зрение выключено.", 1.5f);
            }
            else
            {
                ev.Player.NightVisionColor = Color.green;
                ev.Player.IsNightVisionEnabled = true;
                ev.Player.ShowHint("Ночное зрение включено (зелёный фильтр).", 1.5f);
            }
            return;
        }

        // Стяжки (SCP-268)
        if (ev.Player.CurrentItem.Type == ItemType.SCP268)
        {
            ev.IsAllowed = false;
            if (Physics.Raycast(ev.Player.CameraTransform.position, ev.Player.CameraTransform.forward, out RaycastHit hit, 3f))
            {
                if (hit.collider.TryGetComponent<Player>(out Player target))
                {
                    target.AddItem(ItemType.Handcuffs);
                    ev.Player.RemoveItem(ev.Player.CurrentItem);
                    ev.Player.SendConsoleMessage($"Вы надели наручники на {target.Nickname}.", "green");
                    target.SendConsoleMessage($"{ev.Player.Nickname} надел на вас наручники.", "yellow");
                }
                else
                {
                    ev.Player.SendConsoleMessage("Вы должны смотреть на игрока.", "red");
                }
            }
            else
            {
                ev.Player.SendConsoleMessage("Вы должны смотреть на игрока.", "red");
            }
            return;
        }

        // TollGan (COM-15)
        if (ev.Player.CurrentItem.Type == ItemType.GunCOM15)
        {
            ev.IsAllowed = false;
            if (!ev.Player.RemoteAdminAccess)
            {
                ev.Player.SendConsoleMessage("У вас нет прав на использование TollGan.", "red");
                return;
            }

            if (Physics.Raycast(ev.Player.CameraTransform.position, ev.Player.CameraTransform.forward, out RaycastHit hit, 5f))
            {
                if (hit.collider.TryGetComponent<Player>(out Player target))
                {
                    string group = _plugin.Config.DefaultGunGroup;
                    if (!_plugin.Config.GrandRoles.ContainsKey(group))
                    {
                        ev.Player.SendConsoleMessage($"Группа {group} не найдена. Проверьте конфиг.", "red");
                        return;
                    }
                    _plugin.GrandRoleManager.GiveRandomRole(group, target);
                    ev.Player.RemoveItem(ev.Player.CurrentItem);
                    ev.Player.SendConsoleMessage($"Выдали роль из группы {group} игроку {target.Nickname}.", "green");
                }
                else
                {
                    ev.Player.SendConsoleMessage("Вы должны навестись на игрока.", "red");
                }
            }
            else
            {
                ev.Player.SendConsoleMessage("Вы должны навестись на игрока.", "red");
            }
        }
    }
}
