# Trading Card Game

A Unity-based trading card game featuring elemental reactions, strategic deck building, and turn-based combat.

## Features

- **Elemental Reaction System**: Fire, Water, Ice, and Lightning elements interact to create powerful reactions (Vaporize, Melt, Electro-Charged)
- **Strategic Deck Building**: Build custom decks with up to 30 cards, filter by element/cost/type
- **Turn-Based Combat**: Mana-based resource system with random first turn
- **Status Effects**: Freeze, Stun, Untargetable, and Spread mechanics
- **Card Effects**: Damage, Healing, Buffs, Life Steal, Draw Cards, Damage Reduction
- **Battle Log**: Real-time combat logging with color-coded messages
- **Save/Load System**: Persistent deck storage using PlayerPrefs (cross-platform, including WebGL)
- **WebGL Support**: Fully playable in browser via WebGL build

## Project Structure

```
Trading-Card-Game/
├── Assets/
│   ├── Scripts/
│   │   ├── Battle/              # Combat and effect systems
│   │   │   └── SkillsEffect/    # Effect execution and types
│   │   ├── Database/            # Card data structures
│   │   ├── Editor/              # Custom editor tools
│   │   ├── Handler/             # Card and health handlers
│   │   ├── Manager/             # Game managers
│   │   ├── UI/                  # User interface components
│   │   └── Util/                # Utility scripts
│   ├── ScriptableObjects/       # Card data assets
│   ├── Scenes/                  # Game scenes
│   └── Prefabs/                 # Game object prefabs
```

## Core Systems

### Card System

Cards are defined as ScriptableObjects (`CardDataSO`) with the following structure:

- **Basic Info**: ID, Name, Element, Type (Monster/Spell), Skill Type
- **Stats**: Cost, Attack, HP
- **Triggers**: Event-based effects (OnSummon, OnHit, PerTurn, OnTurnEnd)
- **Effects**: Damage, Heal, Buff, Status, LifeSteal, DrawCard, DamageReduction

**Card Types:**
- **Monster**: Can be played to the field, has ATK/HP, can attack
- **Spell**: Instant effect card, consumed after use

### Element System

Elements interact to create powerful reactions:

- **Vaporize**: Fire + Water (1.5x or 2.0x damage multiplier)
- **Melt**: Fire + Ice (2.0x or 1.5x damage multiplier)
- **Electro-Charged**: Lightning + Water (1.5x damage multiplier)

### Battle System

**Turn Flow:**
1. Random coin flip determines first player
2. First turn intro animation plays
3. Starting player draws a card
4. Mana increases by 1 (max 10)
5. Player plays cards and attacks
6. Turn ends, status effects update
7. Other player's turn begins

**Mana System:**
- Starts at 0, increases by 1 each turn (max 10)
- Used to play cards
- Refills to max at start of each turn

**Health System:**
- Player and Enemy leaders start with 20 HP
- Game ends when a leader's HP reaches 0

### Effect System

Effects are executed through a trigger-based system:

**Effect Types:**
- `DamageEffect`: Deal damage to target
- `HealingEffect`: Restore HP to target
- `BuffEffect`: Increase ATK or max HP
- `StatusEffect`: Apply status conditions
- `LifeStealEffect`: Deal damage and heal self
- `DrawCardEffect`: Draw cards from deck
- `DamageReductionEffect`: Reduce incoming damage

**Target Types:**
- `Self`: Effect user
- `SingleEnemy`: Random enemy unit
- `AllEnemies`: All enemy units
- `RandomEnemies(n)`: N random enemies
- `AllAllies`: All ally units
- `NearbyAllies`: Adjacent ally units
- `AreaAroundSelf`: All units in range
- `Leader`: Enemy/Player leader
- `HitTarget`: Unit that was attacked
- `All`: All units on field

**Status Effects:**
- `Freeze(n)`: Unit cannot attack for n turns
- `Stun(n)`: Unit cannot act for n turns
- `Untargetable(n)`: Unit cannot be targeted for n turns
- `Spread`: Copy element tags to nearby allies

### Deck System

**Deck Building:**
- Maximum 30 cards per deck
- Maximum 3 copies per card
- Filter cards by element, cost, and type
- Save/load decks with custom names

