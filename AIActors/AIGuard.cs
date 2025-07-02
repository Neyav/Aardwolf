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
        public AIGuard(float widthPosition, float heightPosition, int angle)
        {
            Health = 25;
            MaxHealth = 25;
            Angle = angle;
            WidthPosition = widthPosition;
            HeightPosition = heightPosition;
            // Set default sprite frames for the guard
            _spriteAnimation.addFrame("s_grdstand", true, 50, 51, 52, 53, 54, 55, 56, 57, 0, "s_grdstand");

            // Walking animation frames.
            _spriteAnimation.addFrame("s_grdpath1", true, 58, 59, 60, 61, 62, 63, 64, 65, 20, "s_grdpath1s");
            _spriteAnimation.addFrame("s_grdpath1s", true, 58, 59, 60, 61, 62, 63, 64, 65, 5, "s_grdpath2");
            _spriteAnimation.addFrame("s_grdpath2", true, 66, 67, 68, 69, 70, 71, 72, 73, 15, "s_grdpath3");
            _spriteAnimation.addFrame("s_grdpath3", true, 74, 75, 76, 77, 78, 79, 80, 81, 20, "s_grdpath3s");
            _spriteAnimation.addFrame("s_grdpath3s", true, 74, 75, 76, 77, 78, 79, 80, 81, 5, "s_grdpath4");
            _spriteAnimation.addFrame("s_grdpath4", true, 82, 83, 84, 85, 86, 87, 88, 89, 15, "s_grdpath1");

            _spriteAnimation.setCurrentFrame("s_grdpath1");
        }
    }
}
