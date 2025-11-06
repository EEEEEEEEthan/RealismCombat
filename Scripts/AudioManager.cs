using Godot;
namespace RealismCombat;
/// <summary>
///     音频管理器，统一管理背景音乐和音效播放
/// </summary>
public partial class AudioManager : Node
{
	AudioStreamPlayer? bgmPlayer;
	AudioStreamPlayer? sfxPlayer;
	/// <summary>
	///     检查BGM是否正在播放
	/// </summary>
	public bool IsBgmPlaying => bgmPlayer?.Playing ?? false;
	/// <summary>
	///     检查音效是否正在播放
	/// </summary>
	public bool IsSfxPlaying => sfxPlayer?.Playing ?? false;
	public override void _Ready()
	{
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
	/// <summary>
	///     播放背景音乐
	/// </summary>
	/// <param name="stream">音频流</param>
	/// <param name="loop">是否循环</param>
	/// <param name="volumeDb">音量（分贝）</param>
	public void PlayBgm(AudioStream stream, bool loop = true, float volumeDb = 0f)
	{
		if (bgmPlayer == null)
		{
			Log.PrintErr("[AudioManager] BGM播放器未初始化");
			return;
		}
		bgmPlayer.Stream = stream;
		bgmPlayer.VolumeDb = volumeDb;
		bgmPlayer.Play();
		Log.Print($"[AudioManager] 播放BGM: {stream.ResourcePath}");
	}
	/// <summary>
	///     停止背景音乐
	/// </summary>
	public void StopBgm()
	{
		if (bgmPlayer == null)
		{
			Log.PrintErr("[AudioManager] BGM播放器未初始化");
			return;
		}
		bgmPlayer.Stop();
		Log.Print("[AudioManager] 停止BGM");
	}
	/// <summary>
	///     播放音效
	/// </summary>
	/// <param name="stream">音频流</param>
	/// <param name="volumeDb">音量（分贝）</param>
	public void PlaySfx(AudioStream stream, float volumeDb = 0f)
	{
		if (sfxPlayer == null)
		{
			Log.PrintErr("[AudioManager] 音效播放器未初始化");
			return;
		}
		sfxPlayer.Stream = stream;
		sfxPlayer.VolumeDb = volumeDb;
		sfxPlayer.Play();
	}
	/// <summary>
	///     设置BGM音量
	/// </summary>
	/// <param name="volumeDb">音量（分贝）</param>
	public void SetBgmVolume(float volumeDb)
	{
		if (bgmPlayer == null)
		{
			Log.PrintErr("[AudioManager] BGM播放器未初始化");
			return;
		}
		bgmPlayer.VolumeDb = volumeDb;
	}
	/// <summary>
	///     设置音效音量
	/// </summary>
	/// <param name="volumeDb">音量（分贝）</param>
	public void SetSfxVolume(float volumeDb)
	{
		if (sfxPlayer == null)
		{
			Log.PrintErr("[AudioManager] 音效播放器未初始化");
			return;
		}
		sfxPlayer.VolumeDb = volumeDb;
	}
}
