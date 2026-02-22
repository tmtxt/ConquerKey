using System.Windows.Input;
using ConquerKey.ActionHandlers;
using ConquerKey.Configuration;

namespace ConquerKey.Test.Configuration;

[TestFixture]
public class KeyBindingParserTests
{
	[Test]
	public void Parse_SingleModifierAndKey_ReturnsCorrectBinding()
	{
		var result = KeyBindingParser.Parse("Win+F");

		Assert.That(result.Modifiers, Is.EqualTo(ModifierKeys.Windows));
		Assert.That(result.Key, Is.EqualTo(Key.F));
	}

	[Test]
	public void Parse_MultipleModifiers_ReturnsAllModifiers()
	{
		var result = KeyBindingParser.Parse("Ctrl+Shift+G");

		Assert.That(result.Modifiers, Is.EqualTo(ModifierKeys.Control | ModifierKeys.Shift));
		Assert.That(result.Key, Is.EqualTo(Key.G));
	}

	[Test]
	public void Parse_AllFourModifiers_ReturnsAllModifiers()
	{
		var result = KeyBindingParser.Parse("Ctrl+Alt+Shift+Win+F1");

		Assert.That(result.Modifiers, Is.EqualTo(
			ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift | ModifierKeys.Windows));
		Assert.That(result.Key, Is.EqualTo(Key.F1));
	}

	[Test]
	public void Parse_KeyOnly_ReturnsNoModifiers()
	{
		var result = KeyBindingParser.Parse("A");

		Assert.That(result.Modifiers, Is.EqualTo(ModifierKeys.None));
		Assert.That(result.Key, Is.EqualTo(Key.A));
	}

	[Test]
	public void Parse_ControlAlternateSpelling_Works()
	{
		var result = KeyBindingParser.Parse("Control+A");

		Assert.That(result.Modifiers, Is.EqualTo(ModifierKeys.Control));
		Assert.That(result.Key, Is.EqualTo(Key.A));
	}

	[Test]
	public void Parse_WindowsAlternateSpelling_Works()
	{
		var result = KeyBindingParser.Parse("Windows+F");

		Assert.That(result.Modifiers, Is.EqualTo(ModifierKeys.Windows));
		Assert.That(result.Key, Is.EqualTo(Key.F));
	}

	[Test]
	public void Parse_CaseInsensitive_Works()
	{
		var result = KeyBindingParser.Parse("win+shift+f");

		Assert.That(result.Modifiers, Is.EqualTo(ModifierKeys.Windows | ModifierKeys.Shift));
		Assert.That(result.Key, Is.EqualTo(Key.F));
	}

	[Test]
	public void Parse_WithSpaces_TrimsCorrectly()
	{
		var result = KeyBindingParser.Parse("Win + Shift + G");

		Assert.That(result.Modifiers, Is.EqualTo(ModifierKeys.Windows | ModifierKeys.Shift));
		Assert.That(result.Key, Is.EqualTo(Key.G));
	}

	[Test]
	public void Parse_UnknownModifier_ThrowsFormatException()
	{
		Assert.Throws<FormatException>(() => KeyBindingParser.Parse("Super+F"));
	}

	[Test]
	public void Parse_UnknownKey_ThrowsFormatException()
	{
		Assert.Throws<FormatException>(() => KeyBindingParser.Parse("Win+NotAKey"));
	}

	[Test]
	public void Parse_EmptyString_ThrowsFormatException()
	{
		Assert.Throws<FormatException>(() => KeyBindingParser.Parse(""));
	}

	[Test]
	public void ToString_SingleModifier_FormatsCorrectly()
	{
		var binding = new ActionKeyBinding(ModifierKeys.Windows, Key.F);

		var result = KeyBindingParser.ToString(binding);

		Assert.That(result, Is.EqualTo("Win+F"));
	}

	[Test]
	public void ToString_MultipleModifiers_FormatsCorrectly()
	{
		var binding = new ActionKeyBinding(ModifierKeys.Control | ModifierKeys.Shift, Key.G);

		var result = KeyBindingParser.ToString(binding);

		Assert.That(result, Is.EqualTo("Ctrl+Shift+G"));
	}

	[Test]
	public void ToString_NoModifiers_ReturnsKeyOnly()
	{
		var binding = new ActionKeyBinding(ModifierKeys.None, Key.A);

		var result = KeyBindingParser.ToString(binding);

		Assert.That(result, Is.EqualTo("A"));
	}

	[Test]
	public void RoundTrip_ParseThenToString_PreservesBinding()
	{
		// ToString outputs modifiers in canonical order: Ctrl, Alt, Shift, Win
		var original = "Shift+Win+G";
		var parsed = KeyBindingParser.Parse(original);
		var result = KeyBindingParser.ToString(parsed);

		Assert.That(result, Is.EqualTo("Shift+Win+G"));
	}

	[Test]
	public void RoundTrip_AllModifiers_PreservesBinding()
	{
		var original = "Ctrl+Alt+Shift+Win+F1";
		var parsed = KeyBindingParser.Parse(original);
		var result = KeyBindingParser.ToString(parsed);

		Assert.That(result, Is.EqualTo(original));
	}
}
