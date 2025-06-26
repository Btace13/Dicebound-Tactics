public interface IIconUpdater
{
    public void RegisterIconUpdater(IIconUpdater updater)
    {
        InputManager.Instance?.RegisterIconUpdater(this);
    }

    public void UnregisterIconUpdater(IIconUpdater updater)
    {
        InputManager.Instance?.UnregisterIconUpdater(this);
    }

    public void UpdateIcon();
}
