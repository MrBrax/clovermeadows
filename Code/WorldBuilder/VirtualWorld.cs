using System;

namespace vcrossing.Code.WorldBuilder;

public class VirtualWorld
{

    public string WorldId { get; set; }
    public string WorldName { get; set; }
    public string WorldPath { get; set; }
    public const int GridSize = 1;

    public const float GridSizeCenter = GridSize / 2f;

    public Dictionary<string, Dictionary<World.ItemPlacement, WorldNodeLink>> Items = new();

}
