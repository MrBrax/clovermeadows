using System.Text.Json.Serialization;
using Godot;

namespace vcrossing2.Code.Persistence;

public class BaseCarriable : PersistentItem
{
	
	[JsonInclude] public int Durability { get; set; }
	
	public override void GetData( Node3D entity )
	{
		base.GetData( entity );
		
		if ( entity is Carriable.BaseCarriable carriable )
		{
			Durability = carriable.Durability;
		}
	}
}
