using UnityEngine;

[CreateAssetMenu(fileName = "Particle Data", menuName = "Abilities/Particle Data", order = 0)]
public class ParticleData : ScriptableObject
{
	public GameObject castVFXObject; // The object that will cast the particle effect
	public Vector3 castVFXScale = Vector3.one; // Scale of the cast VFX
	public float castTime = 0.25f;
	public GameObject particleObject;
	public GameObject impactObject;
	public float projectileSpeed = 10f; // Speed of the projectile
	public float maxLifeTime = 5f; // How long the projectile lasts before being destroyed
	public ProjectileManager.ProjectilePath projectilePath = ProjectileManager.ProjectilePath.STRAIGHT; // Default path type
	public string projectileSpawnPoint = "rightHand"; // Default spawn point
}
