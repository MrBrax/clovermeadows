using System;

namespace vcrossing.Code.Persistence;

public interface IPersistence
{

    public Type PersistentType { get; }

    public Dictionary<string, Variant> GetNodeData();
    public void SetNodeData( Dictionary<string, Variant> data );

}
