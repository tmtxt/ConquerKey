using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using ConquerKey.ActionHandlers;
using ConquerKey.Configuration;

namespace ConquerKey;

/// <summary>
/// Manages action discovery and registration.
/// Scans the entry assembly for built-in actions and loads external actions from ~/.conquerkey/plugins.
/// Resolves effective key bindings by applying user overrides from ~/.conquerkey.json.
/// </summary>
public class ActionManager
{
	private readonly Dictionary<string, IConquerKeyAction> _actionsByName = new();
	private readonly Dictionary<ActionKeyBinding, IConquerKeyAction> _resolvedBindings = new();
	private readonly ConfigManager _configManager;

	public ActionManager(ConfigManager configManager)
	{
		_configManager = configManager;
	}

	/// <summary>
	/// Gets all registered actions.
	/// </summary>
	public IReadOnlyCollection<IConquerKeyAction> Actions => _actionsByName.Values;

	/// <summary>
	/// Gets all resolved key bindings and their associated actions.
	/// </summary>
	public IReadOnlyDictionary<ActionKeyBinding, IConquerKeyAction> ActionsByKeyBinding => _resolvedBindings;

	/// <summary>
	/// Discovers and loads all actions, then resolves effective key bindings.
	/// </summary>
	public void LoadActions()
	{
		LoadActionsFromAssembly(Assembly.GetEntryAssembly());
		LoadExternalActions();
		ResolveKeyBindings();
	}

	/// <summary>
	/// Tries to get an action by its resolved key binding.
	/// </summary>
	public bool TryGetAction(ModifierKeys modifiers, Key key, out IConquerKeyAction? action)
	{
		var keyBinding = new ActionKeyBinding(modifiers, key);
		return _resolvedBindings.TryGetValue(keyBinding, out action);
	}

	private void RegisterAction(IConquerKeyAction action)
	{
		_actionsByName[action.Name] = action;
	}

	private void ResolveKeyBindings()
	{
		_resolvedBindings.Clear();
		foreach (var action in _actionsByName.Values)
		{
			var effectiveBinding = action.DefaultKeyBinding;

			if (_configManager.TryGetActionConfig(action.Name, out var actionConfig) && actionConfig != null)
			{
				if (!actionConfig.Enabled)
					continue;

				if (!string.IsNullOrWhiteSpace(actionConfig.KeyBinding))
				{
					try
					{
						effectiveBinding = KeyBindingParser.Parse(actionConfig.KeyBinding);
					}
					catch (FormatException ex)
					{
						Debug.WriteLine($"Invalid key binding for action '{action.Name}': {ex.Message}. Using default.");
					}
				}
			}

			_resolvedBindings[effectiveBinding] = action;
		}
	}

	private void LoadActionsFromAssembly(Assembly? assembly)
	{
		if (assembly == null) return;

		try
		{
			var actionTypes = assembly.GetTypes()
				.Where(t => typeof(IConquerKeyAction).IsAssignableFrom(t)
					&& t.IsClass
					&& !t.IsAbstract);

			foreach (var actionType in actionTypes)
			{
				try
				{
					if (Activator.CreateInstance(actionType) is IConquerKeyAction action)
					{
						RegisterAction(action);
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Failed to create action instance of type {actionType.Name}: {ex.Message}");
				}
			}
		}
		catch (ReflectionTypeLoadException ex)
		{
			Debug.WriteLine($"Failed to load some types from assembly {assembly.FullName}: {ex.Message}");
		}
	}

	private void LoadExternalActions()
	{
		var pluginsFolder = GetPluginsFolder();

		if (!Directory.Exists(pluginsFolder))
		{
			try
			{
				Directory.CreateDirectory(pluginsFolder);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to create plugins folder: {ex.Message}");
			}
			return;
		}

		var dllFiles = Directory.GetFiles(pluginsFolder, "*.dll");

		foreach (var dllFile in dllFiles)
		{
			try
			{
				var assembly = Assembly.LoadFrom(dllFile);
				LoadActionsFromAssembly(assembly);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to load action assembly {dllFile}: {ex.Message}");
			}
		}
	}

	private static string GetPluginsFolder()
	{
		var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		return Path.Combine(userProfile, ".conquerkey", "plugins");
	}
}
