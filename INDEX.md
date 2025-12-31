# Critical Strike Wheel of Fortune - Project Index

## 📋 Project Overview

A Unity-based wheel of fortune game where players spin a wheel to win rewards, progress through zones, and collect items. The game features a zone-based progression system with special safe zones and super zones, an inventory system, and a reward claiming mechanism.

## 🏗️ Project Structure

```
CriticalStrikeWheelOfFortune/
├── Assets/
│   ├── Art/                    # Visual assets
│   │   ├── Fonts/              # Font files
│   │   ├── Sprites/            # Sprite assets (57 PNG files)
│   │   └── UI/                 # UI elements
│   ├── Plugins/                # Third-party plugins
│   │   └── Demigiant/          # DOTween animation library
│   ├── Prefabs/                # Unity prefabs
│   │   └── UI/                 # UI prefabs (3 prefabs)
│   ├── Resources/              # Runtime resources
│   │   └── DOTweenSettings.asset
│   ├── Scenes/                 # Unity scenes
│   │   └── DemoScene.unity     # Main game scene
│   ├── ScriptableObjects/      # Game data assets
│   │   ├── GameConfig.asset    # Main game configuration
│   │   ├── Rewards/            # Reward item assets (27 assets)
│   │   └── Wheels/             # Wheel tier configurations (3 assets)
│   ├── Scripts/                # C# source code
│   │   ├── InventorySystem/    # Inventory management
│   │   ├── Managers/           # Core game managers
│   │   ├── RewardSystem/       # Reward claiming system
│   │   ├── ScreenSystem/       # Screen management (empty)
│   │   ├── ScriptableObjects/  # ScriptableObject definitions
│   │   ├── Utilities/          # Utility classes (empty)
│   │   ├── WheelSystem/        # Wheel spinning mechanics
│   │   └── ZoneSystem/         # Zone management (empty)
│   └── TextMesh Pro/           # TextMesh Pro assets
├── Library/                    # Unity generated files
├── Logs/                       # Unity logs
├── Packages/                   # Package dependencies
├── ProjectSettings/            # Unity project settings
└── Temp/                       # Temporary build files
```

## 📦 Dependencies

### Unity Packages
- **Unity Version**: See `ProjectSettings/ProjectVersion.txt`
- **TextMesh Pro**: 3.0.6
- **DOTween**: Via Demigiant plugin (animation library)
- **Unity UI (uGUI)**: 1.0.0
- **Unity 2D**: Feature package 2.0.1

## 🎮 Core Systems

### 1. Game Manager (`Managers/GameManager.cs`)
**Purpose**: Central game state manager and coordinator

**Key Responsibilities**:
- Manages game state (current zone, money, collected rewards)
- Handles win/lose conditions
- Coordinates UI screens (win, lose, claim screens)
- Manages button interactions
- Zone progression logic

**Key Methods**:
- `RestartGame()`: Resets game to initial state
- `OnSpinEnded()`: Handles spin result
- `HandleWin()`: Processes win condition with reward card display
- `HandleLose()`: Processes lose condition
- `Button_Continue()`: Advances to next zone
- `Button_ExitAndClaim()`: Shows claim screen
- `Button_Revive()`: Revives player for a cost
- `UpdateZoneUI()`: Updates zone display with color coding

**Zone Types**:
- **Standard Zones**: Normal gameplay with bombs
- **Safe Zones** (every 5): Silver tier, no bombs
- **Super Zones** (every 30): Gold tier, no bombs

### 2. Level Manager (`Managers/LevelManager.cs`)
**Purpose**: Generates wheel configurations for each zone

**Key Features**:
- Zone-based tier selection (Bronze/Silver/Gold)
- Weighted random reward selection
- Dynamic reward amount scaling based on zone
- Bomb placement logic
- List shuffling for random distribution

**Key Methods**:
- `GenerateLevel(int zoneIndex)`: Creates 8 slices for the wheel
- `GetRandomRewardWeighted()`: Weighted random selection
- `ShuffleList()`: Fisher-Yates shuffle algorithm

**RuntimeSlice Structure**:
```csharp
public class RuntimeSlice
{
    public RewardItemSO item;
    public int amount;
    public bool isBomb;
}
```

### 3. Wheel Controller (`WheelSystem/WheelController.cs`)
**Purpose**: Handles wheel spinning mechanics and animations

**Key Features**:
- DOTween-based smooth spinning animation
- Smart angle calculation for target slice
- Flip animation when changing levels
- Button state management

**Key Methods**:
- `SpinWheel()`: Initiates wheel spin with target selection
- `SetupNewLevel()`: Prepares wheel for new zone with animation
- `AnimateSlicesChange()`: Flip animation for slice updates

**Settings**:
- `spinDuration`: 4 seconds default
- `spinRounds`: 5 full rotations default

### 4. Wheel Slice UI (`WheelSystem/WheelSliceUI.cs`)
**Purpose**: Individual slice display component

**Features**:
- Displays reward icon and amount
- Updates slice data dynamically

### 5. Inventory System

#### Inventory UI (`InventorySystem/InventoryUI.cs`)
**Purpose**: Manages inventory display

**Features**:
- Adds items to inventory
- Clears inventory on restart
- Visual item display

