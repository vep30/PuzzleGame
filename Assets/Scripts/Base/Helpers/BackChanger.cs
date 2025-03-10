using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Scripts.Base.Helpers
{
    public class BackChanger : MonoBehaviour
    {
        public static BackChanger Instance;
        [SerializeField] private Image back;
        [SerializeField] private Sprite whiteBack;
        [SerializeField] private Sprite blackBack;
        
        public Sprite GetCurBack => back.sprite;
        public Color GetCurColor => back.color;
        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance == this)
                Destroy(gameObject);
        }
        
        public void ChangeBack(Sprite newSprite)
        {
            back.sprite = newSprite != null ? newSprite : whiteBack;
        }
        
        public void SwitchBack(bool switchToBlack)
        {
            back.sprite = switchToBlack ? blackBack : whiteBack;
        }
        
        public void ChangeBackAlphaAndColor(Color newColor, float newAlpha)
        {
            ChangeBackColor(newColor);
            ChangeBackAlpha(newAlpha);
        }
        
        public void ChangeBackAlpha(float newAlpha)
        {
            var backColor = back.color;
            back.color = new Color(backColor.r, backColor.g, backColor.b, newAlpha);
        }
        
        public void ChangeBackAlphaSoft(float newAlpha, float duration)
        {
            back.DOFade(newAlpha, duration);
        }
        
        public void ChangeBackColor(Color newColor)
        {
            back.color = newColor;
        }
        
        public void ChangeBackColorSoft(Color newColor, float duration)
        {
            StartCoroutine(ChangeBackColorSoftCor(newColor, duration));
        }
        
        private IEnumerator ChangeBackColorSoftCor(Color targetColor, float duration)
        {
            Color originalColor = back.color;
            
            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                back.color = Color.Lerp(originalColor, targetColor, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            back.color = targetColor;
        }
    }
}