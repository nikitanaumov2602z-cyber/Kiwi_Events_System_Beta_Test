using Exiled.API.Enums;
using System.Collections.Generic;

public class RoleData
{
    public string Name { get; set; } = "";
    public string CustomName { get; set; } = "";
    public string Description { get; set; } = "";
    public List<ItemType> Items { get; set; } = new List<ItemType>();
    public float Health { get; set; } = 100;
    public Dictionary<ItemType, ushort> Ammo { get; set; } = new Dictionary<ItemType, ushort>();
    public Dictionary<EffectType, byte> Effects { get; set; } = new Dictionary<EffectType, byte>();
    public float ScaleX { get; set; } = 1f;
    public float ScaleY { get; set; } = 1f;
    public float ScaleZ { get; set; } = 1f;
}
