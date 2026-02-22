using System.Text.Json.Serialization;

namespace ConquerKey.Configuration;

public class ConquerKeyConfig
{
	[JsonPropertyName("actions")]
	public Dictionary<string, ActionConfig> Actions { get; set; } = new();
}

public class ActionConfig
{
	[JsonPropertyName("keyBinding")]
	public string? KeyBinding { get; set; }

	[JsonPropertyName("enabled")]
	public bool Enabled { get; set; } = true;
}
