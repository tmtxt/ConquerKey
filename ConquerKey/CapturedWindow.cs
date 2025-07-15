using System.Runtime.InteropServices;
using System.Windows.Automation;

namespace ConquerKey
{
	public class CapturedWindow : ICapturedWindow
	{
		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		private AutomationElement _windowInstance;

		public CapturedWindow()
		{
			var foregroundWindow = GetForegroundWindow();
			_windowInstance = AutomationElement.FromHandle(foregroundWindow);
		}
	}
}