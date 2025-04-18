using System.Windows;
using System.Windows.Automation;

namespace ConquerKey;

public partial class HintWindow : Window
{
	private readonly AutomationElement _activeWindow;

	public HintWindow(AutomationElement activeWindow)
	{
		_activeWindow = activeWindow;
		InitializeComponent();
		Loaded += HintWindow_Loaded;
		Activated += HintWindow_Activated;
	}

	private void HintWindow_Loaded(object sender, RoutedEventArgs e)
	{

	}

	private void HintWindow_Activated(object? sender, EventArgs e)
	{
		MyTextBox.Focus();
	}
}