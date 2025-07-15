using System.Windows.Automation;

namespace ConquerKey.ActionHandlers
{
	public interface IActionHandler
	{
		AutomationElementCollection FindInteractableElements(AutomationElement rootElement);
		void Interact(AutomationElement window, AutomationElement element);
	}
}