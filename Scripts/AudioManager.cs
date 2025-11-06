using Godot;
namespace RealismCombat;
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
	public static bool IsSfxPlaying => instance?.sfxPlayer?.Playing ?? false;
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
			Log.PrintErr("[AudioManager] BGM播放器未初始化");
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
			Log.PrintErr("[AudioManager] BGM播放器未初始化");
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
		if (instance?.sfxPlayer == null)
		{
			Log.PrintErr("[AudioManager] 音效播放器未初始化");
			return;
		}
		instance.sfxPlayer.Stream = stream;
		instance.sfxPlayer.VolumeDb = volumeDb;
		instance.sfxPlayer.Play();
	}
	/// <summary>
	///     设置BGM音量
	/// </summary>
	/// <param name="volumeDb">音量（分贝）</param>
	public static void SetBgmVolume(float volumeDb)
	{
		if (instance?.bgmPlayer == null)
		{
			Log.PrintErr("[AudioManager] BGM播放器未初始化");
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
		if (instance?.sfxPlayer == null)
		{
			Log.PrintErr("[AudioManager] 音效播放器未初始化");
			return;
		}
		instance.sfxPlayer.VolumeDb = volumeDb;
	}
	AudioStreamPlayer? bgmPlayer;
	AudioStreamPlayer? sfxPlayer;
	public override void _Ready()
	{
		if (instance != null)
		{
			Log.PrintErr("[AudioManager] 单例已存在，销毁重复实例");
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
		Log.Print("[AudioManager] 音频管理器初始化完成");
	}
	public override void _ExitTree()
	{
		if (instance == this) instance = null;
	}
}
