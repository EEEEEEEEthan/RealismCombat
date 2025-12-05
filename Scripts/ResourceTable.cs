using System;
using System.Collections.Generic;
using Godot;
public class Cache<T>(Func<T> factory)
{
	public static implicit operator T(Cache<T> loader) => loader.Value;
	T? value;
	public T Value => value ??= factory();
}
public class Loader<T>(string path) : Cache<T>(() => GD.Load<T>(path)) where T : class { }
public static class ResourceTable
{
	public static readonly Loader<Texture2D> icon8 = new("res://Textures/Icon8.png");
	public static readonly Loader<Texture2D> bleeding = new("res://Textures/AnimationBleeding.png");
	public static readonly Loader<AudioStream> typingSound = new("res://Audios/gameboy-pluck-41265.mp3");
	public static readonly Loader<AudioStream> arpegio01Loop = new("res://Audios/arpegio01_loop-45094.mp3");
	public static readonly Loader<AudioStream> battleMusic1 = new("res://Audios/battle_music_1.mp3");
	public static readonly Loader<AudioStream> oneBeep = new("res://Audios/one_beep-99630.mp3");
	public static readonly Loader<AudioStream> retroClick = new("res://Audios/retro-click-236673.mp3");
	public static readonly Loader<AudioStream> retroHurt1 = new("res://Audios/retro-hurt-1-236672.mp3");
	public static readonly Loader<AudioStream> selection3 = new("res://Audios/selection3.wav");
	public static readonly Loader<AudioStream> blockSound = new("res://Audios/glitch-bass-101008.mp3");
	public static readonly Loader<PackedScene> gameNodeScene = new("res://Scenes/GameNode.tscn");
	public static readonly Loader<PackedScene> combatNodeScene = new("res://Scenes/CombatNode.tscn");
	public static readonly Loader<PackedScene> characterNodeScene = new("res://Scenes/CharacterNode.tscn");
	public static readonly Loader<PackedScene> propertyNodeScene = new("res://Scenes/PropertyNode.tscn");
	public static readonly Loader<PackedScene> menuDialogueScene = new("res://Scenes/MenuDialogue.tscn");
}
public static class SpriteTable
{
	public static readonly Cache<AtlasTexture> arrowDown = new(() => CreateAtlas(ResourceTable.icon8, 16, 2, 8, 5));
	public static readonly Cache<AtlasTexture> arrowRight = new(() => CreateAtlas(ResourceTable.icon8, 8, 0, 8, 8));
	public static readonly Cache<AtlasTexture> star = new(() => CreateAtlas(ResourceTable.icon8, 9, 9, 5, 5));
	public static readonly IReadOnlyList<Cache<AtlasTexture>> bleeding = new List<Cache<AtlasTexture>>
	{
		new(() => CreateAtlas(ResourceTable.bleeding, 0, 0, 16, 16)),
		new(() => CreateAtlas(ResourceTable.bleeding, 16, 0, 16, 16)),
		new(() => CreateAtlas(ResourceTable.bleeding, 32, 0, 16, 16)),
		new(() => CreateAtlas(ResourceTable.bleeding, 48, 0, 16, 16)),
		new(() => CreateAtlas(ResourceTable.bleeding, 64, 0, 16, 16)),
		new(() => CreateAtlas(ResourceTable.bleeding, 80, 0, 16, 16)),
		new(() => CreateAtlas(ResourceTable.bleeding, 96, 0, 16, 16)),
		new(() => CreateAtlas(ResourceTable.bleeding, 112, 0, 16, 16)),
		new(() => CreateAtlas(ResourceTable.bleeding, 128, 0, 16, 16)),
		new(() => CreateAtlas(ResourceTable.bleeding, 144, 0, 16, 16)),
		new(() => CreateAtlas(ResourceTable.bleeding, 160, 0, 16, 16)),
	};
	static AtlasTexture CreateAtlas(Texture2D texture, int x, int y, int width, int height)
	{
		var atlas = new AtlasTexture();
		atlas.Atlas = texture;
		atlas.Region = new(x, y, width, height);
		return atlas;
	}
}
