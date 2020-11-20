/// <summary>
///   Interface definition that use Agora RTC SDK as a clinet.	
/// </summary>
public interface IVideoChatClient
{
    void Join(string channel);
    void Leave();
    void LoadEngine(string appId);
    void UnloadEngine();
    void OnSceneLoaded();
    void EnableVideo(bool enable);
    event System.Action OnViewControllerFinish;
}
