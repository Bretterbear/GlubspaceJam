namespace GlubspaceJam.Scripts;

public interface IDynamicReceiver
{
    public void ProvidePower();
    public void StopPower();
    public bool IsOn();
    public void Inverted();

    public void DynamicsSetup();
}