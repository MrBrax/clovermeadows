using System;
using System.Threading.Tasks;
using Godot.Collections;
using vcrossing.Code.Carriable;
using vcrossing.Code.Carriable.Actions;
using vcrossing.Code.Data;
using vcrossing.Code.Player;

namespace vcrossing.Code.Items;

public partial class Plant : WorldItem, IUsable, IWaterable, IWorldLoaded
{

    [Export] public Node3D SeedHole { get; set; } // TODO: better name

    public DateTime LastWatered { get; set; }

    public float GrowProgress { get; set; }
    public float WaterAmount { get; set; }
    public float WiltAmount { get; set; }
    public bool IsWilted { get; set; }

    private const float WiltSpeed = 10f;
    private const float WaterUseSpeed = 10f;
    private const float GrowSpeed = 10f;

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
        // WaterAmount = Math.Min( 100, WaterAmount + wateringCan.WaterAmount );
        WaterAmount = 100f;
        WiltAmount = 0f;
    }

    public override void _Process( double delta )
    {
        base._Process( delta );

        Render();

        if ( IsWilted ) return;

        if ( WaterAmount <= 0 )
        {
            WiltAmount += (float)delta * WiltSpeed;
            /* if ( WiltAmount >= 10 )
            {
                IsWilted = true;
                Logger.Info( "Plant wilted" );
            } */
        }
        else
        {
            /* GrowProgress += (float)delta;
            if ( GrowProgress >= 10 )
            {
                GrowProgress = 0;
                WaterAmount -= 10;
                Logger.Info( "Plant grew" );
            } */

            WaterAmount -= (float)delta * WaterUseSpeed;

            if ( GrowProgress < 100 )
            {
                GrowProgress += (float)delta * GrowSpeed;
            }
            else
            {

            }
        }
    }

    private void Render()
    {

        if ( Model == null ) return;

        var model = GetNode<MeshInstance3D>( Model );

        if ( model == null ) return;

        model.Scale = Vector3.One * (GrowProgress / 100f);

        SeedHole.Visible = GrowProgress < 5f;

        // Logger.Info( $"GrowProgress: {GrowProgress}, {model.Scale}" );

        /*  if ( IsWilted )
         {
             model.MaterialOverride = Loader.LoadResource<Material>( "res://Assets/Materials/WiltedPlant.tres" );
         }
         else
         {
             model.MaterialOverride = null;
         } */

        model.Position = new Vector3( 0, (GrowProgress / 100) * 0.1f, 0 );

    }

    public void WorldLoaded()
    {
        // TODO: calculate the grow, wilt and water amount based on the last watered time
    }

}
