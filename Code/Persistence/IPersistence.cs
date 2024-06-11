using System;

namespace vcrossing.Code.Persistence;

public interface IPersistence
{

    public Type PersistentType { get; }

    public Godot.Collections.Dictionary<string, Variant> GetNodeData();
    public void SetNodeData( Godot.Collections.Dictionary<string, Variant> data );

}
