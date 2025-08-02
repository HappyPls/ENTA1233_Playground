using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class BaseBulletManager : MonoBehaviour
{
    [Header("Bullets")]
    [SerializeField] private FireballProjectile FireballPrefab;
    [SerializeField] private PhysicsBullet BulletPrefab;

    [Header("Particle")]
    [SerializeField] private RaycastBullet BulletParticle;

    

    protected void SpawnFireball(Transform firePoint, Vector3 direction)
    {
        if (FireballPrefab == null)
        {
            Debug.LogWarning("PhysicsBulletPrefab not assigned!");
            return;
        }

        FireballProjectile spawnedFireball = Instantiate(
            FireballPrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        spawnedFireball.Initialize(this, null);
    }

    protected void SpawnPhysicsBullet(Transform firePoint, Vector3 direction)
    {
        if (BulletPrefab == null)
        {
            Debug.LogWarning("PhysicsBullet Prefab not assigned!");
                return;
        }

        PhysicsBullet spawnedBullet = Instantiate(
            BulletPrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
            );

        spawnedBullet.Initialize(this, null);

    }

    protected void OnProjectileCollision(Vector3 position, Vector3 rotation)
    {
        SpawnParticle(position, rotation);
    }

    private void SpawnParticle(Vector3 position, Vector3 rotation)
    {
        if (BulletParticle != null)
        {
            Quaternion lookRotation = Quaternion.LookRotation(rotation);
            Instantiate(BulletParticle, position, lookRotation);
        }
    }

}
