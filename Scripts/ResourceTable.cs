using System;
using Godot;
namespace RealismCombat;
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
}
public static class SpriteTable
{
	public static readonly Cache<AtlasTexture> arrowDown = new(() => CreateAtlas(ResourceTable.icon8, 16, 2, 8, 5));
	static AtlasTexture CreateAtlas(Texture2D texture, int x, int y, int width, int height)
	{
		var atlas = new AtlasTexture();
		atlas.Atlas = texture;
		atlas.Region = new(x, y, width, height);
		return atlas;
	}
}
