using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class TimedAudioStreamPlayer3D : AudioStreamPlayer3D
{
	[Export] public TimedAudioStreamPlayer3DResource TimedAudioStreamPlayer3DResource { get; set; }

	public enum ActivityStateEnum
	{
		Active,
		Inactive
	}

	public ActivityStateEnum ActivityState
	{
		get => activityState; set
		{
			activityState = value;

			if (activityState == ActivityStateEnum.Inactive)
				Timer.Paused = true;
			else
				Timer.Paused = false;
		}
	}
	Dictionary<string, Array<AudioStream>> soundSets = new();

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

	AudioStreamRandomizer queuedStream = null;
	private ActivityStateEnum activityState = ActivityStateEnum.Active;

	public override void _Ready()
	{
		Finished += OnFinished;
		Timer.Timeout += OnTimeout;

		if (TimedAudioStreamPlayer3DResource.SoundSets.Count > 0)
			if (TimedAudioStreamPlayer3DResource.SoundSets.ContainsKey(CurrentSoundSet))
				if (TimedAudioStreamPlayer3DResource.SoundSets[CurrentSoundSet].Count > 0)
					SetStreams(TimedAudioStreamPlayer3DResource.SoundSets[CurrentSoundSet]);

		if (TimedAudioStreamPlayer3DResource.Autoplay)
			StartLoop();
	}

	public void SetRandomTime()
	{
		Timer.WaitTime = new Random().NextDouble() * TimedAudioStreamPlayer3DResource.RandomWaitTime + TimedAudioStreamPlayer3DResource.MinWaitTime;
	}

	public void OnTimeout()
	{
		if (_isLooping)
			Play();
	}

	public void OnFinished()
	{
		if (TimedAudioStreamPlayer3DResource.RandomWaitTime == 0 && TimedAudioStreamPlayer3DResource.MinWaitTime == 0)
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
		if (TimedAudioStreamPlayer3DResource.PlayOnLoopStart)
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

	public void SetStreams(Array<AudioStream> streams)
	{
		var randomizer = new AudioStreamRandomizer();
		foreach (var stream in streams)
			randomizer.AddStream(-1, stream);
		PitchScale = TimedAudioStreamPlayer3DResource.Pitch;
		randomizer.RandomPitch = TimedAudioStreamPlayer3DResource.RandomPitchAdded;
		randomizer.RandomVolumeOffsetDb = TimedAudioStreamPlayer3DResource.RandomVolumeAdded;

		queuedStream = randomizer;
	}

	public void UpdateSoundSet(string soundSet)
	{
		if (soundSets.ContainsKey(soundSet))
			SetStreams(soundSets[CurrentSoundSet]);
		else
			GD.PrintErr($"Sound set {soundSet} not found in local soundSets");
	}

	public void AddSoundSet(string soundSet, Array<AudioStream> streams, bool replace = false)
	{
		if (soundSets.ContainsKey(soundSet))
		{
			if (replace)
				soundSets[soundSet] = streams;
			else
				foreach (var stream in streams)
					soundSets[soundSet].Add(stream);
		}
		else
			soundSets.Add(soundSet, streams);
	}

	public void AddSoundSetsFromRaw(Array<AudioStream> streams, bool replace = false)
	{
		Dictionary<string, Array<AudioStream>> tempSoundSets = new();

		foreach (var stream in streams)
		{
			var prefix = stream.ResourcePath.Split("/").Last<string>().Split(".").First<string>().Split("_").First<string>();

			if (!tempSoundSets.ContainsKey(prefix))
				tempSoundSets[prefix] = new Array<AudioStream>();

			GD.Print($"Adding sound set \"{prefix}\" to local soundSets");
			tempSoundSets[prefix].Add(stream);
		}

		foreach (var soundSet in tempSoundSets)
			AddSoundSet(soundSet.Key, soundSet.Value, replace);
	}

	public void Play()
	{
		if (queuedStream != null)
		{
			Stream = queuedStream;
			queuedStream = null;
		}

		if (!Playing)
			base.Play();
	}
}
