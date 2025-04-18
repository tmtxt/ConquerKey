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
using Condition = System.Windows.Automation.Condition;

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
		string windowTitle =
			"CargoWise Next - ediProd - Branch: Sydney Aust Branch - Company: WiseTech Global (Australia) Pty Ltd - Department: Development";
		var rootElement = GetRootElementByWindowTitle(windowTitle);
		rootElement.SetFocus();

		// Add your logic here
		Width = rootElement.Current.BoundingRectangle.Width * (96.0 / VisualTreeHelper.GetDpi(this).PixelsPerInchX);
		// Width = rootElement.Current.BoundingRectangle.Width;
		Height = (rootElement.Current.BoundingRectangle.Height + 30) * (96.0 / VisualTreeHelper.GetDpi(this).PixelsPerInchY);
		// Left = rootElement.Current.BoundingRectangle.X;
		// Top = rootElement.Current.BoundingRectangle.Y;
		Left = rootElement.Current.BoundingRectangle.X * (96.0 / VisualTreeHelper.GetDpi(this).PixelsPerInchX);
		Top = rootElement.Current.BoundingRectangle.Y * (96.0 / VisualTreeHelper.GetDpi(this).PixelsPerInchY);

		AutomationElementCollection clickableElements = FindClickableElements(rootElement);
		for (var index = 0; index < clickableElements.Count; index++)
		{
			var clickableElement = clickableElements[index];
			AddHintText(clickableElement, index, rootElement);
		}

		var textBox = new TextBox
		{
			//Text = "",
			Width = 200,
			Height = 30,
			Margin = new Thickness(10),
			Padding = new Thickness(5),
			Background = Brushes.LightGray,
			Foreground = Brushes.Black
		};
		Canvas.SetLeft(textBox, 0); // X-coordinate
		Canvas.SetTop(textBox, Height - 30); // Y-coordinate
		// Add the TextBlock to the Canvas
		if (Content is Canvas canvas)
		{
			canvas.Children.Add(textBox);
		}

		textBox.PreviewTextInput += (s, evt) =>
		{
			evt.Handled = !int.TryParse(evt.Text, out _);
		};

		Activate();
		textBox.Focus();
	}

	private void AddHintText(AutomationElement clickableElement, int index, AutomationElement rootElement)
	{
		// Add your logic here
		var textBlock = new TextBlock
		{
			Text = index.ToString(),
			Foreground = Brushes.Black,
			FontSize = 12,
			Background = Brushes.Aqua,
			Margin = new Thickness(0),
			Padding = new Thickness(0),
		};

		var x = clickableElement.Current.BoundingRectangle.X - rootElement.Current.BoundingRectangle.X;
		var y = clickableElement.Current.BoundingRectangle.Y - rootElement.Current.BoundingRectangle.Y;

		// Set the absolute position
		Canvas.SetLeft(textBlock, x); // X-coordinate
		Canvas.SetTop(textBlock, y); // Y-coordinate

		// Add the TextBlock to the Canvas
		if (Content is Canvas canvas)
		{
			canvas.Children.Add(textBlock);
		}
	}

	static AutomationElement GetRootElementByWindowTitle(string windowTitle)
	{
		return AutomationElement.RootElement.FindFirst(
			TreeScope.Children,
			new PropertyCondition(AutomationElement.NameProperty, windowTitle));
	}

	private AutomationElementCollection FindClickableElements(AutomationElement rootElement)
	{
		// Define a condition to find elements that are clickable
		var clickableCondition = new OrCondition(
			new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
			new PropertyCondition(AutomationElement.IsInvokePatternAvailableProperty, true)
		);
		var visibleCondition = new PropertyCondition(AutomationElement.IsOffscreenProperty, false);
		var finalCondition = new AndCondition(
			clickableCondition,
			visibleCondition
			);
		// var clickableCondition = new PropertyCondition(AutomationElement.IsOffscreenProperty, false);

		// Find all matching elements
		var clickableElements = rootElement.FindAll(TreeScope.Descendants, finalCondition);

		return clickableElements;
	}
}