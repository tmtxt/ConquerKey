using System.IO;
using System.Text.Json;

namespace ConquerKey.Configuration;

public class ConfigManager
{
	private readonly ConquerKeyConfig _config;

	public ConquerKeyConfig Config => _config;

	public ConfigManager()
	{
		_config = LoadConfig();
	}

	/// <summary>
	/// Tries to get the configuration for a specific action by name.
	/// </summary>
	public bool TryGetActionConfig(string actionName, out ActionConfig? actionConfig)
	{
		return _config.Actions.TryGetValue(actionName, out actionConfig);
	}

	private static ConquerKeyConfig LoadConfig()
	{
		var configPath = GetConfigFilePath();
		if (!File.Exists(configPath))
			return new ConquerKeyConfig();

		try
		{
			var json = File.ReadAllText(configPath);
			return JsonSerializer.Deserialize<ConquerKeyConfig>(json) ?? new ConquerKeyConfig();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Failed to load config from {configPath}: {ex.Message}");
			return new ConquerKeyConfig();
		}
	}

	public static string GetConfigFilePath()
	{
		var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		return Path.Combine(userProfile, ".conquerkey.json");
	}
}
