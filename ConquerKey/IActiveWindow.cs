using System.Windows.Automation;

namespace ConquerKey;

public interface IActiveWindow
{
	public AutomationElement Instance { get; }
}