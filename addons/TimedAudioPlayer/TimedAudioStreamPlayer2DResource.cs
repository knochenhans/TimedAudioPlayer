using Godot;
using Godot.Collections;

public partial class TimedAudioStreamPlayer2DResource : Resource
{
    [Export] public double MinWaitTime { get; set; } = 0;
    [Export] public bool PlayOnLoopStart { get; set; } = true;
    [Export] public bool Autoplay { get; set; } = false;
    [Export] public float Pitch { get; set; } = 1;

    [Export] public Dictionary<string, Array<AudioStream>> SoundSets { get; set; } = new Dictionary<string, Array<AudioStream>>();

    [ExportGroup("Randomization Settings")]
    [Export] public double RandomWaitTime { get; set; } = 0;
    [Export] public float RandomPitchAdded { get; set; } = 1;
    [Export] public float RandomVolumeAdded { get; set; } = 0;
}