using System.Windows.Automation;
using System.Windows.Input;

namespace ConquerKey.ActionHandlers;

/// <summary>
/// Represents a key binding for a plugin, consisting of modifier keys and a trigger key.
/// </summary>
/// <param name="Modifiers">The modifier keys (Ctrl, Alt, Shift, Windows) required for the binding.</param>
/// <param name="Key">The trigger key that activates the plugin when pressed with the modifiers.</param>
public record PluginKeyBinding(ModifierKeys Modifiers, Key Key);

/// <summary>
/// Interface for ConquerKey plugins that provide keyboard-driven interactions with UI elements.
/// </summary>
public interface IConquerKeyPlugin
{
  /// <summary>
  /// Gets the unique name of the plugin.
  /// </summary>
  string Name { get; }

  /// <summary>
  /// Gets the key binding that activates this plugin.
  /// </summary>
  PluginKeyBinding KeyBinding { get; }

  /// <summary>
  /// Finds all interactable UI elements within the specified root element.
  /// </summary>
  /// <param name="rootElement">The root automation element to search within.</param>
  /// <returns>A collection of interactable automation elements.</returns>
  AutomationElementCollection FindInteractableElements(AutomationElement rootElement);

  /// <summary>
  /// Performs the plugin's action on the specified UI element.
  /// </summary>
  /// <param name="window">The parent window containing the element.</param>
  /// <param name="element">The target element to interact with.</param>
  void Interact(AutomationElement window, AutomationElement element);
}
