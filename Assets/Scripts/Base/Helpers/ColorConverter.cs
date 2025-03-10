using UnityEngine;

namespace Scripts.Base.Helpers
{
    public static class ColorConverter
    {
        public static Color HexToColor(string hex, byte alpha = 255)
        {
            // ������� ������� "#" � ������, ���� ��� ����
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);
            
            // �������� ������������ ����
            if (hex.Length != 6)
            {
                Debug.LogError("Hex color code must be 6 characters long.");
                return Color.white; // ���������� ����� ���� � ������ ������
            }
            
            // ������ ���� �� ������������������ ����
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            
            // ���������� ���� � ������� Unity
            return new Color32(r, g, b, alpha);
        }
    }
}