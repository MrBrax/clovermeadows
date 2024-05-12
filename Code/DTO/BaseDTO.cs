using System.Text.Json.Serialization;
using Godot;
using vcrossing2.Code.Items;

namespace vcrossing2.Code.DTO;

[JsonDerivedType( typeof( BaseDTO ), "base")]
[JsonDerivedType( typeof( BaseItemDTO ), "item")]
[JsonDerivedType( typeof( BaseCarriableDTO ), "carriable")]
public class BaseDTO
{
	
	// public Vector2I GridPosition { get; set; }
	
	public string ItemDataPath { get; set; }
	
	protected ItemData GetItemData()
	{
		return GD.Load<ItemData>( ItemDataPath );
	}
	
	public virtual string GetName()
	{
		return GetItemData().Name;
	}
	
}
