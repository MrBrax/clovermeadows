using System;

namespace vcrossing.Code.Interfaces;

public interface IDataPath
{

	/// <summary>
	/// The path to the item data file.
	/// </summary>
	[Obsolete( "Use ItemDataId instead." )]
	public string ItemDataPath { get; set; }

	public string ItemDataId { get; set; }

}
