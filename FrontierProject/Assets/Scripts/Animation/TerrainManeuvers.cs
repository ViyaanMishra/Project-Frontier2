using UnityEngine;

namespace Frontier.Animation
{
    /// <summary>
    /// Terrain maneuvers: dodge-roll, vault, slide, wall-press.
    /// Context-sensitive movement animations.
    /// </summary>
    public class TerrainManeuvers : MonoBehaviour
    {
        private Animator _animator;
        private CharacterController _controller;

        [Header("Parameters")]
        public string isSlidingParam = "IsSliding";
        public string isVaultingParam = "IsVaulting";
        public string isRollingParam = "IsRolling";
        public string wallPressParam = "WallPress";

        [Header("Settings")]
        public float slideSpeed = 8f;
        public float slideDuration = 1.5f;
        public float rollDuration = 0.8f;
        public float vaultHeight = 1.2f;
        public float wallPressDistance = 0.5f;

        [Header("State")]
        public bool isSliding;
        public bool isVaulting;
        public bool isRolling;
        public bool isWallPressed;

        private float _slideTimer;
        private Vector3 _slideDirection;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (_animator == null) return;

            // Check for wall press
            CheckWallPress();

            // Update slide
            if (isSliding)
            {
                _slideTimer -= Time.deltaTime;
                if (_slideTimer <= 0)
                {
                    EndSlide();
                }
            }
        }

        public void TrySlide(Vector3 direction)
        {
            if (isSliding || isVaulting || isRolling) return;

            // Can only slide when moving
            if (_controller != null && _controller.velocity.magnitude > 1f)
            {
                StartSlide(direction);
            }
        }

        public void TryRoll(Vector3 direction)
        {
            if (isSliding || isVaulting || isRolling) return;

            isRolling = true;
            _animator.SetTrigger("Roll");
            _animator.SetBool(isRollingParam, true);

            // Apply roll impulse
            if (_controller != null)
            {
                _controller.Move(direction.normalized * 2f);
            }

            StartCoroutine(RollCoroutine());
        }

        public void TryVault()
        {
            if (isSliding || isVaulting || isRolling) return;

            // Check for vaultable obstacle
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out RaycastHit hit, 1.5f))
            {
                if (hit.transform.CompareTag("Obstacle") && hit.point.y - transform.position.y < vaultHeight)
                {
                    StartVault(hit.point);
                }
            }
        }

        private void StartSlide(Vector3 direction)
        {
            isSliding = true;
            _slideDirection = direction.normalized;
            _slideTimer = slideDuration;
            
            _animator.SetBool(isSlidingParam, true);
            _animator.SetTrigger("StartSlide");
        }

        private void EndSlide()
        {
            isSliding = false;
            _animator.SetBool(isSlidingParam, false);
            _animator.SetTrigger("EndSlide");
        }

        private void StartVault(Vector3 obstaclePoint)
        {
            isVaulting = true;
            _animator.SetBool(isVaultingParam, true);
            _animator.SetTrigger("Vault");

            StartCoroutine(VaultCoroutine(obstaclePoint));
        }

        private System.Collections.IEnumerator RollCoroutine()
        {
            yield return new WaitForSeconds(rollDuration);
            isRolling = false;
            _animator.SetBool(isRollingParam, false);
        }

        private System.Collections.IEnumerator VaultCoroutine(Vector3 obstaclePoint)
        {
            // Move character over obstacle
            Vector3 startPos = transform.position;
            Vector3 vaultPos = obstaclePoint + transform.forward * 0.5f + Vector3.up * vaultHeight;
            Vector3 endPos = obstaclePoint + transform.forward * 2f;

            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed;
                
                if (t < 0.5f)
                {
                    // Climb up
                    transform.position = Vector3.Lerp(startPos, vaultPos, t * 2f);
                }
                else
                {
                    // Jump down
                    transform.position = Vector3.Lerp(vaultPos, endPos, (t - 0.5f) * 2f);
                }
                
                yield return null;
            }

            isVaulting = false;
            _animator.SetBool(isVaultingParam, false);
        }

        private void CheckWallPress()
        {
            // Check for walls in movement direction
            Vector3 checkDir = _controller != null ? _controller.velocity.normalized : transform.forward;
            if (checkDir.magnitude < 0.1f) checkDir = transform.forward;

            if (Physics.Raycast(transform.position + Vector3.up, checkDir, out RaycastHit hit, wallPressDistance))
            {
                if (!isWallPressed)
                {
                    isWallPressed = true;
                    _animator.SetBool(wallPressParam, true);
                    _animator.SetTrigger("WallPress");
                }
            }
            else
            {
                if (isWallPressed)
                {
                    isWallPressed = false;
                    _animator.SetBool(wallPressParam, false);
                }
            }
        }

        public void CancelAllManeuvers()
        {
            isSliding = false;
            isVaulting = false;
            isRolling = false;
            isWallPressed = false;

            _animator.SetBool(isSlidingParam, false);
            _animator.SetBool(isVaultingParam, false);
            _animator.SetBool(isRollingParam, false);
            _animator.SetBool(wallPressParam, false);
        }
    }
}
