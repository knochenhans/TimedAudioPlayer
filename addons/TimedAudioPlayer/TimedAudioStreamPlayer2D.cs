using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class TimedAudioStreamPlayer2D : AudioStreamPlayer2D
{
	[Export] public TimedAudioStreamPlayer2DResource TimedAudioStreamPlayer2DResource { get; set; }

	bool _isLooping = false;

	string currentSoundSet = "default";
	public string CurrentSoundSet
	{
		get => currentSoundSet;
		set
		{
			if (value != currentSoundSet)
			{
				currentSoundSet = value;
				UpdateSoundSet(currentSoundSet);
			}
		}
	}

	Timer Timer => GetNode<Timer>("Timer");

	public override void _Ready()
	{
		Finished += OnFinished;
		Timer.Timeout += OnTimeout;

		if (TimedAudioStreamPlayer2DResource.SoundSets.Count > 0)
			if (TimedAudioStreamPlayer2DResource.SoundSets.ContainsKey(CurrentSoundSet))
				if (TimedAudioStreamPlayer2DResource.SoundSets[CurrentSoundSet].Count > 0)
					SetStreams(TimedAudioStreamPlayer2DResource.SoundSets[CurrentSoundSet]);

		if (TimedAudioStreamPlayer2DResource.Autoplay)
			StartLoop();
	}

	public void SetRandomTime()
	{
		Timer.WaitTime = new Random().NextDouble() * TimedAudioStreamPlayer2DResource.RandomWaitTime + TimedAudioStreamPlayer2DResource.MinWaitTime;
	}

	public void OnTimeout()
	{
		if (_isLooping)
			Play();
	}

	public void OnFinished()
	{
		if (TimedAudioStreamPlayer2DResource.RandomWaitTime == 0 && TimedAudioStreamPlayer2DResource.MinWaitTime == 0)
		{
			if (_isLooping)
				Play();
		}
		else
		{
			SetRandomTime();
			Timer.Start();
		}
	}

	public void StartLoop()
	{
		_isLooping = true;
		if (TimedAudioStreamPlayer2DResource.PlayOnLoopStart)
			Play();
		else
		{
			SetRandomTime();
			Timer.Start();
		}
	}

	public void StopLoop()
	{
		_isLooping = false;
	}

	public void SetStreams(Godot.Collections.Array<AudioStream> streams)
	{
		var randomizer = new AudioStreamRandomizer();
		foreach (var stream in streams)
			randomizer.AddStream(-1, stream);
		PitchScale = TimedAudioStreamPlayer2DResource.Pitch;
		randomizer.RandomPitch = TimedAudioStreamPlayer2DResource.RandomPitchAdded;
		randomizer.RandomVolumeOffsetDb = TimedAudioStreamPlayer2DResource.RandomVolumeAdded;
		Stream = randomizer;
	}

	public void UpdateSoundSet(string soundSet)
	{
		if (TimedAudioStreamPlayer2DResource.SoundSets.ContainsKey(soundSet))
			SetStreams(TimedAudioStreamPlayer2DResource.SoundSets[CurrentSoundSet]);
		else
			GD.PrintErr($"Sound set {soundSet} not found in {nameof(TimedAudioStreamPlayer2DResource)}");
	}

	public void AddSoundSet(string soundSet, Godot.Collections.Array<AudioStream> streams, bool replace = false)
	{
		if (TimedAudioStreamPlayer2DResource.SoundSets.ContainsKey(soundSet))
		{
			if (replace)
				TimedAudioStreamPlayer2DResource.SoundSets[soundSet] = streams;
			else
				foreach (var stream in streams)
					TimedAudioStreamPlayer2DResource.SoundSets[soundSet].Add(stream);
		}
		else
			TimedAudioStreamPlayer2DResource.SoundSets.Add(soundSet, streams);
	}

	public void AddSoundSetsFromRaw(Array<AudioStream> streams, bool replace = false)
	{
		Dictionary<string, Array<AudioStream>> soundSets = new();

		foreach (var stream in streams)
		{
			var prefix = stream.ResourcePath.Split("/").Last<string>().Split(".").First<string>().Split("_").First<string>();

			if (!soundSets.ContainsKey(prefix))
				soundSets[prefix] = new Array<AudioStream>();

			GD.Print($"Adding sound set \"{prefix}\" for {nameof(TimedAudioStreamPlayer2DResource)}");
			soundSets[prefix].Add(stream);
		}

		foreach (var soundSet in soundSets)
			AddSoundSet(soundSet.Key, soundSet.Value, replace);
	}
}
