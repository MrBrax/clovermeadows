namespace vcrossing2.Code.DTO;

public class BaseItemDTO : BaseDTO
{
	
	public World.ItemPlacementType PlacementType { get; set; } = World.ItemPlacementType.Placed;
	public World.ItemRotation GridRotation { get; set; } = World.ItemRotation.North;
	
}
