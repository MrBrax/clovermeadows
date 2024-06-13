﻿using System;
using vcrossing.Code.Player;
using vcrossing.Code.WorldBuilder.Weather;

namespace vcrossing.Code.WorldBuilder;

public partial class WeatherManager : Node3D
{

    public enum WeatherEffects
    {
        None = 0,
        Rain = 1,

    }

    [Export] public bool IsInside { get; set; } = false;
    [Export] public WorldEnvironment Environment { get; set; }
    [Export] public DirectionalLight3D SunLight { get; set; }

    private int StringToInt( string input )
    {
        var hash = 0;
        for ( int i = 0; i < input.Length; i++ )
        {
            hash = input[i] + (hash << 6) + (hash << 16) - hash;
        }
        return hash;
    }

    public bool PrecipitationEnabled { get; private set; }
    public bool LightningEnabled { get; private set; }
    public bool WindEnabled { get; private set; }
    public bool FogEnabled { get; private set; }

    protected float GetStaticFloat( string seed )
    {
        return GetStaticFloat( StringToInt( seed ) );
    }

    protected float GetStaticFloat( int seed )
    {
        var random = new Random( seed );
        return (float)random.NextSingle();
    }

    protected float GetPrecipitationChance( DateTime time )
    {
        var input = $"{time.DayOfYear}{time.Hour}-precipitation";
        return GetStaticFloat( input );
    }

    protected float GetLightningChance( DateTime time )
    {
        var input = $"{time.DayOfYear}{time.Hour}-lightning";
        return GetStaticFloat( input );
    }

    protected float GetFogChance( DateTime time )
    {
        var input = $"{time.DayOfYear}{time.Hour}-fog";
        return GetStaticFloat( input );
    }

    protected float GetCloudDensity( DateTime time )
    {
        var input = $"{time.DayOfYear}{time.Hour}-cloud-density";
        return GetStaticFloat( input );
    }

    public override void _Ready()
    {
        base._Ready();

        // Logger.Info( "WeatherManager", $"Precipitation chance: {GetPrecipitationChance( DateTime.Now )}" );
        // Logger.Info( "WeatherManager", $"Lightning chance: {GetLightningChance( DateTime.Now )}" );

        // debug check this week's weather
        /* for ( int i = 0; i < 7; i++ )
        {
            var date = DateTime.Now.AddDays( i );
            Logger.Info( "WeatherManager", $"Precipitation chance for {date}: {GetPrecipitationChance( date )}" );
            Logger.Info( "WeatherManager", $"Lightning chance for {date}: {GetLightningChance( date )}" );
        } */

        Setup();
    }


    private void Setup()
    {
        // remove     /// weather nodes
        /* foreach ( var child in GetChildren() )
        {
            child.QueueFree();
        } */

        var precipitationChance = GetPrecipitationChance( DateTime.Now );
        var lightningChance = GetLightningChance( DateTime.Now );
        var fogChance = GetFogChance( DateTime.Now );

        Logger.Info( "WeatherManager", $"Precipitation chance: {precipitationChance}" );
        Logger.Info( "WeatherManager", $"Lightning chance: {lightningChance}" );
        Logger.Info( "WeatherManager", $"Fog chance: {fogChance}" );

        /* SetPrecipitation( precipitationChance > 0.5f );
        SetLightning( lightningChance > 0.5f );
        //  SetWind( true );
        SetFog( fogChance > 0.2f ); */

        // reset all
        SetPrecipitation( false );
        SetLightning( false );
        SetWind( false );
        SetFog( false );

        if ( precipitationChance > 0.8f )
        {
            SetPrecipitation( true );

            // if it's raining, there's a higher chance of lightning
            if ( lightningChance > 0.8f )
            {
                SetLightning( true );
            }

            // higher chance of fog when it's raining
            if ( fogChance > 0.6f )
            {
                SetFog( true );
            }

            // cloud density is higher when it's raining
            SetCloudDensity( 0.5f + GetCloudDensity( DateTime.Now ) * 0.5f );
        }
        else
        {
            if ( lightningChance > 0.9f )
            {
                SetLightning( true );
            }

            if ( fogChance > 0.8f )
            {
                SetFog( true );
            }

            SetCloudDensity( GetCloudDensity( DateTime.Now ) * 0.5f );

        }

    }

    public override void _Process( double delta )
    {
        base._Process( delta );

        var player = GetNode<PlayerController>( "/root/Main/Player" );
        if ( player == null )
        {
            return;
        }

        GlobalPosition = player.GlobalPosition;
    }

    private void SetPrecipitation( bool state )
    {
        PrecipitationEnabled = state;
        if ( IsInside )
        {
            GetNode<Rain>( "RainInside" ).SetEnabled( state );
        }
        else
        {
            GetNode<Rain>( "RainOutside" ).SetEnabled( state );
        }
    }

    private void SetLightning( bool state )
    {
        LightningEnabled = state;
        if ( IsInside )
        {
            GetNode<WeatherBase>( "LightningInside" ).SetEnabled( state );
        }
        else
        {
            GetNode<WeatherBase>( "LightningOutside" ).SetEnabled( state );
        }
    }

    private void SetWind( bool state )
    {
        WindEnabled = state;
        if ( state && IsInside ) return; // no wind inside
        GetNode<WeatherBase>( "Wind" ).SetEnabled( state );
    }

    private void SetFog( bool state )
    {
        FogEnabled = state;
        if ( Environment == null ) return;
        Environment.Environment.FogDensity = state ? 0.04f : 0.0f;
        if ( PrecipitationEnabled )
        {
            GetNode<Rain>( "RainOutside" )?.SetFogState( state );
        }
    }

    private void SetCloudDensity( float density )
    {
        if ( SunLight == null ) return;
        SunLight.ShadowBlur = density * 2f;
    }

}
