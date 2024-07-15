namespace vcrossing.Code.Data;

[GlobalClass]
public sealed partial class EmoteData : Resource
{
	[Export] public string Name { get; set; } = "Emote";
	[Export] public Texture2D Texture { get; set; }
	[Export] public float DisplayDuration { get; set; } = 2.0f;
	[Export] public bool Loop { get; set; } = false;
	[Export] public string Animation { get; set; } = "default";

	[Export] public Tween.TransitionType TransitionAppearType { get; set; } = Tween.TransitionType.Linear;
	[Export] public Tween.TransitionType TransitionDisappearType { get; set; } = Tween.TransitionType.Linear;

	[Export] public float TransitionAppearTime { get; set; } = 0.5f;
	[Export] public float TransitionDisappearTime { get; set; } = 0.5f;

	[Export] public AudioStream Audio { get; set; }
}
