using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Additional
{
    public class UserImg : MonoBehaviour
    {
        [SerializeField] private Image userImg;

        [HideInInspector] public string imgName;

        public Action<UserImg> elementDelete;

        public void SetUserImg(Sprite userSprite)
        {
            userImg.sprite = userSprite;
        }

        public void DeleteUserImg()
        {
            elementDelete?.Invoke(this);
            Destroy(gameObject);
        }
    }
}