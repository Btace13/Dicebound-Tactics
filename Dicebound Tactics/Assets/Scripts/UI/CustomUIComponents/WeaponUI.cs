using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class WeaponUI : MonoBehaviour
{
    [BoxGroup("References"), SerializeField] private Image weaponIcon;
    [BoxGroup("References"), SerializeField] private Image weaponTypeIcon;
    [BoxGroup("References"), SerializeField] private TextMeshProUGUI weaponNameText;
    [BoxGroup("References"), SerializeField] private TextMeshProUGUI weaponRarityText;

    [BoxGroup("Debug"), SerializeField] private WeaponData currentWeaponData;

    public void UpdateWeaponUI(WeaponData data)
    {
        weaponIcon.sprite = data.Icon;
        weaponTypeIcon.sprite = data.WeaponTypeIcon;
        weaponNameText.text = data.WeaponName + "+" + data.rarityAmount.ToString(); // Append rarity amount to name
        weaponRarityText.text = "";
        for (int i = 0; i < data.rarityAmount; i++)
        {
            weaponRarityText.text += "*"; // Append stars for rarity
        }
        weaponIcon.color = data.RarityColor;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (currentWeaponData != null)
        {
            // Update the UI with the current weapon data in the editor
            UpdateWeaponUI(currentWeaponData);
        }
    }
#endif
}