#### Inventory Item UI (`InventorySystem/InventoryItemUI.cs`)
**Purpose**: Individual inventory item display

### 6. Reward System

#### Reward Claim Screen (`RewardSystem/RewardClaimScreen.cs`)
**Purpose**: Displays collected rewards at end of session

**Key Features**:
- Reward compression (stacks same items)
- Sequential card animation
- Auto-scrolling scroll view
- Animated card appearance

**Key Methods**:
- `ShowClaimSequence()`: Main entry point
- `CompressRewards()`: Groups and sums duplicate rewards
- `ShowCardsRoutine()`: Coroutine for animated display

### 7. Scriptable Objects

#### Game Config SO (`ScriptableObjects/GameConfigSO.cs`)
**Purpose**: Main game configuration

**Configuration**:
- `bronzeTier`: Standard wheel (with bombs)
- `silverTier`: Safe zone wheel (no bombs)
- `goldTier`: Super zone wheel (no bombs)
- `safeZoneInterval`: 5 (every 5th zone)
- `superZoneInterval`: 30 (every 30th zone)
- `amountMultiplierPerZone`: 0.1 (10% increase per zone)

**Key Methods**:
- `GetTierForZone(int zoneIndex)`: Returns appropriate tier

#### Wheel Tier SO (`ScriptableObjects/WheelTierSO.cs`)
**Purpose**: Defines wheel tier templates

**Properties**:
- `tierName`: "Bronze", "Silver", "Gold"
- `hasBomb`: Whether bombs appear in this tier
- `bombItem`: Bomb reward item reference
- `potentialRewards`: List of possible rewards with weights

**RewardDef Structure**:
```csharp
public struct RewardDef
{
    public RewardItemSO item;
    public int baseAmount;    // Base amount for zone 1
    public float weight;      // Drop weight (0-100)
}
```

#### Reward Item SO (`ScriptableObjects/RewardItemSO.cs`)
**Purpose**: Defines individual reward items

**Properties**:
- `id`: Unique identifier
- `displayName`: UI display name
- `icon`: Sprite icon
- `frameIcon`: Optional rarity frame
- `type`: RewardType enum
- `rarity`: Rarity enum

#### Reward Type (`ScriptableObjects/RewardType.cs`)
**Purpose**: Enumerations for reward system

**Enums**:
- `RewardType`: Currency, UpgradePoint, Weapon, Cosmetic, Consumable, Chest, Bomb
- `Rarity`: Common, Rare, Epic, Legendary

## 🎯 Game Flow

1. **Game Start**: `GameManager.RestartGame()` initializes zone 1
2. **Level Setup**: `WheelController.SetupNewLevel()` prepares wheel with `LevelManager.GenerateLevel()`
3. **Player Spins**: `WheelController.SpinWheel()` animates wheel
4. **Result Processing**: `GameManager.OnSpinEnded()` handles result
   - **Win**: Adds reward to inventory, shows win screen
   - **Lose**: Shows lose screen with revive option
5. **Zone Progression**: Player continues to next zone or exits to claim
6. **Reward Claiming**: `RewardClaimScreen` displays all collected rewards

## 🎨 UI Components

### Win Screen
- Reward card display (prefab-based)
- Continue button (advance zone)
- Exit button (go to claim screen)

### Lose Screen
- Failure information display
- Revive button (cost: 100 money)
- Give up button (restart game)

### Claim Screen
- Scrollable reward cards
- Sequential animation
- Main menu button (restarts game)

### Wheel UI
- 8 slice wheel
- Spin button
- Zone display (color-coded)

## 🔧 Technical Details

### Animation System
- **DOTween**: Used for all animations
  - Wheel rotation
  - Slice flip animations
  - Card pop-in animations
  - Button scale animations

### Zone System
- Zone numbers displayed with color coding:
  - **White**: Standard zones
  - **Silver (#C0C0C0)**: Safe zones (every 5)
  - **Gold**: Super zones (every 30)

### Reward Scaling
Formula: `finalAmount = baseAmount * (1 + (zoneIndex * multiplierPerZone))`
- Example: Zone 10, base 100, multiplier 0.1 → 200

### Weighted Random Selection
Uses cumulative weight distribution for fair random selection from reward pools.

## 📝 Notes

- Empty directories: `ScreenSystem/`, `Utilities/`, `ZoneSystem/` (reserved for future features)
- Auto-referencing: Many components use `OnValidate()` to auto-find UI elements by name
- Button naming convention: `ui_btn_*` for automatic discovery
- Prefab structure: Cards use `ui_image_icon_value` and `ui_text_amount_value` naming

## 🚀 Future Expansion Areas

1. **ScreenSystem**: Screen management framework
2. **Utilities**: Helper classes and extensions
3. **ZoneSystem**: Advanced zone mechanics
4. Additional reward types and mechanics
5. Save/load system for progress persistence

## 📊 Asset Counts

- **Scripts**: 11 C# files
- **Reward Assets**: 27 ScriptableObject assets
- **Wheel Configs**: 3 tier configurations
- **Sprites**: 57 PNG files
- **UI Prefabs**: 3 prefabs

---

*Last indexed: Generated automatically*
*Unity Project: CriticalStrikeWheelOfFortune*

