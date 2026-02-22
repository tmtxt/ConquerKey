using System.Windows.Automation;
using System.Windows.Input;

namespace ConquerKey.ActionHandlers;

/// <summary>
/// Action that enables clicking on UI elements using keyboard navigation.
/// Default key binding: Win + F.
/// </summary>
public class ClickAction : IConquerKeyAction
{
	public string Name => "Click";

	public ActionKeyBinding DefaultKeyBinding => new(ModifierKeys.Windows, Key.F);

	public IElementFinder DefaultElementFinder { get; } = new ClickDefaultElementFinder();

	public void Interact(AutomationElement window, AutomationElement element)
	{
		var rect = element.Current.BoundingRectangle;
		var x = (int)(rect.X + rect.Width / 2);
		var y = (int)(rect.Y + rect.Height / 2);

		WindowUtilities.SendMouseClick(x, y);
	}

	/// <summary>
	/// Default element finder for the Click action.
	/// Discovers common interactable UI control types (buttons, links, inputs, etc.).
	/// </summary>
	private class ClickDefaultElementFinder : IElementFinder
	{
		public bool CanHandle(AutomationElement rootElement) => true;

		public AutomationElementCollection FindElements(AutomationElement window, AutomationElement element)
		{
			var visibleCondition = new PropertyCondition(AutomationElement.IsOffscreenProperty, false);
			var controlElementCondition = new OrCondition(
				new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
				new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Hyperlink),
				new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Tab),
				new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem),
				new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.CheckBox),
				new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.RadioButton),
				new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit),
				new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox),
				new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem),
				new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.DataItem),
				new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.MenuItem),
				new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.List),
				new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Menu),
				new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TreeItem),
				new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Table),
				new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.DataGrid)
			);
			var finalCondition = new AndCondition(
				controlElementCondition,
				visibleCondition
			);

			return element.FindAll(TreeScope.Descendants, finalCondition);
		}
	}
}
