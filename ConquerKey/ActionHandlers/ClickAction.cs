using System.Windows.Automation;
using System.Windows.Input;

namespace ConquerKey.ActionHandlers;

/// <summary>
/// Action that enables clicking on UI elements using keyboard navigation.
/// Default key binding: Win + F.
/// </summary>
public class ClickAction : IConquerKeyAction
{
	public string Name => "Click";

	public ActionKeyBinding DefaultKeyBinding => new(ModifierKeys.Windows, Key.F);

	public IElementFinder DefaultElementFinder { get; } = new ClickDefaultElementFinder();

	public void Interact(AutomationElement window, AutomationElement element)
	{
		var rect = element.Current.BoundingRectangle;
		var x = (int)(rect.X + rect.Width / 2);
		var y = (int)(rect.Y + rect.Height / 2);

		WindowUtilities.SendMouseClick(x, y);
	}

	/// <summary>
	/// Default element finder for the Click action.
	/// Discovers common interactable UI control types (buttons, links, inputs, etc.).
	/// Uses depth-limited TreeWalker traversal with offscreen subtree pruning for performance.
	/// </summary>
	private class ClickDefaultElementFinder : IElementFinder
	{
		private const int MaxDepth = 25;

		private static readonly HashSet<ControlType> TargetControlTypes =
		[
			ControlType.Button,
			ControlType.Hyperlink,
			ControlType.Tab,
			ControlType.TabItem,
			ControlType.CheckBox,
			ControlType.RadioButton,
			ControlType.Edit,
			ControlType.ComboBox,
			ControlType.ListItem,
			ControlType.DataItem,
			ControlType.MenuItem,
			ControlType.List,
			ControlType.Menu,
			ControlType.TreeItem,
			ControlType.Table,
			ControlType.DataGrid
		];

		public bool CanHandle(AutomationElement rootElement) => true;

		public IReadOnlyList<AutomationElement> FindElements(AutomationElement window, AutomationElement element)
		{
			var results = new List<AutomationElement>();
			WalkTree(element, results, 0);
			return results;
		}

		private static void WalkTree(AutomationElement parent, List<AutomationElement> results, int depth)
		{
			if (depth > MaxDepth) return;

			var walker = TreeWalker.ControlViewWalker;
			AutomationElement? child;

			try
			{
				child = walker.GetFirstChild(parent);
			}
			catch
			{
				return;
			}

			while (child != null)
			{
				try
				{
					var current = child.Current;

					// Skip offscreen elements and their entire subtree
					if (current.IsOffscreen)
					{
						child = walker.GetNextSibling(child);
						continue;
					}

					if (TargetControlTypes.Contains(current.ControlType))
					{
						results.Add(child);
					}

					WalkTree(child, results, depth + 1);
				}
				catch
				{
					// Element may have become stale during traversal
				}

				try
				{
					child = walker.GetNextSibling(child);
				}
				catch
				{
					break;
				}
			}
		}
	}
}
