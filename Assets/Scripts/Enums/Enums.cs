
public enum AnimationName
{
    IdleDown,
    IdleUp, 
    IdleRight, 
    IdleLeft,
    WalkDown,
    WalkUp, 
    WalkRight,
    WalkLeft,
    RunDown,
    RunUp, 
    RunRight,
    RunLeft,
    UseToolDown,
    UseToolUp,
    UseToolRight,
    UseToolLeft,
    SwingToolDown,
    SwingToolUp,
    SwingToolRight,
    SwingToolLeft,
    LiftToolDown,
    LiftToolUp,
    LiftToolRight,
    LiftToolLeft,
    HoldToolDown,
    HoldToolUp,
    HoldToolRight,
    HoldToolLeft,
    PickDown,
    PickUp,
    PickRight,
    PickLeft,
    Count
}
public enum CharacterPartAnimator
{
    Body,
    Arms,
    Hair,
    Tool,
    Hat,
    Count
}

public enum PartVariantColour
{
    None,
    Count
}

public enum PartVariantType
{
    None,
    Carry,
    Hoe,
    Pickaxe,
    Axe,
    Scythe,
    WateringCan,
    Count
}

public enum GridBoolProperty
{
    Diggable,
    CanDropItem,
    CanPlaceFurniture,
    IsPath,
    IsNPCObstacle
}

public enum InventoryLocation
{
    Player,
    Chest,
    Count
}

public enum Season
{
    Spring, 
    Summer,
    Autumn,
    Winter,
    None,
    Count
}

public enum Weather
{
    Dry, 
    Raining,
    Snowing,
    None, 
    Count
}

public enum SceneName
{
    Scene1_Farm,
    Scene2_Field,
    Scene3_Cabin
}

public enum ToolEffect
{
    None,
    Watering
}

public enum HarvestActionEffect
{
    DeciduousLeavesFalling,
    PineConesFalling,
    ChoppingTreeTrunk,
    BreakingStone, 
    Reaping,
    None
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right,
    None
}

public enum ItemType
{
    Seed,
    Commodity,
    Watering_tool,
    Hoeing_tool,
    Chopping_tool,
    Breaking_tool,
    Reaping_tool,
    Collecting_tool,
    Reapable_scenary,
    Furniture,
    None,
    Count
}

public enum Facing
{
    none, 
    front, 
    back,
    right
}