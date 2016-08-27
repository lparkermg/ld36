using UnityEngine;
using System.Collections;
namespace Helpers.Levels
{
    public static class LevelHelper
    {

        public static int[,] ParseFromTexture2D(Texture2D texture)
        {
            var mapData = new int[texture.width,texture.height];
            
            for(var x = 0; x < texture.width; x++)
            {
                for(var y = 0; y < texture.height; y++)
                {
                    var pixelColor = texture.GetPixel(x, y);
                    mapData[x, y] = ColorToInt(pixelColor);

                }
            }

            return mapData;
        } 

        private static int ColorToInt(Color color)
        {
            if(color == Color.white)
            {
                return 999;
            }
            else if(color == Color.black)
            {
                return 0;
            }
            else if(color == Color.blue)
            {
                return 1;
            }
            else if(color == Color.red)
            {
                return 2;
            }

            return 999;
        }
    }
}
