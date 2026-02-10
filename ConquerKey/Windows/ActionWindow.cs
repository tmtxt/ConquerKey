using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ConquerKey.ActionHandlers;
using static ConquerKey.WindowUtilities;

namespace ConquerKey.Windows;

public class ActionWindow : Window
{
	private readonly IConquerKeyPlugin _plugin;
	private readonly AutomationElement _activeWindow;
	private readonly AutomationElementCollection _interactableElements;
	private readonly TextBox _hintTextBox;
	private bool _isClosing;
	private DispatcherTimer? _autoExecuteTimer;

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	/// <summary>
	/// Converts an index to an alphabetic label (0->A, 1->B, ..., 25->Z, 26->AA, 27->AB, etc.)
	/// </summary>
	private static string IndexToAlphaLabel(int index)
	{
		var sb = new StringBuilder();
		do
		{
			sb.Append((char)('A' + (index % 26)));
			index = index / 26 - 1;
		} while (index >= 0);

		// Reverse the string since we built it backwards
		for (var i = 0; i < sb.Length / 2; i++)
		{
			(sb[i], sb[sb.Length - 1 - i]) = (sb[sb.Length - 1 - i], sb[i]);
		}

		return sb.ToString();
	}

	/// <summary>
	/// Increments an alphabetic label to the next label (A->B, Z->AA, AZ->BA, etc.)
	/// </summary>
	private static string IncrementAlphaLabel(string label)
	{
		var chars = label.ToCharArray();

		// Start from the rightmost character and increment
		for (var i = chars.Length - 1; i >= 0; i--)
		{
			if (chars[i] < 'Z')
			{
				// No carry needed, just increment this character
				chars[i]++;
				return new string(chars);
			}

			// Carry over: Z becomes A
			chars[i] = 'A';
		}

		// If we get here, all characters were Z, so we need an extra character
		// e.g., Z -> AA, ZZ -> AAA
		return new string('A', chars.Length + 1);
	}

	/// <summary>
	/// Converts an alphabetic label to an index (A->0, B->1, ..., Z->25, AA->26, AB->27, etc.)
	/// Returns -1 if the input is invalid.
	/// </summary>
	private static int AlphaLabelToIndex(string label)
	{
		if (string.IsNullOrWhiteSpace(label))
			return -1;

		label = label.ToUpperInvariant();
		if (!label.All(c => c >= 'A' && c <= 'Z'))
			return -1;

		var index = 0;
		for (var i = 0; i < label.Length; i++)
		{
			index = index * 26 + (label[i] - 'A' + 1);
		}
		return index - 1;
	}

	public ActionWindow(IConquerKeyPlugin plugin)
	{
		_plugin = plugin;

		var foregroundWindow = GetForegroundWindow();
		_activeWindow = AutomationElement.FromHandle(foregroundWindow);
		_interactableElements = plugin.FindInteractableElements(_activeWindow);

		Activated += ActionWindow_Activated;
		Deactivated += ActionWindow_Deactivated;
		KeyDown += ActionWindow_KeyDown;

		ConfigureWindow();
		AddHintLabels();
		_hintTextBox = AddHintTextBox();
	}

	private void AddHintLabels()
	{
		var label = "A";
		for (var index = 0; index < _interactableElements.Count; index++)
		{
			var clickableElement = _interactableElements[index];
			AddHintLabel(clickableElement, label);
			label = IncrementAlphaLabel(label);
		}

		void AddHintLabel(AutomationElement clickableElement, string label)
		{
			var boundingRect = clickableElement.Current.BoundingRectangle;

			// Skip elements with invalid bounding rectangles
			if (boundingRect.IsEmpty ||
					double.IsNaN(boundingRect.X) || double.IsNaN(boundingRect.Y) ||
					double.IsInfinity(boundingRect.X) || double.IsInfinity(boundingRect.Y))
			{
				return;
			}

			// Create a modern styled border with text
			var border = new Border
			{
				Background = new SolidColorBrush(Color.FromArgb(240, 0, 212, 255)), // Cyan with transparency
				BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204)), // Blue border
				BorderThickness = new Thickness(1.5),
				CornerRadius = new CornerRadius(4),
				Padding = new Thickness(6, 2, 6, 2),
				Effect = new System.Windows.Media.Effects.DropShadowEffect
				{
					Color = Colors.Black,
					Direction = 315,
					ShadowDepth = 2,
					BlurRadius = 6,
					Opacity = 0.5
				}
			};

			var textBlock = new TextBlock
			{
				Text = label,
				FontWeight = FontWeights.Bold,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};

			border.Child = textBlock;

			var x = PixelToDeviceIndependentUnit(this, boundingRect.X - _activeWindow.Current.BoundingRectangle.X, false);
			var y = PixelToDeviceIndependentUnit(this, boundingRect.Y - _activeWindow.Current.BoundingRectangle.Y, true);

			// Skip if calculated positions are invalid
			if (double.IsNaN(x) || double.IsNaN(y) || double.IsInfinity(x) || double.IsInfinity(y))
			{
				return;
			}

			// Set the absolute position
			Canvas.SetLeft(border, x);
			Canvas.SetTop(border, y);

