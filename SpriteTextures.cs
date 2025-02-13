using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aardwolf
{
    internal class SpriteFrame
    {
        // Sprite Frames will be Front, FrontLeft, Left, BackLeft, Back, BackRight, Right, FrontRight
        private int[] SpriteTexture;

        public void setFrames(int Front, int FrontLeft, int Left, int BackLeft, int Back, int BackRight, int Right, int FrontRight)
        {
            SpriteTexture[0] = Front;
            SpriteTexture[1] = FrontLeft;
            SpriteTexture[2] = Left;
            SpriteTexture[3] = BackLeft;
            SpriteTexture[4] = Back;
            SpriteTexture[5] = BackRight;
            SpriteTexture[6] = Right;
            SpriteTexture[7] = FrontRight;
        }

        public int getFrame(float spriteAngle, float viewerAngle)
        {
            // Calculate the relative angle
            float relativeAngle = (spriteAngle - viewerAngle + 360) % 360;

            // Determine the frame index based on the relative angle
            int viewFrame = (int)(relativeAngle / 45) % 8;

            return SpriteTexture[viewFrame];            
        }
        public SpriteFrame()
        {
            SpriteTexture = new int[8];
        }
    }
    internal class SpriteTextures
    {
       
    }
}
