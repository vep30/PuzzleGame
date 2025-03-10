using System;
using UnityEngine;

namespace Scripts.Base.Helpers
{
    public class AnyButton : MonoBehaviour
    {
        [HideInInspector] public bool useMouseDown, useMouseDrag;
        
        public Action anyAction;
        public SpriteRenderer sprite;
        public bool higlightReaction;
        
        private Color higlightColor = new Color32(204, 204, 204, 255); // color - CCCCCC
        
        public void OnMouseDown()
        {
            if (!useMouseDown)
                return;
            
            anyAction?.Invoke();
        }

        public void OnMouseEnter()
        {
            if (!higlightReaction)
                return;
            
            sprite.color = higlightColor;
        }
        
        public void OnMouseExit()
        {
            if (!higlightReaction)
                return;
            
            sprite.color = Color.white;
        }
        
        public void OnMouseDrag()
        {
            if (!useMouseDrag)
                return;
            
            anyAction?.Invoke();
        }
    }
}