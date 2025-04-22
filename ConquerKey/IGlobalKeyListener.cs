namespace ConquerKey;

public interface IGlobalKeyListener : IDisposable
{
	void StartListening();
	void StopListening();
}