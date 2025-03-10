using UnityEngine;

namespace Scripts.Base.Helpers
{
    public static class TextureHelper
    {
        public static Sprite Texture2DResizeToSprite(this Texture2D texture, int targetWidth, int targetHeight)
        {
            return texture.ResizeTexture(targetWidth, targetHeight).Texture2DToSprite();
        }
        
        public static Sprite Texture2DToSprite(this Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f));
        }
        
        public static Texture2D ResizeTexture(this Texture2D texture, int targetWidth, int targetHeight)
        {
            RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
            rt.filterMode = FilterMode.Bilinear;
            
            RenderTexture.active = rt;
            Graphics.Blit(texture, rt);
            Texture2D newTexture = new Texture2D(targetWidth, targetHeight);
            newTexture.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            newTexture.Apply();
            
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            
            return newTexture;
        }
    }
}