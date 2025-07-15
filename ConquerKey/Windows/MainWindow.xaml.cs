using System.Windows;

namespace ConquerKey.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
	}

	private void ShowHintWindow()
	{
		var activeWindow = WindowUtilities.GetActiveWindow();
		var hintWindow = new HintWindow(activeWindow);
		// hintWindow.Topmost = true;
		hintWindow.Show();
		// hintWindow.Activate();
		WindowUtilities.ActivateWindow(hintWindow);
	}


}