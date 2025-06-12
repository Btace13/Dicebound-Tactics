public interface ISelectable
{
	public bool IsSelected { get; set; }

	public abstract void OnSelect();

	public abstract void OnDeselect();
}
