namespace Main.Scripts.Scenarios
{
public interface Scenario
{
    public void Start();
    public void Stop();
    public void Update();
    public ScenarioStatus GetStatus();
}
}