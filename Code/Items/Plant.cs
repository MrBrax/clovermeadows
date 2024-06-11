using System;
using System.Threading.Tasks;
using Godot.Collections;
using vcrossing.Code.Carriable;
using vcrossing.Code.Carriable.Actions;
using vcrossing.Code.Data;
using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

public partial class Plant : WorldItem, IUsable, IWaterable
{

    public DateTime LastWatered { get; set; }

    public float GrowProgress { get; set; }
    public float WaterAmount { get; set; }

    public override Type PersistentType => typeof( Persistence.Plant );

    public bool CanUse( PlayerController player )
    {
        return true;
    }

    public void OnUse( PlayerController player )
    {
        throw new NotImplementedException();
    }

    public void OnWater( WateringCan wateringCan )
    {
        Logger.Info( "Watered plant" );
        LastWatered = DateTime.Now;
    }
}
