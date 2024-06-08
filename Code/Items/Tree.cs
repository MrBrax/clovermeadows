using System;
using vcrossing.Code.Data;
using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

public partial class Tree : WorldItem, IUsable
{

	// public TreeData TreeData { get; private set; }

	public override bool CanBePickedUp()
	{
		return false;
	}

	public bool CanUse( PlayerController player )
	{
		return true;
	}

	public void OnUse( PlayerController player )
	{
		Shake();
	}

	private void Shake()
	{
		throw new NotImplementedException();
	}

	/* public override bool ShouldBeSaved()
	{
		return false;
	} */
}
