using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class BulletManager : MonoBehaviour
    {
        [SerializeField] private Camera Cam;
        //[SerializeField] private GameObject Player;
        //[SerializeField] private GameObject Barrel;

        [SerializeField] private GameObject BulletPrefab;

        [SerializeField] private PlayerInputActions PlayerInputs;

        private void Update()
        {
            if (PlayerInputs.Aim && PlayerInputs.Fire)
            {
                OnFirePressed();
            }
            PlayerInputs.Fire = false;
        }

        private void OnFirePressed()
        {
            Vector3 direction = Cam.transform.forward;

            Instantiate(BulletPrefab, Cam.transform.position, Cam.transform.rotation);
        }
    }
}
