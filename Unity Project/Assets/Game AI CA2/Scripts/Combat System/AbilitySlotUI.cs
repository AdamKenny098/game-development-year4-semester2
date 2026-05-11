using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilitySlotUI : MonoBehaviour
{
    public Image iconImage;
    public Image cooldownFill;
    public TMP_Text keyText;
    AbilityData ability;

    public void SetKey(string key)
    {
        if (keyText != null) keyText.text = key;
    }

    public void BindAbility(AbilityData abilityData)
    {
        ability = abilityData;

        if (iconImage != null)
        {
            if (ability != null && ability.icon != null)
            {
                iconImage.enabled = true;
                iconImage.sprite = ability.icon;
            }
            else
            {
                iconImage.enabled = false;
                iconImage.sprite = null;
            }
        }

        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = 0f;
        }
    }

    public void UpdateCooldown(AbilityManager manager, int slot)
    {
        if (cooldownFill == null) return;
        if (manager == null || ability == null)
        {
            cooldownFill.fillAmount = 0f;
            return;
        }

        float duration = manager.GetCooldownDuration(slot);
        float remainder = manager.GetCooldownRemaining(slot);

        if (duration <= 0f || remainder <= 0f)
        {
            cooldownFill.fillAmount = 0f;
        }
        else
        {
            cooldownFill.fillAmount = remainder / duration;
        }
    }
}
