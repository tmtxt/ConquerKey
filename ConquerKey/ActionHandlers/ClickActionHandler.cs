using System.Windows.Automation;

namespace ConquerKey.ActionHandlers
{
	public class ClickActionHandler : IActionHandler
	{
		public AutomationElementCollection FindInteractableElements(AutomationElement rootElement)
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

			var clickableElements = rootElement.FindAll(TreeScope.Descendants, finalCondition);
			return clickableElements;
		}

		public void Interact(AutomationElement window, AutomationElement element)
		{
			var rect = element.Current.BoundingRectangle;
			var x = (int)(rect.X + rect.Width / 2);
			var y = (int)(rect.Y + rect.Height / 2);

			WindowUtilities.SendMouseClick(x, y);
		}
	}
}