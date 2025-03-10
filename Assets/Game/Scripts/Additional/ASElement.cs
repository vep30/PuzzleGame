using TMPro;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Scripts.Base.Helpers;
using UnityEngine.EventSystems;
using System.Collections.Generic;

// ASElement - AdditionalSettingsElement
namespace Game.Scripts.Additional
{
    public class ASElement : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private float pauseSlide = 1.5f;
        [SerializeField] private float showHideSpeed = .75f;
        [SerializeField] private Button editBtn, deleteBtn;

        public Image img;
        public TMP_Text name;
        public GameObject outline;
        public Action deleteElement;
        public Action<ASElement> clickElement;
        public Action<ASElement> clickEditElement;

        [HideInInspector] public string pathToFolder;
        [HideInInspector] public bool isUserImg;
        [HideInInspector] public List<Sprite> images = new();
        
        private List<Texture2D> imgInfo = new();
        private Coroutine slideShowCor;
        private bool isSelected;
        
        private void Start()
        {
            editBtn.gameObject.SetActive(isUserImg);
            editBtn.onClick.AddListener(EditElementClick);
            deleteBtn.gameObject.SetActive(isUserImg);
            deleteBtn.onClick.AddListener(DeleteUserElement);
        }
        
        private void EditElementClick()
        {
            clickEditElement?.Invoke(this);
        }
        
        private IEnumerator SlideShowCoroutine()
        {
            if (images.Count <= 1)
                yield break;

            var currentIndex = 1;
            while (true)
            {
                yield return new WaitForSeconds(pauseSlide);
                yield return ChangeAlphaCor();
                img.sprite = images[currentIndex];
                currentIndex = (currentIndex + 1) % images.Count;
                yield return ChangeAlphaCor(false);
            }
        }
        
        private IEnumerator ChangeAlphaCor(bool hide = true)
        {
            var currentColor = img.color;
            var targetColor = hide ? 0 : 1;
            float curTime = 0f;
            while (curTime < showHideSpeed)
            {
                curTime += Time.deltaTime;
                currentColor.a = Mathf.Lerp(currentColor.a, targetColor, curTime / showHideSpeed);
                img.color = currentColor;
                yield return null;
            }
            
            currentColor.a = targetColor;
            img.color = currentColor;
        }
        
        private void StopSlideShow()
        {
            if (slideShowCor == null)
                return;

            StopCoroutine(slideShowCor);
            slideShowCor = null;
        }
        
        private void DeleteUserElement()
        {
            StopSlideShow();
            
            if (!string.IsNullOrEmpty(pathToFolder))
                if (Directory.Exists(pathToFolder))
                    Directory.Delete(pathToFolder, true);
            
            if (isSelected)
                deleteElement?.Invoke();
            
            Destroy(gameObject);
        }

        public void SetImgInfo(List<Texture2D> info)
        {
            imgInfo.Clear();
            imgInfo.AddRange(info);
        }

        public void ChangeSelected(bool select)
        {
            isSelected = select;
            outline.SetActive(select);
        }

        public List<Texture2D> GetImgInfo()
        {
            return imgInfo;
        }
        
        public void StartSlideShow()
        {
            images.Clear();
            StopSlideShow();
            if (imgInfo.Count <= 0)
                return;

            foreach (var info in imgInfo)
                images.Add(info.Texture2DToSprite());
            
            img.sprite = images[0];
            if (images.Count > 1)
                slideShowCor = StartCoroutine(SlideShowCoroutine());
        }
        
        public void OnPointerClick(PointerEventData pointerEventData)
        {
            clickElement?.Invoke(this);
        }
    }
}