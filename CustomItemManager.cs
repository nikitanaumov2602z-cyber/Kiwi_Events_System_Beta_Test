using Exiled.API.Features;
using Exiled.API.Enums;
using Exiled.Events.EventArgs.Player;
using UnityEngine;
using MEC;
using System;

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
            case 5:
                player.AddItem(ItemType.GrenadeHE);
                player.SendConsoleMessage("Выдали C4 (взрывчатка). Используйте, чтобы установить бомбу.", "green");
                break;
            case 6:
                player.AddItem(ItemType.SCP500);
                player.SendConsoleMessage("Выдали Схему SCP-294. Используйте, чтобы получить случайный напиток.", "green");
                break;
            default:
                player.SendConsoleMessage($"Неизвестный ID: {id}. Используйте 'ci list'.", "red");
                break;
        }
    }

    public string GetItemList()
    {
        return "1 - ПНВ (зелёное НВ)\n2 - Тайзер\n3 - Стяжки (кепка 268)\n4 - TollGan (COM-15, пистолет выдачи ролей)\n5 - C4 (взрывчатка)\n6 - Схема SCP-294 (случайный напиток)";
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
            return;
        }

        // C4 (High-Explosive Grenade)
        if (ev.Player.CurrentItem.Type == ItemType.GrenadeHE)
        {
            ev.IsAllowed = false;
            Vector3 plantPos = ev.Player.Position;
            ev.Player.RemoveItem(ev.Player.CurrentItem);
            ev.Player.ShowHint("C4 установлена! Отойдите!", 2f);
            Map.Broadcast(3, $"<color=red>⚠️ C4 установлена! Взрыв через 3 секунды!</color>");
            Timing.RunCoroutine(ExplodeC4(plantPos));
            return;
        }

        // Схема SCP-294 (SCP-500)
        if (ev.Player.CurrentItem.Type == ItemType.SCP500)
        {
            ev.IsAllowed = false;
            GiveRandomDrink(ev.Player);
            ev.Player.RemoveItem(ev.Player.CurrentItem);
            ev.Player.ShowHint("Вы использовали Схему SCP-294!", 2f);
        }
    }

    private void GiveRandomDrink(Player player)
    {
        string[] drinks = {
            "RedBull (ускорение)",
            "Зелье здоровья (+40 HP)",
            "Антидот (снятие эффектов)",
            "Кофе (восстановление выносливости)",
            "Витаминный коктейль (регенерация HP)"
        };

        Random rnd = new Random();
        int choice = rnd.Next(drinks.Length);
        string drink = drinks[choice];
        player.SendConsoleMessage($"Вы получили напиток: {drink}", "green");

        switch (choice)
        {
            case 0: // RedBull
                player.EnableEffect(EffectType.MovementBoost, 30);
                break;
            case 1: // Зелье здоровья
                player.Health = Math.Min(player.Health + 40, player.MaxHealth);
                break;
            case 2: // Антидот
                player.ClearEffects();
                break;
            case 3: // Кофе
                player.EnableEffect(EffectType.Exhausted, 0); // снимаем усталость
                player.EnableEffect(EffectType.Vitality, 30);
                break;
            case 4: // Витаминный коктейль
                player.EnableEffect(EffectType.Regeneration, 30);
                break;
        }
    }

    private IEnumerator<float> ExplodeC4(Vector3 position)
    {
        yield return Timing.WaitForSeconds(3f);
        ExplosiveGrenade grenade = UnityEngine.Object.Instantiate(NetworkManager.singleton.spawnableGrenades[0]);
        grenade.Position = position;
        grenade.NetworkScale = 3f;
        grenade.FuseTime = 0.1f;
        grenade.ServerActivate();

        float damage = 200f;
        float radius = 8f;
        foreach (var player in Player.List)
        {
            if (Vector3.Distance(player.Position, position) <= radius)
            {
                player.Health -= damage;
                if (player.Health <= 0)
                    player.Kill(DamageTypes.Explosion);
            }
        }
        Map.Broadcast(5, "<color=red>💥 ВЗРЫВ C4!</color>");
        yield break;
    }
}