			// Add the Border to the Canvas
			if (Content is Canvas canvas)
			{
				canvas.Children.Add(border);
			}
		}
	}

	private TextBox AddHintTextBox()
	{
		// Modern styled container for the input area
		var inputContainer = new Border
		{
			Background = new SolidColorBrush(Color.FromArgb(250, 30, 30, 30)), // Semi-transparent dark background
			BorderBrush = new SolidColorBrush(Color.FromRgb(0, 122, 204)), // Blue border
			BorderThickness = new Thickness(2),
			CornerRadius = new CornerRadius(8),
			Padding = new Thickness(15, 15, 15, 15),
			Effect = new System.Windows.Media.Effects.DropShadowEffect
			{
				Color = Colors.Black,
				Direction = 90,
				ShadowDepth = 5,
				BlurRadius = 15,
				Opacity = 0.6
			}
		};

		var stackPanel = new StackPanel
		{
			Orientation = Orientation.Horizontal,
			VerticalAlignment = VerticalAlignment.Center
		};

		var hintLabel = new TextBlock
		{
			Text = "⌨ Enter Code:",
			Foreground = new SolidColorBrush(Color.FromRgb(0, 212, 255)), // Cyan
			FontSize = 14,
			FontWeight = FontWeights.SemiBold,
			VerticalAlignment = VerticalAlignment.Center,
			Margin = new Thickness(0, 0, 12, 0)
		};

		var hintTextBox = new TextBox
		{
			FontSize = 16,
			FontWeight = FontWeights.Bold,
			Width = 150,
			Height = 35,
			Padding = new Thickness(10, 5, 10, 5),
			Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
			Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
			BorderBrush = new SolidColorBrush(Color.FromRgb(0, 212, 255)),
			BorderThickness = new Thickness(2),
			CaretBrush = new SolidColorBrush(Color.FromRgb(0, 212, 255)),
			VerticalContentAlignment = VerticalAlignment.Center
		};

		stackPanel.Children.Add(hintLabel);
		stackPanel.Children.Add(hintTextBox);
		inputContainer.Child = stackPanel;

		// Position at bottom center
		Canvas.SetLeft(inputContainer, (Width - 320) / 2);
		Canvas.SetBottom(inputContainer, 20);

		if (Content is Canvas canvas)
		{
			canvas.Children.Add(inputContainer);
		}

		hintTextBox.PreviewTextInput += (s, evt) =>
		{
			// Only allow alphabetic characters
			evt.Handled = !evt.Text.All(c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'));
		};

		hintTextBox.TextChanged += (s, evt) =>
		{
			// Cancel any existing timer
			_autoExecuteTimer?.Stop();
			_autoExecuteTimer = null;

			// Check if the entered code matches a valid element
			var elementIndex = AlphaLabelToIndex(hintTextBox.Text);
			if (elementIndex < 0 || elementIndex >= _interactableElements.Count)
			{
				return;
			}

			// Start a 0.5s timer to auto-execute
			_autoExecuteTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(500)
			};
			_autoExecuteTimer.Tick += (_, _) =>
			{
				_autoExecuteTimer?.Stop();
				ExecuteAction(elementIndex);
			};
			_autoExecuteTimer.Start();
		};

		hintTextBox.KeyDown += (s, evt) =>
		{
			if (evt.Key != Key.Enter) return;

			// Cancel the auto-execute timer since user pressed Enter
			_autoExecuteTimer?.Stop();
			_autoExecuteTimer = null;

			var elementIndex = AlphaLabelToIndex(hintTextBox.Text);
			if (elementIndex < 0 || elementIndex >= _interactableElements.Count)
			{
				return;
			}

			ExecuteAction(elementIndex);
			evt.Handled = true; // Mark the event as handled if necessary
		};

		return hintTextBox;
	}

	private void ConfigureWindow()
	{
		Topmost = true;
		Width = PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Width, false);
		// + 80 for better spacing with the modern input box
		Height = PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Height + 80, true);
		Left = PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Left, false);
		Top = PixelToDeviceIndependentUnit(this, _activeWindow.Current.BoundingRectangle.Top, true);
		WindowStyle = WindowStyle.None;
		AllowsTransparency = true;
		Background = new SolidColorBrush(Color.FromArgb(25, 0, 0, 0)); // Subtle dark overlay

		var canvas = new Canvas();

		// Modern border with gradient effect
		var border = new Border
		{
			BorderBrush = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 1),
				GradientStops = new GradientStopCollection
				{
					new GradientStop(Color.FromRgb(0, 212, 255), 0.0),  // Cyan
					new GradientStop(Color.FromRgb(0, 122, 204), 0.5),  // Blue
					new GradientStop(Color.FromRgb(104, 33, 122), 1.0)  // Purple
				}
			},
			BorderThickness = new Thickness(3),
			CornerRadius = new CornerRadius(8),
			Effect = new System.Windows.Media.Effects.DropShadowEffect
			{
				Color = Color.FromRgb(0, 212, 255),
				Direction = 0,
				ShadowDepth = 0,
				BlurRadius = 20,
				Opacity = 0.7
			}
		};
		canvas.Children.Add(border);
		Content = canvas;
	}

	private void ActionWindow_Activated(object? sender, EventArgs e)
	{
		_hintTextBox.Focus();
	}

	private void ActionWindow_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key != Key.Escape) return;

		_isClosing = true;
		Close();
		e.Handled = true; // Mark the event as handled
	}

	private void ActionWindow_Deactivated(object? sender, EventArgs e)
	{
		if (_isClosing) return;

		_isClosing = true;
		Close();
	}

	private void ExecuteAction(int elementIndex)
	{
		_isClosing = true;
		Close();

		var uiElement = _interactableElements[elementIndex];
		if (uiElement != null)
		{
			_plugin.Interact(_activeWindow, uiElement);
		}
	}
}