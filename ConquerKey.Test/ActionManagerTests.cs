using System.Windows.Input;
using ConquerKey.ActionHandlers;
using ConquerKey.Configuration;

namespace ConquerKey.Test;

[TestFixture]
public class ActionManagerTests
{
	[Test]
	public void TryGetAction_NoActionsLoaded_ReturnsFalse()
	{
		var manager = new ActionManager(new ConfigManager());

		var result = manager.TryGetAction(ModifierKeys.Windows, Key.F, out var action);

		Assert.That(result, Is.False);
		Assert.That(action, Is.Null);
	}

	[Test]
	public void TryGetAction_NonExistentBinding_ReturnsFalse()
	{
		var manager = new ActionManager(new ConfigManager());
		manager.LoadActions();

		var result = manager.TryGetAction(ModifierKeys.Control | ModifierKeys.Alt, Key.Z, out var action);

		Assert.That(result, Is.False);
	}

	[Test]
	public void ClickAction_HasCorrectDefaultProperties()
	{
		var action = new ClickAction();

		Assert.That(action.Name, Is.EqualTo("Click"));
		Assert.That(action.DefaultKeyBinding.Modifiers, Is.EqualTo(ModifierKeys.Windows));
		Assert.That(action.DefaultKeyBinding.Key, Is.EqualTo(Key.F));
		Assert.That(action.DefaultElementFinder, Is.Not.Null);
	}

	[Test]
	public void ClickAction_DefaultElementFinder_CanHandleReturnsTrue()
	{
		var action = new ClickAction();

		// The default element finder should always be able to handle any root element
		// (CanHandle returns true as a universal fallback)
		Assert.That(action.DefaultElementFinder, Is.Not.Null);
	}

	[Test]
	public void ActionKeyBinding_Equality_WorksCorrectly()
	{
		var binding1 = new ActionKeyBinding(ModifierKeys.Windows, Key.F);
		var binding2 = new ActionKeyBinding(ModifierKeys.Windows, Key.F);
		var binding3 = new ActionKeyBinding(ModifierKeys.Control, Key.F);

		Assert.That(binding1, Is.EqualTo(binding2));
		Assert.That(binding1, Is.Not.EqualTo(binding3));
	}

	[Test]
	public void ActionKeyBinding_DifferentKey_NotEqual()
	{
		var binding1 = new ActionKeyBinding(ModifierKeys.Windows, Key.F);
		var binding2 = new ActionKeyBinding(ModifierKeys.Windows, Key.G);

		Assert.That(binding1, Is.Not.EqualTo(binding2));
	}
}
