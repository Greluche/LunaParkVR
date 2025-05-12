 using UnityEngine;

public class Shooter : MonoBehaviour
{
   

/// <summary>
/// Class <c>Shooter</c> shoots a bullet when the attached
/// XRGrabInteractable is activated.
/// </summary>

    [SerializeField, Tooltip("Where to spawn the bullet")] 
    private Transform muzzle;
    [SerializeField, Tooltip("The bullet to spawn")] 
    private GameObject bulletPrefab;
    [SerializeField, Tooltip("The force at which the bullet will be shot")] 
    private float force = 10f;

    /// <summary>
    /// Method <c>Shoot</c> shoots the bullet prefab at a 
    /// certain force, all specified by this <c>Pistol</c>.
    /// </summary>
    public void Shoot()
    {
        var bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        var rb = bullet.GetComponent<Rigidbody>();
        rb?.AddForce(transform.forward * (10 * force), ForceMode.Impulse);
    }

}
