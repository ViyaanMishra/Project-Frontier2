using UnityEngine;
using System.Collections.Generic;

namespace Frontier.Combat
{
    /// <summary>
    /// Weapon manager for equip/swap/holster logic.
    /// Supports 30+ weapons across melee, ranged, heavy, and throwable categories.
    /// </summary>
    public class WeaponManager : MonoBehaviour
    {
        [System.Serializable]
        public struct WeaponSlot
        {
            public WeaponData weapon;
            public int ammoCount;
            public int maxAmmo;
            public float durability;
            public bool isEquipped;
        }

        public enum WeaponCategory
        {
            Melee, Pistol, Primary, Heavy, Throwable, Special
        }

        [Header("Weapon Slots")]
        public WeaponSlot meleeSlot;
        public WeaponSlot pistolSlot;
        public WeaponSlot primarySlot;
        public WeaponSlot heavySlot;
        public List<WeaponSlot> throwableSlots = new List<WeaponSlot>();

        [Header("Settings")]
        public float swapCooldown = 0.5f;
        public float holsterPositionOffset = 0.5f;
        
        [Header("Transforms")]
        public Transform rightHand;
        public Transform leftHand;
        public Transform holsterBack;
        public Transform holsterHip;

        private float _swapTimer;
        private WeaponSlot _currentWeapon;
        private bool _isSwapping;
        private GameObject _currentWeaponModel;

        public WeaponSlot CurrentWeapon => _currentWeapon;
        public bool CanFire => _swapTimer <= 0 && !_isSwapping && _currentWeapon.weapon != null;

        private void Update()
        {
            if (_swapTimer > 0)
                _swapTimer -= Time.deltaTime;

            HandleWeaponInput();
        }

        private void HandleWeaponInput()
        {
            // Weapon swap keys
            if (Input.GetKeyDown(KeyCode.Alpha1)) EquipPrimary();
            if (Input.GetKeyDown(KeyCode.Alpha2)) EquipPistol();
            if (Input.GetKeyDown(KeyCode.Alpha3)) EquipMelee();
            if (Input.GetKeyDown(KeyCode.Alpha4)) EquipHeavy();
            if (Input.GetKeyDown(KeyCode.G)) ThrowThrowable();
            
            // Reload
            if (Input.GetKeyDown(KeyCode.R) && CanFire)
            {
                Reload();
            }
        }

        #region Equip Methods

        public void EquipPrimary()
        {
            if (primarySlot.weapon == null || _swapTimer > 0) return;
            EquipWeapon(ref primarySlot);
        }

        public void EquipPistol()
        {
            if (pistolSlot.weapon == null || _swapTimer > 0) return;
            EquipWeapon(ref pistolSlot);
        }

        public void EquipMelee()
        {
            if (meleeSlot.weapon == null || _swapTimer > 0) return;
            EquipWeapon(ref meleeSlot);
        }

        public void EquipHeavy()
        {
            if (heavySlot.weapon == null || _swapTimer > 0) return;
            EquipWeapon(ref heavySlot);
        }

        private void EquipWeapon(ref WeaponSlot slot)
        {
            if (_isSwapping) return;

            _isSwapping = true;
            
            // Unequip current
            if (_currentWeapon.weapon != null)
            {
                _currentWeapon.isEquipped = false;
                HolsterWeapon(_currentWeapon);
            }

            // Equip new
            _currentWeapon = slot;
            slot.isEquipped = true;
            _swapTimer = swapCooldown;

            // Spawn/show weapon model
            ShowWeaponModel(slot.weapon);

            _isSwapping = false;
        }

        private void HolsterWeapon(WeaponSlot slot)
        {
            if (_currentWeaponModel != null)
            {
                _currentWeaponModel.SetActive(false);
            }
        }

        private void ShowWeaponModel(WeaponData weapon)
        {
            if (weapon.prefab != null)
            {
                if (_currentWeaponModel != null)
                    Destroy(_currentWeaponModel);

                _currentWeaponModel = Instantiate(weapon.prefab, rightHand);
                _currentWeaponModel.transform.localPosition = Vector3.zero;
                _currentWeaponModel.transform.localRotation = Quaternion.identity;
            }
        }

        #endregion

        #region Combat Actions

        public void Fire()
        {
            if (!CanFire) return;

            var weapon = _currentWeapon.weapon;
            
            switch (weapon.fireMode)
            {
                case FireMode.SemiAuto:
                    FireSingle();
                    break;
                case FireMode.Auto:
                    // Handled by input hold
                    break;
                case FireMode.Burst:
                    FireBurst(weapon.burstCount);
                    break;
            }
        }

        public void FireSingle()
        {
            if (_currentWeapon.ammoCount <= 0)
            {
                PlayEmptyClick();
                return;
            }

            _currentWeapon.ammoCount--;
            
            // Trigger attack animation
            var animator = GetComponent<Animator>();
            if (animator != null)
                animator.SetTrigger("Fire");

            // Spawn projectile or hitscan
            if (_currentWeapon.weapon.isHitscan)
            {
                PerformHitscan();
            }
            else
            {
                SpawnProjectile();
            }
        }

        public void FireBurst(int burstCount)
        {
            StartCoroutine(BurstCoroutine(burstCount));
        }

        private System.Collections.IEnumerator BurstCoroutine(int burstCount)
        {
            for (int i = 0; i < burstCount; i++)
            {
                FireSingle();
                yield return new WaitForSeconds(_currentWeapon.weapon.fireRate);
            }
        }

        public void Reload()
        {
            if (_currentWeapon.weapon == null || _currentWeapon.ammoCount >= _currentWeapon.maxAmmo)
                return;

            // Play reload animation
            var animator = GetComponent<Animator>();
            if (animator != null)
                animator.SetTrigger("Reload");

            // Reload after animation
            Invoke(nameof(CompleteReload), _currentWeapon.weapon.reloadTime);
        }

        private void CompleteReload()
        {
            int needed = _currentWeapon.maxAmmo - _currentWeapon.ammoCount;
            // Deduct from inventory (would integrate with inventory system)
            _currentWeapon.ammoCount = _currentWeapon.maxAmmo;
        }

        public void ThrowThrowable()
        {
            foreach (var slot in throwableSlots)
            {
                if (slot.weapon != null && slot.weapon.category == WeaponCategory.Throwable)
                {
                    // Throw logic
                    slot.ammoCount--;
                    if (slot.ammoCount <= 0)
                    {
                        throwableSlots.Remove(slot);
                    }
                    break;
                }
            }
        }

        #endregion

        #region Helpers

        private void PerformHitscan()
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, _currentWeapon.weapon.effectiveRange))
            {
                // Apply damage
                var target = hit.transform.GetComponent<IDamageable>();
                if (target != null)
                {
                    target.TakeDamage(_currentWeapon.weapon.damage, hit.point);
                }
            }
        }

        private void SpawnProjectile()
        {
            // Would spawn projectile prefab from weapon data
        }

        private void PlayEmptyClick()
        {
            // Play empty click sound
        }

        public interface IDamageable
        {
            void TakeDamage(float damage, Vector3 hitPoint);
        }

        #endregion
    }

    [System.Serializable]
    public class WeaponData
    {
        public string displayName;
        public WeaponManager.WeaponCategory category;
        public GameObject prefab;
        public float damage;
        public float fireRate;
        public float effectiveRange;
        public float reloadTime;
        public bool isHitscan;
        public FireMode fireMode;
        public int burstCount;
        public float spread;
        public float recoil;
    }

    public enum FireMode
    {
        SemiAuto, Auto, Burst, Charge
    }
}
