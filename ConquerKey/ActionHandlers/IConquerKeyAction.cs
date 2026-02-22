using System.Windows.Automation;
using System.Windows.Input;

namespace ConquerKey.ActionHandlers;

/// <summary>
/// Represents a key binding for an action, consisting of modifier keys and a trigger key.
/// </summary>
/// <param name="Modifiers">The modifier keys (Ctrl, Alt, Shift, Windows) required for the binding.</param>
/// <param name="Key">The trigger key that activates the action when pressed with the modifiers.</param>
public record ActionKeyBinding(ModifierKeys Modifiers, Key Key);

/// <summary>
/// Interface for ConquerKey actions that provide keyboard-driven interactions with UI elements.
/// </summary>
public interface IConquerKeyAction
{
	/// <summary>
	/// Gets the unique name of the action.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the default key binding that activates this action.
	/// Can be overridden by user configuration in ~/.conquerkey.json.
	/// </summary>
	ActionKeyBinding DefaultKeyBinding { get; }

	/// <summary>
	/// Gets the default element finder used by this action to discover UI elements.
	/// Used as a fallback when no discovered IElementFinder's CanHandle returns true.
	/// </summary>
	IElementFinder DefaultElementFinder { get; }

	/// <summary>
	/// Performs the action on the specified UI element.
	/// </summary>
	/// <param name="window">The parent window containing the element.</param>
	/// <param name="element">The target element to interact with.</param>
	void Interact(AutomationElement window, AutomationElement element);
}
