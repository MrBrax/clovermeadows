using System;
using vcrossing.Code.Items;
using vcrossing.Code.Player;
using vcrossing.Code.Save;

namespace vcrossing.Code.Npc;

public partial class Villager : BaseNpc
{

	public SittableNode SittingNode { get; set; }
	public LyingNode LyingNode { get; set; }

	public bool IsLyingOrSitting => SittingNode != null || LyingNode != null;

	private NpcSaveData _saveData;

	public NpcSaveData SaveData
	{
		get
		{
			if ( NpcData == null ) throw new NullReferenceException( "NpcData is null" );
			if ( string.IsNullOrEmpty( GetData().NpcId ) ) throw new NullReferenceException( "NpcId is null" );
			return _saveData ??= NpcSaveData.Load( GetData().NpcId );
		}
		set => _saveData = value;
	}

	public override bool ShouldDisableMovement()
	{
		if ( IsDisabled ) return true;
		if ( IsLyingOrSitting ) return true;
		if ( WorldManager.IsLoading ) return true;
		return false;
	}

	public override void _Ready()
	{
		base._Ready();
		SaveData = NpcSaveData.Load( GetData().NpcId );
	}

	public void LieInBed( PlacedItem bed )
	{
		if ( LyingNode != null )
		{
			Logger.LogError( "Already lying" );
			return;
		}

		var lyingNodes = bed.GetChildren().Where( c => c is LyingNode ).Cast<LyingNode>().ToList();

		var freeNode = lyingNodes.FirstOrDefault( n => !IsInstanceValid( n.Occupant ) );

		if ( freeNode != null )
		{
			Logger.Info( "Npc", "Lying node is free" );
			freeNode.Occupant = this;
			LyingNode = freeNode;

			LastPosition = GlobalPosition;

			SetState( CurrentState.SittingOrLying );
			GlobalPosition = freeNode.GlobalPosition;
			Model.Rotation = freeNode.GlobalRotation;
		}
		else
		{
			Logger.Warn( "Npc", "No free lying node" );
		}
	}

	public void GetUpFromBedOrSittable()
	{
		if ( LyingNode != null )
		{
			Logger.Info( "Npc", "Getting up" );
			LyingNode.Occupant = null;
			LyingNode = null;
			SetState( CurrentState.Idle );
			GlobalPosition = LastPosition;
		}
		else if ( SittingNode != null )
		{
			Logger.Info( "Npc", "Getting up" );
			SittingNode.Occupant = null;
			SittingNode = null;
			SetState( CurrentState.Idle );
			GlobalPosition = LastPosition;
		}
	}

	private void CheckForBed()
	{
		if ( FollowTarget is not PlayerController player )
		{
			Logger.LogError( "Follow target is not a player" );
			return;
		}

		var playerInteract = player.Interact;

		if ( !IsInstanceValid( playerInteract.LyingNode ) )
		{
			if ( IsInstanceValid( LyingNode ) )
			{
				GetUpFromBedOrSittable();
			}

			return;
		}

		if ( IsInstanceValid( LyingNode ) )
		{
			// GD.Print( "Lying node is free" );
			return;
		}

		var bed = playerInteract.LyingNode.GetParent();
		while ( IsInstanceValid( bed ) )
		{
			if ( bed is PlacedItem b )
			{
				/*var lyingNodes = b.GetChildren().Where( c => c is LyingNode ).Cast<LyingNode>().ToList();

				var freeNode = lyingNodes.FirstOrDefault( n => n.Occupant == null );

				if ( freeNode != null )
				{
					Logger.Info( "Lying node is free" );
					// playerInteract.LyingNode.Occupant = null;
					// playerInteract.LyingNode = null;

					freeNode.Occupant = this;
					LyingNode = freeNode;

					LastPosition = GlobalPosition;

					SetState( CurrentState.SittingOrLying );
					GlobalPosition = freeNode.GlobalPosition;
					Model.Rotation = freeNode.GlobalRotation;
				}
				else
				{
					Logger.Info( "No free lying node" );
				}

				return;*/

				LieInBed( b );
				return;
			}

			bed = bed.GetParent();
		}
	}


}
