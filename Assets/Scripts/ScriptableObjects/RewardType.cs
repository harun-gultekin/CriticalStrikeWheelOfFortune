using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RewardType
{
    Currency,       // Cash, Gold
    UpgradePoint,   // Rifle Point, Armor Point vs.
    Weapon,         // Shotgun, SMG, Rifle (Tier 1-2-3)
    Cosmetic,       // Glasses, Hats
    Consumable,     // Healthshot, Grenade
    Chest,          // Big, Small, Silver Chest vs.
    Bomb            // Death
}

public enum Rarity
{
    Common,         // NoTier (Genelde)
    Rare,           // Tier 1
    Epic,           // Tier 2
    Legendary       // Tier 3
}