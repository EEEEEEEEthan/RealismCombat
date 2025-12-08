using System.Collections.Generic;
using Godot;
/// <summary>
///     音频管理器，统一管理背景音乐和音效播放
/// </summary>
public partial class AudioManager : Node
{
	static AudioManager? instance;
	/// <summary>
	///     检查AudioManager单例是否已初始化
	/// </summary>
	public static bool IsInitialized => instance != null;
	/// <summary>
	///     检查BGM是否正在播放
	/// </summary>
	public static bool IsBgmPlaying => instance?.bgmPlayer?.Playing ?? false;
	/// <summary>
	///     检查音效是否正在播放
	/// </summary>
	public static bool IsSfxPlaying
	{
		get
		{
			if (instance == null) return false;
			if (instance.sfxPlayer is { Playing: true, }) return true;
			foreach (var player in instance.activeSfxPlayers)
			{
				if (!IsInstanceValid(player)) continue;
				if (player.Playing) return true;
			}
			return false;
		}
	}
	/// <summary>
	///     播放背景音乐
	/// </summary>
	/// <param name="stream">音频流</param>
	/// <param name="loop">是否循环</param>
	/// <param name="volumeDb">音量（分贝）</param>
	public static void PlayBgm(AudioStream stream, bool loop = true, float volumeDb = 0f)
	{
		if (instance?.bgmPlayer == null)
		{
			Log.PrintError("[AudioManager] BGM播放器未初始化");
			return;
		}
		instance.bgmPlayer.Stream = stream;
		instance.bgmPlayer.VolumeDb = volumeDb;
		instance.bgmPlayer.Play();
		Log.Print($"[AudioManager] 播放BGM: {stream.ResourcePath}");
	}
	/// <summary>
	///     停止背景音乐
	/// </summary>
	public static void StopBgm()
	{
		if (instance?.bgmPlayer == null)
		{
			Log.PrintError("[AudioManager] BGM播放器未初始化");
			return;
		}
		instance.bgmPlayer.Stop();
		Log.Print("[AudioManager] 停止BGM");
	}
	/// <summary>
	///     播放音效
	/// </summary>
	/// <param name="stream">音频流</param>
	/// <param name="volumeDb">音量（分贝）</param>
	public static void PlaySfx(AudioStream stream, float volumeDb = 0f)
	{
		var manager = instance;
		if (manager == null)
		{
			Log.PrintError("[AudioManager] 音效播放器未初始化");
			return;
		}
		if (manager.sfxPlayer is { Playing: false, })
		{
			manager.sfxPlayer.Stream = stream;
			manager.primarySfxBaseVolume = volumeDb;
			manager.sfxPlayer.VolumeDb = manager.ResolveSfxVolume(manager.primarySfxBaseVolume);
			manager.sfxPlayer.Play();
			return;
		}
		var extraPlayer = new AudioStreamPlayer
		{
			Stream = stream,
			VolumeDb = manager.ResolveSfxVolume(volumeDb),
			Bus = manager.sfxPlayer?.Bus ?? "Master",
		};
		manager.AddChild(extraPlayer);
		manager.activeSfxPlayers.Add(extraPlayer);
		manager.extraSfxBaseVolumes[extraPlayer] = volumeDb;
		void OnFinished()
		{
			extraPlayer.Finished -= OnFinished;
			manager.activeSfxPlayers.Remove(extraPlayer);
			manager.extraSfxBaseVolumes.Remove(extraPlayer);
			if (!IsInstanceValid(extraPlayer)) return;
			extraPlayer.QueueFree();
		}
		extraPlayer.Finished += OnFinished;
		extraPlayer.Play();
	}
	/// <summary>
	///     设置BGM音量
	/// </summary>
	/// <param name="volumeDb">音量（分贝）</param>
	public static void SetBgmVolume(float volumeDb)
	{
		if (instance?.bgmPlayer == null)
		{
			Log.PrintError("[AudioManager] BGM播放器未初始化");
			return;
		}
		instance.bgmPlayer.VolumeDb = volumeDb;
	}
	/// <summary>
	///     设置音效音量
	/// </summary>
	/// <param name="volumeDb">音量（分贝）</param>
	public static void SetSfxVolume(float volumeDb)
	{
		var manager = instance;
		if (manager == null)
		{
			Log.PrintError("[AudioManager] 音效播放器未初始化");
			return;
		}
		manager.sfxVolumeDb = volumeDb;
		if (manager.sfxPlayer != null) manager.sfxPlayer.VolumeDb = manager.ResolveSfxVolume(manager.primarySfxBaseVolume);
		foreach (var player in manager.activeSfxPlayers)
		{
			if (!IsInstanceValid(player)) continue;
			if (!manager.extraSfxBaseVolumes.TryGetValue(player, out var baseVolume)) baseVolume = 0f;
			player.VolumeDb = manager.ResolveSfxVolume(baseVolume);
		}
	}
	readonly List<AudioStreamPlayer> activeSfxPlayers = new();
	readonly Dictionary<AudioStreamPlayer, float> extraSfxBaseVolumes = new();
	AudioStreamPlayer? bgmPlayer;
	AudioStreamPlayer? sfxPlayer;
	float sfxVolumeDb;
	float primarySfxBaseVolume;
	public override void _Ready()
	{
		if (instance != null)
		{
			Log.PrintError("[AudioManager] 单例已存在，销毁重复实例");
			QueueFree();
			return;
		}
		instance = this;
		bgmPlayer = new()
		{
			Name = "BgmPlayer",
			Bus = "Master",
		};
		AddChild(bgmPlayer);
		sfxPlayer = new()
		{
			Name = "SfxPlayer",
			Bus = "Master",
		};
		AddChild(sfxPlayer);
		sfxVolumeDb = 0f;
		Log.Print("[AudioManager] 音频管理器初始化完成");
	}
	public override void _ExitTree()
	{
		if (instance == this)
		{
			foreach (var player in activeSfxPlayers)
			{
				if (!IsInstanceValid(player)) continue;
				player.Stop();
				player.QueueFree();
			}
			activeSfxPlayers.Clear();
			extraSfxBaseVolumes.Clear();
			instance = null;
		}
	}
	float ResolveSfxVolume(float volumeDb) => volumeDb + sfxVolumeDb;
}
