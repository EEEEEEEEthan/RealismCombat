using Godot;
using System;

public partial class ChatDialogue : Node
{
	[Export] Label label = null!;

	[Export] public float CharactersPerSecond { get; set; } = 30f;

	[Export] public bool AutoCalculateMaxLines { get; set; } = true;

	[Export] public int FallbackMaxLines { get; set; } = 3;

	[Export] public float PunctuationExtraDelaySeconds { get; set; } = 0.18f;

	[Export] public string PunctuationSet { get; set; } = "，。,.!?！？；;：:";

	/// <summary>
	/// 是否快速播放（加速显示）
	/// </summary>
	public bool FastForward { get; set; } = false;

	string _fullText = string.Empty;
	int _typedCount = 0;
	bool _isTyping = false;
	float _timeAccumulator = 0f;
	float _nextDelaySeconds = 0f;

	/// <summary>
	/// 开始按给定文本进行打字机显示
	/// </summary>
	/// <param name="text">需要显示的完整文本</param>
	public void StartTyping(string text)
	{
		EnsureLabelPrepared();
		_fullText = text ?? string.Empty;
		_typedCount = 0;
		label.Text = string.Empty;
		label.LinesSkipped = 0;
		UpdateMaxLinesVisible();
		_isTyping = true;
		_timeAccumulator = 0f;
		if (CharactersPerSecond <= 0)
		{
			label.Text = _fullText;
			UpdateMaxLinesVisible();
			UpdateScrollWindow();
			_isTyping = false;
			SetProcess(false);
			return;
		}
		_nextDelaySeconds = Math.Max(0.0001f, GetCharIntervalSeconds());
		SetProcess(true);
	}



	/// <summary>
	/// 是否正在打字
	/// </summary>
	public bool IsTyping => _isTyping;

    public override void _Ready()
    {
        if (label != null)
        {
            label.Resized += OnLabelResized;
        }
	}

	public override void _Process(double delta)
	{
		if (!_isTyping)
		{
			return;
		}
		if (CharactersPerSecond <= 0)
		{
			label.Text = _fullText;
			UpdateMaxLinesVisible();
			UpdateScrollWindow();
			_isTyping = false;
			SetProcess(false);
			return;
		}
		_timeAccumulator += (float)delta;
		var safety = 0;
		while (_isTyping && _timeAccumulator >= _nextDelaySeconds && safety < 64)
		{
			_timeAccumulator -= _nextDelaySeconds;
			EmitNextChar();
			safety++;
		}
	}

	void OnLabelResized()
	{
		UpdateMaxLinesVisible();
		UpdateScrollWindow();
	}

	void EmitNextChar()
	{
		if (_typedCount >= _fullText.Length)
		{
			_isTyping = false;
			SetProcess(false);
			return;
		}
		_typedCount++;
		label.Text = _fullText.Substring(0, _typedCount);
		UpdateMaxLinesVisible();
		UpdateScrollWindow();
		var nextDelay = GetCharIntervalSeconds();
		if (_typedCount > 0)
		{
			var c = _fullText[_typedCount - 1];
			if (IsPunctuation(c))
			{
				var extra = Math.Max(0f, PunctuationExtraDelaySeconds);
				if (FastForward)
				{
					extra *= 0.3f;
				}
				nextDelay += extra;
			}
		}
		_nextDelaySeconds = Math.Max(0.0001f, nextDelay);
	}

	void EnsureLabelPrepared()
	{
		if (label == null)
		{
			throw new InvalidOperationException("Label 未设置");
		}
		label.ClipText = true;
		label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
	}

	void UpdateScrollWindow()
	{
		var maxLines = label.MaxLinesVisible;
		if (maxLines <= 0)
		{
			label.LinesSkipped = 0;
			return;
		}
		var totalLines = label.GetLineCount();
		var overflow = totalLines - maxLines;
		label.LinesSkipped = Math.Max(0, overflow);
	}

	void UpdateMaxLinesVisible()
	{
		if (!AutoCalculateMaxLines)
		{
			label.MaxLinesVisible = Math.Max(1, FallbackMaxLines);
			return;
		}
		var computed = ComputeMaxLinesByFontMetrics();
		if (computed <= 0)
		{
			computed = Math.Max(1, FallbackMaxLines);
		}
		label.MaxLinesVisible = computed;
	}

	int ComputeMaxLinesByFontMetrics()
	{
		var font = label.GetThemeFont("font");
		var fontSize = label.GetThemeFontSize("font_size");
		if (font == null || fontSize <= 0)
		{
			return 0;
		}
		var height = font.GetHeight(fontSize);
		if (height <= 0)
		{
			return 0;
		}
		var h = label.Size.Y;
		if (h <= 0)
		{
			return 0;
		}
		return Math.Max(1, Mathf.FloorToInt(h / height));
	}

	float GetCharIntervalSeconds()
	{
		if (CharactersPerSecond <= 0)
		{
			return 0f;
		}
		var interval = 1f / CharactersPerSecond;
		if (FastForward)
		{
			interval /= 3f;
		}
		return interval;
	}

	bool IsPunctuation(char c)
	{
		if (string.IsNullOrEmpty(PunctuationSet))
		{
			return false;
		}
		return PunctuationSet.IndexOf(c) >= 0;
	}
}
