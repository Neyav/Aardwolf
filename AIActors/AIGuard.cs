using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AardwolfCore;

namespace Aardwolf.AIActors
{
    internal class AIGuard : AIAnimatedActor
    {
        public AIGuard(float widthPosition, float heightPosition)
        {
            Health = 25;
            MaxHealth = 25;
            WidthPosition = widthPosition;
            HeightPosition = heightPosition;
            SpriteFrame = new SpriteFrame();
            // Set default sprite frames for the guard
            SpriteFrame.setFrames(50, 51, 52, 53, 54, 55, 56, 57); // Example texture IDs
        }
    }
}
