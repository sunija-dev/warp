using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverChangeTexture : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ExchangeImage[] arExchangeImages;

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (ExchangeImage exchangeImage in arExchangeImages)
        {
            exchangeImage.image.sprite = exchangeImage.spriteHover;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (ExchangeImage exchangeImage in arExchangeImages)
        {
            exchangeImage.image.sprite = exchangeImage.spriteDefault;
        }
    }

    [System.Serializable]
    public class ExchangeImage
    {
        public Image image;
        public Sprite spriteDefault;
        public Sprite spriteHover;
    }
}
