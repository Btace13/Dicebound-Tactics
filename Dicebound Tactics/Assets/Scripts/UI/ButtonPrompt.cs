using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonPrompt : MonoBehaviour
{
    [SerializeField] private Image buttonIcon;
    [SerializeField] private Image backgroundImage;
    
    public void SetButtonIcon(Sprite icon)
    {
        if (buttonIcon != null)
        {
            buttonIcon.sprite = icon;
        }
    }
    
    public void SetColors(Color iconColor, Color backgroundColor)
    {
        if (buttonIcon != null)
        {
            buttonIcon.color = iconColor;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }
    }
    
    public void PlayPressAnimation()
    {
        transform.DOPunchScale(Vector3.one * 0.3f, 0.3f).SetEase(Ease.OutBounce);
    }
    
    public void PlayHighlightAnimation()
    {
        if (backgroundImage != null)
        {
            backgroundImage.DOFade(0.5f, 0.1f).SetLoops(6, LoopType.Yoyo);
        }
    }
}
