using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace Player
{
    public class BulletManager : BaseBulletManager
    {
        [Header("External Scripts")]
        [SerializeField] private Camera Cam;
        [SerializeField] private PlayerInputActions PlayerInputs;

        [Header("Raycast")]
        [SerializeField] private LayerMask RaycastMask;
        [SerializeField] private ShootType ShootingCalculation;
        private float fireDelay = 1f;
        private bool isFiring = false;

        [Header("Firing Point")]
        [SerializeField] private Transform FirePoint;
        [SerializeField] private GameObject fireballPrefab;

        [Header("Sounds")]
        [SerializeField] private AudioSource ShootingSource;
        [SerializeField] private AudioClip ShootingSound;

        private bool isCharging = false;
        private PlayerStats playerStats;

        public enum ShootType
        {
            Raycast = 0,
            Physics = 1,
        }

        private void Update()
        {
            if (PlayerInputs.toggleFire)
            {
                ToggleShootType();
                PlayerInputs.toggleFire = false; // reset after toggle
            }

            if (PlayerInputs.aim && PlayerInputs.fire && !isCharging)
            {
                OnFirePressed();
            }

            PlayerInputs.fire = false;
        }

        private void OnFirePressed()
        {
            Debug.Log("Shooting Projectile!");
            switch (ShootingCalculation)
            {
                case ShootType.Raycast:
                    if (!isFiring)
                        StartCoroutine(DelayedRaycastShot());
                    break;

                case ShootType.Physics:
                    if (FirePoint != null)
                    {
                        Vector3 shootDirection = Cam.transform.forward;

                        GameObject projectile = Instantiate(fireballPrefab, FirePoint.position, Quaternion.LookRotation(shootDirection));
                        if (projectile.TryGetComponent(out FireballProjectile fireball))
                        {
                            isCharging = true;
                            fireball.Initialize(this, () =>
                            {
                                isCharging = false;
                                Debug.Log("Fireball Recharged! Input re-enabled");
                            });
                        }

                        PlayShootSound();
                    }
                    break;

                default:
                    Debug.LogError("Unexpected Value");
                    break;

            }
        }

        private void DoRaycastShot()
        {
            Debug.Log("Raycasting");

            if (Physics.Raycast(Cam.transform.position, Cam.transform.forward, out RaycastHit hit, Mathf.Infinity, RaycastMask))
            {
                Debug.Log("Raycast Hit!");
                playerStats = GetComponent<PlayerStats>();

                EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
                if (enemy != null)
                {
                    Debug.Log("Enemy hit! Applying Damage.");
                    Vector3 hitDirection = hit.collider.transform.position - Cam.transform.position;
                    enemy.OnDamage((int)playerStats.GetTotalDamage(), hitDirection, transform, 15f);
                }

                OnProjectileCollision(hit.point, hit.normal);
            }
            else
            {
                Debug.Log("Raycast Miss!");
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            if (PlayerInputs.aim)
                Gizmos.DrawLine(Cam.transform.position, Cam.transform.position + Cam.transform.forward * 100);
        }
        private void ToggleShootType()
        {
            ShootingCalculation = ShootingCalculation == ShootType.Raycast ? ShootType.Physics : ShootType.Raycast;
            Debug.Log("Switched Fire Mode to: " + ShootingCalculation);
        }

        private void PlayShootSound()
        {
            if (ShootingSource && ShootingSound)
                ShootingSource.PlayOneShot(ShootingSound);
        }

    private IEnumerator DelayedRaycastShot()
        {
            DoRaycastShot();
            PlayShootSound();
            isFiring = true;
            yield return new WaitForSeconds(fireDelay);


            isFiring = false;
        }
    }
}
