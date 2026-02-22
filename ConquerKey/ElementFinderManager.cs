using System.Diagnostics;
using System.IO;
using System.Reflection;
using ConquerKey.ActionHandlers;

namespace ConquerKey;

/// <summary>
/// Manages IElementFinder discovery via reflection.
/// Discovers implementations from the entry assembly and external plugin DLLs.
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
		LoadExternalFinders();
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

	private void LoadExternalFinders()
	{
		var pluginsFolder = GetPluginsFolder();
		if (!Directory.Exists(pluginsFolder)) return;

		foreach (var dllFile in Directory.GetFiles(pluginsFolder, "*.dll"))
		{
			try
			{
				var assembly = Assembly.LoadFrom(dllFile);
				LoadFindersFromAssembly(assembly);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to load finder assembly {dllFile}: {ex.Message}");
			}
		}
	}

	private static string GetPluginsFolder()
	{
		var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		return Path.Combine(userProfile, ".conquerkey", "plugins");
	}
}
