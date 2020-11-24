using UnityEngine;
namespace agora_utilities
{
    public class AgoraUIUtils
    {
        public static Vector2 GetScaledDimension(float width, float height, float WindowSideLength)
        {
            float newWidth = width;
            float newHeight = height;
            float ratio = (float)height / (float)width;
            if (width > height)
            {
                newHeight = WindowSideLength;
                newWidth = WindowSideLength / ratio;
            }
            else
            {
                newHeight = WindowSideLength * ratio;
                newWidth = WindowSideLength;
            }
            return new Vector2(newWidth, newHeight);
        }

        public static Vector2 GetRandomPosition(float Offset)
        {
            float xPos = UnityEngine.Random.Range(Offset - Screen.width / 2f, Screen.width / 2f - Offset);
            float yPos = UnityEngine.Random.Range(Offset, Screen.height / 2f - Offset);
            return new Vector2(xPos, yPos);
        }
    }
}
