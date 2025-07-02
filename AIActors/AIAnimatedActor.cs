using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AardwolfCore;

namespace Aardwolf.AIActors
{
    internal class AIAnimatedActor : AIActor
    {
        public SpriteAnimation _spriteAnimation { get; set; }

        public int getAnimationFrame(Camera camera, double timeDelta)
        {
            // Get the current frame from the sprite animation
            return _spriteAnimation.getFrame(camera, Angle, timeDelta);
        }

        public AIAnimatedActor()
        {
            _spriteAnimation = new SpriteAnimation();
        }
    }
}
