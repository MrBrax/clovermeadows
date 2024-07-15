using System;

namespace vcrossing.Code.Data;

public sealed partial class WeatherData : Resource
{

    [Flags]
    public enum WeatherEffects
    {
        None = 1 << 0,
        Rain = 1 << 1,
        Lightning = 1 << 2,
        Wind = 1 << 3,
        Snow = 1 << 4,
        Fog = 1 << 5,
    }

    [Export] public WeatherEffects WeatherEffect { get; set; } = WeatherEffects.None;

    [Export] public PackedScene InsideScene { get; set; }
    [Export] public PackedScene OutsideScene { get; set; }

    [Export] public Godot.Collections.Dictionary<int, float> TimeOfDayChance { get; set; } = [];


}
