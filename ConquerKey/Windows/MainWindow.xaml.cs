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

	private void MinimizeButton_Click(object sender, RoutedEventArgs e)
	{
		WindowState = WindowState.Minimized;
	}

	private void ExitButton_Click(object sender, RoutedEventArgs e)
	{
		Application.Current.Shutdown();
	}
}