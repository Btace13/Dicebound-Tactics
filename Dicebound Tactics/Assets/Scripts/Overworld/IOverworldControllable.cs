using UnityEngine;


public interface IOverworldControllable
{
    public bool IsControllable { get; set; }
    public bool IsControlled { get; set; }
    public OverworldCharacterController OverworldCharacterController { get; set; }
}
