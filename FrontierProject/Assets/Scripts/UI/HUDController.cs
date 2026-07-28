using UnityEngine;
using System.Collections.Generic;

namespace Frontier.UI
{
    /// <summary>
    /// HUD Controller displaying health, stamina, needs bars, minimap, and hotbar.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Health & Stamina")]
        public UnityEngine.UI.Slider healthSlider;
        public UnityEngine.UI.Slider staminaSlider;
        public UnityEngine.UI.Text healthText;
        public UnityEngine.UI.Text staminaText;

        [Header("Needs Bars")]
        public UnityEngine.UI.Slider hungerSlider;
        public UnityEngine.UI.Slider thirstSlider;
        public UnityEngine.UI.Slider sleepSlider;
        public UnityEngine.UI.Slider hygieneSlider;

        [Header("Minimap")]
        public RenderTexture minimapTexture;
        public Camera minimapCamera;
        public RectTransform playerArrow;

        [Header("Hotbar")]
        public UnityEngine.UI.Image[] hotbarSlots = new UnityEngine.UI.Image[8];
        public UnityEngine.UI.Text[] hotbarCounts = new UnityEngine.UI.Text[8];
        public int selectedSlot = 0;

        [Header("Compass")]
        public UnityEngine.UI.Text compassHeading;
        public RectTransform compassNeedle;

        [Header("Notifications")]
        public UnityEngine.UI.Text notificationText;
        public float notificationDisplayTime = 3f;

        private float _currentHealth = 100f;
        private float _maxHealth = 100f;
        private float _currentStamina = 100f;
        private float _maxStamina = 100f;

        private void Update()
        {
            UpdateHotbarSelection();
            UpdateCompass();
            UpdateMinimap();
        }

        public void SetHealth(float current, float max)
        {
            _currentHealth = current;
            _maxHealth = max;
            
            if (healthSlider != null)
                healthSlider.value = current / max;
            
            if (healthText != null)
                healthText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }

        public void SetStamina(float current, float max)
        {
            _currentStamina = current;
            _maxStamina = max;
            
            if (staminaSlider != null)
                staminaSlider.value = current / max;
            
            if (staminaText != null)
                staminaText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }

        public void SetNeed(NeedType type, float value)
        {
            switch (type)
            {
                case NeedType.Hunger:
                    if (hungerSlider != null) hungerSlider.value = value;
                    break;
                case NeedType.Thirst:
                    if (thirstSlider != null) thirstSlider.value = value;
                    break;
                case NeedType.Sleep:
                    if (sleepSlider != null) sleepSlider.value = value;
                    break;
                case NeedType.Hygiene:
                    if (hygieneSlider != null) hygieneSlider.value = value;
                    break;
            }
        }

        public void SetHotbarSlot(int index, Sprite icon, int count)
        {
            if (index < 0 || index >= hotbarSlots.Length) return;
            
            if (hotbarSlots[index] != null)
                hotbarSlots[index].sprite = icon;
            
            if (hotbarCounts[index] != null)
                hotbarCounts[index].text = count > 1 ? count.ToString() : "";
        }

        private void UpdateHotbarSelection()
        {
            // Highlight selected slot
            for (int i = 0; i < hotbarSlots.Length; i++)
            {
                if (hotbarSlots[i] != null)
                {
                    // Would add highlight effect to selected slot
                }
            }
        }

        private void UpdateCompass()
        {
            if (compassHeading != null)
            {
                float heading = transform.eulerAngles.y;
                compassHeading.text = $"{Mathf.RoundToInt(heading)}°";
            }

            if (compassNeedle != null)
            {
                compassNeedle.eulerAngles = new Vector3(0, 0, -transform.eulerAngles.y);
            }
        }

        private void UpdateMinimap()
        {
            if (minimapCamera != null && playerArrow != null)
            {
                // Update player arrow rotation on minimap
                playerArrow.eulerAngles = new Vector3(0, 0, -transform.eulerAngles.y);
            }
        }

        public void ShowNotification(string message)
        {
            if (notificationText != null)
            {
                notificationText.text = message;
                CancelInvoke(nameof(HideNotification));
                Invoke(nameof(HideNotification), notificationDisplayTime);
            }
        }

        private void HideNotification()
        {
            if (notificationText != null)
                notificationText.text = "";
        }

        public enum NeedType
        {
            Hunger, Thirst, Sleep, Hygiene, Social, Recreation, Comfort, Safety
        }
    }
}