**Deck Storage:**
- Decks serialized as JSON and stored via `PlayerPrefs` (key: `saved_decks`)
- Compatible with all platforms including **WebGL** (uses browser IndexedDB)
- Can edit existing decks
- Can delete saved decks

## Key Scripts

### Managers

- **GameManager**: Main game initialization and manager coordination
- **BattleManager**: Combat logic, turn management, attack resolution
- **DeckManager**: Card pool management, deck generation, save/load
- **DeckBuilderManager**: Deck building UI and logic
- **ManaManager**: Mana system and UI updates
- **HandManager**: Hand card management
- **UIManager**: Scene navigation and UI panel management
- **BattleLogManager**: Battle logging system

### Battle Components

- **EffectExecutor**: Executes card effects based on triggers
- **EffectFactory**: Creates effect instances
- **TargetSelector**: Selects targets for effects
- **EffectContext**: Context data for effect execution
- **EffectTarget**: Target representation (card/leader)

### Handlers

- **HealthPointHandler**: HP management and UI
- **CardDragHandler**: Card drag-and-drop in deck builder
- **BattleCardDragHandler**: Card drag-and-drop in battle
- **AttackDragHandler**: Attack drag-and-drop targeting

### UI Components

- **CardDisplay**: Card visual representation
- **BattleLogUI**: Battle log display
- **FirstTurnIntro**: First turn animation
- **DeckOptionsPopup**: Deck options menu
- **DeckFilterPanel**: Card filtering UI

## Scenes

- **MainScene**: Main menu and deck selection
- **BattleScene**: Combat gameplay
- **DownloadDataScene**: Data loading scene

## Setup Instructions

1. Open the project in Unity (recommended version: 2021.3 or later)
2. Ensure all required packages are installed:
   - TextMesh Pro
   - Addressables
   - LeanTween (included)
   - Newtonsoft.Json
3. Set up card data in `Assets/ScriptableObjects/Cards/`
4. Assign card assets to Addressables with label `CardData`
5. In **Window → Asset Management → Addressables → Groups**, ensure the **CardData** group has the following schemas attached:
   - `Content Packing & Loading` (Build Path: `Local.BuildPath`, Load Path: `Local.LoadPath`)
   - `Content Update Restrictions`
6. Build Addressables: **Groups → Build → New Build → Default Build Script**
7. Configure manager references in the scene

## WebGL Deployment

1. Complete the Addressables build (Step 6 above) before each WebGL build
2. In **Build Settings**, select **WebGL** platform
3. Optionally enable **Development Build** to see console logs in the browser (`F12 → Console`)
4. Build and upload the output folder to your web server
5. Deck data persists in the browser's IndexedDB via `PlayerPrefs`

## Card Creation

To create a new card:

1. Right-click in `Assets/ScriptableObjects/Cards/`
2. Select `Create > TradingCardGame > Card Data`
3. Fill in card properties:
   - Basic info (ID, name, element, type)
   - Stats (cost, ATK, HP)
   - Visuals (sprite)
   - Description
   - Triggers and effects
4. Add to Addressables with "CardData" label

## Adding New Effects

To add a new effect type:

1. Create a new script in `Assets/Scripts/Battle/SkillsEffect/EffectType/`
2. Inherit from `EffectBase`
3. Implement `ApplyEffect(CardDisplay source, EffectContext context)`
4. Register in `EffectFactory.cs`
5. Use in card data via the `type` field

## Battle Log Integration

The battle log system provides color-coded messages:

- `LogDamage(attacker, target, damage)`: White attacker, red damage
- `LogHeal(target, heal)`: White target, green heal amount
- `LogStatus(message)`: Orange/yellow status messages
- `LogElementReaction(message)`: Blue element reaction messages
- `LogGeneral(message)`: General battle information

## Dependencies

- Unity 2021.3+
- TextMesh Pro
- Unity Addressables
- LeanTween (included in project)
- Newtonsoft.Json (for deck serialization)

## Future Enhancements

- Multiplayer support
- More element types and reactions
- Card rarity system
- Achievement system
- Tutorial mode
- AI difficulty settings

## License

This project is for educational and personal use.

## Credits

Developed as a Unity trading card game project.
