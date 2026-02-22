using System.Windows.Input;
using ConquerKey.ActionHandlers;

namespace ConquerKey.Configuration;

public static class KeyBindingParser
{
	/// <summary>
	/// Parses a key binding string like "Win+Shift+G" into an ActionKeyBinding.
	/// The last segment is the trigger key; all prior segments are modifiers.
	/// Supported modifiers: Ctrl (or Control), Alt, Shift, Win (or Windows).
	/// </summary>
	public static ActionKeyBinding Parse(string keyBindingString)
	{
		var parts = keyBindingString.Split('+', StringSplitOptions.TrimEntries);
		if (parts.Length == 0)
			throw new FormatException($"Invalid key binding: '{keyBindingString}'");

		var modifiers = ModifierKeys.None;
		for (var i = 0; i < parts.Length - 1; i++)
		{
			modifiers |= parts[i].ToLowerInvariant() switch
			{
				"ctrl" or "control" => ModifierKeys.Control,
				"alt" => ModifierKeys.Alt,
				"shift" => ModifierKeys.Shift,
				"win" or "windows" => ModifierKeys.Windows,
				_ => throw new FormatException($"Unknown modifier: '{parts[i]}'")
			};
		}

		var keyString = parts[^1];
		if (!Enum.TryParse<Key>(keyString, ignoreCase: true, out var key))
			throw new FormatException($"Unknown key: '{keyString}'");

		return new ActionKeyBinding(modifiers, key);
	}

	/// <summary>
	/// Converts an ActionKeyBinding back to its string representation.
	/// </summary>
	public static string ToString(ActionKeyBinding binding)
	{
		var parts = new List<string>();
		if (binding.Modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
		if (binding.Modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
		if (binding.Modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
		if (binding.Modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
		parts.Add(binding.Key.ToString());
		return string.Join("+", parts);
	}
}
