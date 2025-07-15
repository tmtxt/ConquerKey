using System.Windows.Automation;

namespace ConquerKey.ActionWindow
{
	public interface IActionHandler
	{
		AutomationElementCollection FindInteractableElements(AutomationElement rootElement);
	}
}