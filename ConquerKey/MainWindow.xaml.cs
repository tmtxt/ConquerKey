using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ConquerKey;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

	public MainWindow()
	{
		InitializeComponent();
		Loaded += MainWindow_Loaded;
	}

	private void MainWindow_Loaded(object sender, RoutedEventArgs e)
	{
		string windowTitle = "CargoWise Next - ediProd - Branch: Sydney Aust Branch - Company: WiseTech Global (Australia) Pty Ltd - Department: Development";
		var rootElement = GetRootElementByWindowTitle(windowTitle);

		// Add your logic here
		Width = rootElement.Current.BoundingRectangle.Width * (96.0 / VisualTreeHelper.GetDpi(this).PixelsPerInchX);
		// Width = rootElement.Current.BoundingRectangle.Width;
		Height = rootElement.Current.BoundingRectangle.Height * (96.0 / VisualTreeHelper.GetDpi(this).PixelsPerInchY);
		// Left = rootElement.Current.BoundingRectangle.X;
		// Top = rootElement.Current.BoundingRectangle.Y;
		Left = rootElement.Current.BoundingRectangle.X * (96.0 / VisualTreeHelper.GetDpi(this).PixelsPerInchX);
		Top = rootElement.Current.BoundingRectangle.Y * (96.0 / VisualTreeHelper.GetDpi(this).PixelsPerInchY);
	}

	static AutomationElement GetRootElementByWindowTitle(string windowTitle)
	{
		return AutomationElement.RootElement.FindFirst(
			TreeScope.Children,
			new PropertyCondition(AutomationElement.NameProperty, windowTitle));
	}
}