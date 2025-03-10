using UnityEngine;

namespace Scripts.Base.Helpers
{
    public static class ColorConverter
    {
        public static Color HexToColor(string hex, byte alpha = 255)
        {
            // Удаляем символы "#" в начале, если они есть
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);
            
            // Проверка корректности кода
            if (hex.Length != 6)
            {
                Debug.LogError("Hex color code must be 6 characters long.");
                return Color.white; // Возвращаем белый цвет в случае ошибки
            }
            
            // Парсим цвет из шестнадцатеричного кода
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            
            // Возвращаем цвет в формате Unity
            return new Color32(r, g, b, alpha);
        }
    }
}