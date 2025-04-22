using System.Windows.Automation;

namespace ConquerKey;

public interface IActiveWindow
{
	public AutomationElement Current { get; }
}