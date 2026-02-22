using System.Diagnostics;
using System.Reflection;
using ConquerKey.ActionHandlers;

namespace ConquerKey;

/// <summary>
/// Manages IElementFinder discovery via reflection.
/// Discovers implementations from the entry assembly.
/// Does not include a default fallback -- each action provides its own via DefaultElementFinder.
/// </summary>
public class ElementFinderManager
{
	private readonly List<IElementFinder> _finders = new();

	/// <summary>
	/// The discovered element finders (not including action-specific defaults).
	/// </summary>
	public IReadOnlyList<IElementFinder> Finders => _finders;

	/// <summary>
	/// Discovers and loads all IElementFinder implementations.
	/// </summary>
	public void LoadFinders()
	{
		LoadFindersFromAssembly(Assembly.GetEntryAssembly());
	}

	private void LoadFindersFromAssembly(Assembly? assembly)
	{
		if (assembly == null) return;

		try
		{
			var finderTypes = assembly.GetTypes()
				.Where(t => typeof(IElementFinder).IsAssignableFrom(t)
					&& t.IsClass
					&& !t.IsAbstract
					&& t.IsPublic);

			foreach (var finderType in finderTypes)
			{
				try
				{
					if (Activator.CreateInstance(finderType) is IElementFinder finder)
						_finders.Add(finder);
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Failed to create IElementFinder of type {finderType.Name}: {ex.Message}");
				}
			}
		}
		catch (ReflectionTypeLoadException ex)
		{
			Debug.WriteLine($"Failed to load types from assembly {assembly.FullName}: {ex.Message}");
		}
	}
}
