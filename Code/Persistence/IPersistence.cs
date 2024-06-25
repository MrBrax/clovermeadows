using System;

namespace vcrossing.Code.Persistence;

public interface IPersistence
{

	public string PersistentItemType { get; set; }

	// [Obsolete] public Type PersistentType { get; }

	/// <summary>
	/// Get the data of the node. Used for saving the node. Just return a dictionary with the data you want to save.
	/// </summary>
	public Dictionary<string, object> GetNodeData();

	/// <summary>
	/// Set the data of the node. Used for loading the node. Use <see cref="Dictionary.GetValueOrDefault"/> and casting to get the data.
	/// </summary>
	public void SetNodeData( PersistentItem item, Dictionary<string, object> data );

}
