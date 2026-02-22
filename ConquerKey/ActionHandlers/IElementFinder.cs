using System.Windows.Automation;

namespace ConquerKey.ActionHandlers;

/// <summary>
/// Interface for finding UI elements within a window.
/// </summary>
public interface IElementFinder
{
  /// <summary>
  /// Determines whether this finder can handle the given root element.
  /// </summary>
  /// <param name="rootElement">The root element to check.</param>
  /// <returns>True if this finder can handle the element; otherwise, false.</returns>
  bool CanHandle(AutomationElement rootElement);

  /// <summary>
  /// Finds UI elements within the specified element.
  /// </summary>
  /// <param name="window">The parent window containing the element.</param>
  /// <param name="element">The element to search within.</param>
  /// <returns>A collection of automation elements.</returns>
  AutomationElementCollection FindElements(AutomationElement window, AutomationElement element);
}
