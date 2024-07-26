using System;
using vcrossing.Code.Data;
using vcrossing.Code.Items;
using vcrossing.Code.Objects;
using vcrossing.Code.Persistence;
using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

public sealed partial class CraftingStation : WorldItem, IUsable
{

    public enum CraftingStationType
    {
        CookingSimple = 1,
        CookingAdvanced = 2,
        General = 3,
    }

    [Export] public string Title;

    [Export] public CraftingStationType Type;

    public bool CanUse( PlayerController player )
    {
        return true;
    }

    public void OnUse( PlayerController player )
    {
        NodeManager.UserInterface.CreateCraftMenu( Type, Title );
    }
}