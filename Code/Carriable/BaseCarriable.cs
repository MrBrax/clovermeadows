using System;
using System.Text.Json;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;

namespace vcrossing.Code.Carriable;

public partial class BaseCarriable : Node3D, IWorldItem, IPersistence, IDataPath
{
	// [Signal] public delegate void Equip();
	// [Signal] public delegate void Unequip();
	// [Signal] public delegate void Use();

	[Export] public bool IsOnGround { get; set; }
	[Export] public bool IsPlacedInEditor { get; set; }
	[Export] public World.ItemPlacement Placement { get; set; } = World.ItemPlacement.Floor;

	[Export] public int Durability { get; set; }
	[Export] public float UseTime { get; set; }

	// public virtual Type PersistentType => typeof( Persistence.BaseCarriable );
	[Export] public string PersistentItemType { get; set; } = nameof( Persistence.BaseCarriable );

	[Export] public Node3D Model { get; set; }

	protected float _timeUntilUse = 0;

	// TODO: move these to signals
	public delegate void Used( PlayerController player );

	// public event Used OnUsed;

	public delegate void Equipped( PlayerController player );

	// public event Equipped OnEquipped;

	public delegate void Unequipped( PlayerController player );

	// public event Unequipped OnUnequipped;

	public delegate void Broken( PlayerController player );

	// public event Broken OnBroken;


	protected World World => GetNode<WorldManager>( "/root/Main/WorldManager" ).ActiveWorld;

	[Export( PropertyHint.File, "*.tres" )]
	public string ItemDataPath { get; set; }
	public string ItemDataId { get; set; }

	public Node3D Holder { get; private set; }
	public PlayerController Player => Holder as PlayerController;

	private ToolData _itemData;
	public ToolData ItemData
	{
		get
		{
			if ( _itemData == null )
			{
				if ( string.IsNullOrWhiteSpace( ItemDataPath ) ) throw new Exception( "ItemDataPath is null" );
				_itemData = Loader.LoadResource<ToolData>( ItemDataPath );
				if ( _itemData == null ) throw new Exception( $"Failed to load item data from {ItemDataPath}" );
			}
			return _itemData;
		}
		set => _itemData = value;
	}

	public void SetHolder( Node3D holder )
	{
		Logger.Info( $"Setting holder to {holder}" );
		Holder = holder;
	}

	/* protected void LoadItemData()
	{
		if ( string.IsNullOrEmpty( ItemDataPath ) ) throw new Exception( "ItemDataPath is null" );
		ItemData = Loader.LoadResource<ToolData>( ItemDataPath );
		if ( ItemData == null ) throw new Exception( $"Failed to load item data from {ItemDataPath}" );
	} */

	public string GetName()
	{
		var durabilityPercent = (float)Durability / ItemData.MaxDurability * 100;
		return $"{ItemData.Name} ({durabilityPercent}%)";
	}

	public override void _Ready()
	{
		base._Ready();
		// LoadItemData();
		_timeUntilUse = UseTime;
	}

	public virtual bool CanUse()
	{
		return _timeUntilUse <= 0 && !Player.IsInVehicle;
	}

	/* [Obsolete( "Use ItemData property instead" )]
	public ItemData GetItemData()
	{
		return Loader.LoadResource<ItemData>( ItemDataPath );
	} */

	public virtual float CustomPlayerSpeed()
	{
		return 1;
	}

	// TODO: don't use player since npc can use items too
	public virtual void OnEquip( PlayerController player )
	{
		SetHolder( player );
		// OnEquipped?.Invoke( player );
	}

	public virtual void OnUnequip( PlayerController player )
	{
		Holder = null;
		// OnUnequipped?.Invoke( player );
	}

	public virtual void OnUseDown( PlayerController player )
	{

	}

	public virtual void OnUseUp( PlayerController player )
	{

	}

	public virtual void OnUse( PlayerController player )
	{

	}

	public virtual void OnBreak( PlayerController player )
	{
		// OnBroken?.Invoke( player );
		// Inventory.RemoveItem( this );
	}

	public override void _Process( double delta )
	{
		base._Process( delta );

		if ( IsOnGround ) return;

		if ( _timeUntilUse > 0 )
		{
			_timeUntilUse -= (float)delta;
		}
	}

	public bool ShouldBeSaved()
	{
		return true;
	}

	internal virtual bool ShouldDisableMovement()
	{
		return false;
	}

	public Dictionary<string, object> GetNodeData()
	{
		return new Dictionary<string, object>
		{
			{ "Durability", Durability },
		};
	}

	public void SetNodeData( PersistentItem item, Dictionary<string, object> data )
	{
		// Durability = (int)data.GetValueOrDefault( "Durability", 0 );
		// Durability = data["Durability"] as int? ?? 0;
		if ( data.TryGetValue( "Durability", out var durability ) && durability is JsonElement element )
		{
			Durability = element.GetInt32();
		}
		else
		{
			Durability = 0;
		}
	}

}
