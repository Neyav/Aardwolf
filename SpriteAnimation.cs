using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aardwolf
{
    internal class SpriteAnimation
    {
        Dictionary<string, SpriteFrame> _spriteFrames;
        string _CurrentFrame;

        public void addFrame (string frameName, bool rotate, int front, int frontLeft, int left, int backLeft, int back, int backRight, int right, int frontRight, int spriteTime, string nextSprite)
        {
            SpriteFrame frame = new SpriteFrame();
            frame.setFrames(front, frontLeft, left, backLeft, back, backRight, right, frontRight, 
                (int)(1000f / 70f * spriteTime), nextSprite); // Convert spriteTime from 70hz to milliseconds
            _spriteFrames.Add(frameName, frame);
        }

        public void setCurrentFrame(string frameName)
        {
            if (_spriteFrames.ContainsKey(frameName))
            {
                _CurrentFrame = frameName;
            }
            else
            {
                throw new ArgumentException($"Frame '{frameName}' does not exist in the sprite animation.");
            }
        }

        public int getFrame(Camera camera, float spriteAngle, double timeDelta)
        {
            int modifiedTimeDelta = (int)(timeDelta * 1000);
            // If the current frame is null, return the first frame
            if (_CurrentFrame == null || !_spriteFrames.ContainsKey(_CurrentFrame))
            {
                _CurrentFrame = _spriteFrames.Keys.FirstOrDefault();
            }
            // Get the current sprite frame
            SpriteFrame currentSpriteFrame = _spriteFrames[_CurrentFrame];
            // Calculate the next frame if needed
            while (currentSpriteFrame.calculateNextFrame(modifiedTimeDelta))
            {
                // We need to switch to the next frame.
                if (currentSpriteFrame._nextSprite != null && _spriteFrames.ContainsKey(currentSpriteFrame._nextSprite))
                {
                    _CurrentFrame = currentSpriteFrame._nextSprite;
                    currentSpriteFrame = _spriteFrames[_CurrentFrame];
                }
                else
                {
                    // If no next sprite is defined, break the loop
                    break;
                }
            }

            // Return the frame based on the camera angle
            return currentSpriteFrame.getFrame(spriteAngle, camera.Yaw);
        }

        public SpriteAnimation()
        {
            _spriteFrames = new Dictionary<string, SpriteFrame>();
        }
    }
}
