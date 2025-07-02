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
        private int[] _SpriteTexture;
        private bool _SpriteRotates;
        private int _SpriteTime;
        private int _TimeElapsed;
        public string _nextSprite;

        public void setFrames(int Front, int FrontLeft, int Left, int BackLeft, int Back, int BackRight, int Right, int FrontRight, int SpriteTime, string nextSprite)
        {
            _SpriteTexture[0] = Front;
            _SpriteTexture[1] = FrontLeft;
            _SpriteTexture[2] = Left;
            _SpriteTexture[3] = BackLeft;
            _SpriteTexture[4] = Back;
            _SpriteTexture[5] = BackRight;
            _SpriteTexture[6] = Right;
            _SpriteTexture[7] = FrontRight;

            _SpriteRotates = true;
            _SpriteTime = SpriteTime;
            _nextSprite = nextSprite;

            _TimeElapsed = 0;
        }

        public void setFrames(int Front, int SpriteTime, string nextSprite)
        {

            _SpriteTexture[0] = Front;
            _SpriteRotates = false;

            _SpriteTime = SpriteTime;
            _nextSprite = nextSprite;

            _TimeElapsed = 0;
        }

        public int getFrame(float spriteAngle, float viewerAngle)
        {
            // If the sprite does not rotate, return the first frame
            if (!_SpriteRotates)
            {
                return _SpriteTexture[0];
            }
            // Calculate the relative angle
            float relativeAngle = (spriteAngle - viewerAngle + 360) % 360;

            // Determine the frame index based on the relative angle
            int viewFrame = (int)(relativeAngle / 45) % 8;

            return _SpriteTexture[viewFrame];            
        }
        public bool calculateNextFrame(int timeDelta)
        {
            if (_SpriteTime == 0)
                return false; // No time set for sprite switching

            _TimeElapsed += timeDelta;
            if (_TimeElapsed >= _SpriteTime)
            {
                timeDelta -= (_TimeElapsed - _SpriteTime);
                _TimeElapsed = 0;
                return true; // Time to switch frames
            }

            return false; // We don't need to switch frames.
        }

        public SpriteFrame()
        {
            _SpriteTexture = new int[8];
        }
    }
}
