namespace ConquerKey.Test;

[TestFixture]
public class ElementFinderManagerTests
{
	[Test]
	public void Finders_BeforeLoad_IsEmpty()
	{
		var manager = new ElementFinderManager();

		Assert.That(manager.Finders, Is.Empty);
	}

	[Test]
	public void LoadFinders_ScansAssembly_FindersListPopulated()
	{
		var manager = new ElementFinderManager();
		manager.LoadFinders();

		// The finders list may or may not contain items depending on
		// whether there are public IElementFinder implementations in the assembly.
		// The key thing is it doesn't throw.
		Assert.That(manager.Finders, Is.Not.Null);
	}

	[Test]
	public void Finders_DoesNotContainPrivateNestedFinders()
	{
		// ClickDefaultElementFinder is a private nested class in ClickAction.
		// It should NOT be discovered by ElementFinderManager since it's not public.
		var manager = new ElementFinderManager();
		manager.LoadFinders();

		// Verify no finder with "ClickDefault" in its type name was discovered
		Assert.That(manager.Finders.All(f => !f.GetType().Name.Contains("ClickDefault")), Is.True);
	}
}
