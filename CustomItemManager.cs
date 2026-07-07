using Exiled.API.Features;
using Exiled.API.Enums;
using Exiled.Events.EventArgs.Player;
using UnityEngine;
using MEC;
using System.Collections.Generic;

public class CustomItemManager
{
    private readonly Plugin _plugin;
    private Dictionary<Player, string> _pendingDrink = new Dictionary<Player, string>();

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
                player.SendConsoleMessage("Выдали ПНВ.", "green");
                break;
            case 2:
                player.AddItem(ItemType.Tazer);
                player.SendConsoleMessage("Выдали Тайзер.", "green");
                break;
            case 3:
                player.AddItem(ItemType.SCP268);
                player.SendConsoleMessage("Выдали Стяжки. Используйте на игрока для надевания наручников.", "green");
                break;
            case 4:
                player.AddItem(ItemType.GunCOM15);
                player.SendConsoleMessage("Выдали TollGan (COM-15, пистолет выдачи ролей). Наведите на игрока и стреляйте.", "green");
                break;
            case 5:
                player.AddItem(ItemType.SCP018);
                player.SendConsoleMessage("Выдали C4 (взрывчатка). Используйте, чтобы установить бомбу.", "green");
                break;
            case 6:
                player.AddItem(ItemType.SCP207);
                player.SendConsoleMessage("Выдали SCP-294 (кофейный автомат). Используйте для выбора напитка.", "green");
                break;
            default:
                player.SendConsoleMessage($"Неизвестный ID: {id}. Используйте 'ci list'.", "red");
                break;
        }
    }

    public string GetItemList()
    {
        return "1 - ПНВ\n2 - Тайзер\n3 - Стяжки \n4 - TollGan (COM-15, пистолет выдачи ролей)\n5 - C4 (взрывчатка)\n6 - SCP-294 (кофейный автомат)";
    }

    public void OnUsingItem(UsingItemEventArgs ev)
    {
        if (ev.Player.CurrentItem == null) return;

        // SCP-294
        if (ev.Player.CurrentItem.Type == ItemType.SCP207)
        {
            ev.IsAllowed = false;
            ev.Player.SendConsoleMessage("=== Выберите напиток (введите номер в консоль) ===", "cyan");
            ev.Player.SendConsoleMessage("1 - Кофе (скорость +20%, 60с)", "white");
            ev.Player.SendConsoleMessage("2 - Чай (регенерация HP, 30с)", "white");
            ev.Player.SendConsoleMessage("3 - Энергетик (скорость +30%, урон +10%, 45с)", "white");
            ev.Player.SendConsoleMessage("4 - Сок (восстановление HP 50)", "white");
            ev.Player.SendConsoleMessage("5 - Вода (снятие эффектов)", "white");
            ev.Player.SendConsoleMessage("6 - Яд (урон 50)", "red");
            ev.Player.SendConsoleMessage("7 - Случайный напиток", "yellow");
            _pendingDrink[ev.Player] = "waiting";
        }
    }
    public void ProcessDrinkChoice(Player player, string choice)
    {
        if (!_pendingDrink.ContainsKey(player)) return;
        _pendingDrink.Remove(player);

        if (!int.TryParse(choice, out int drinkId) || drinkId < 1 || drinkId > 7)
        {
            player.SendConsoleMessage("Неверный выбор. Используйте цифры 1-7.", "red");
            return;
        }

        ApplyDrink(player, drinkId);
    }

    private void ApplyDrink(Player player, int drinkId)
    {
        switch (drinkId)
        {
            case 1: // Cofe
                player.ShowHint("Вы выпили кофе! Скорость +20% на 60с.", 3f);
                player.Health += 10;
                break;
            case 2:
                player.ShowHint("Вы выпили чай! Регенерация HP 30с.", 3f)
                Timing.RunCoroutine(RegenCoroutine(player, 30f));
                break;
            case 3:
                player.ShowHint("Вы выпили энергетик! Скорость +30%, урон +10% на 45с.", 3f)
                player.Health += 20;
                break;
            case 4:
                player.ShowHint("Вы выпили сок! Восстановлено 50 HP.", 3f);
                player.Health += 50;
                break;
            case 5: // Reset
                player.ShowHint("Вы выпили воду! Эффекты сняты.", 3f)
                player.Health = 100; 
                break;
            case 6:
                player.ShowHint("Вы выпили яд! -50 HP.", 3f);
                player.Health -= 50;
                if (player.Health <= 0) player.Kill(DamageTypes.Poison);
                break;
            case 7:
                int random = UnityEngine.Random.Range(1, 7);
                ApplyDrink(player, random);
                break;
        }
    }

    private IEnumerator<float> RegenCoroutine(Player player, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration && player.IsAlive)
        {
            player.Health += 2;
            if (player.Health > player.MaxHealth) player.Health = player.MaxHealth;
            yield return Timing.WaitForSeconds(1f);
            elapsed += 1f;
        }
    }
}
