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
            _spriteAnimation.addFrame("s_grdstand", 50, 51, 52, 53, 54, 55, 56, 57, 0, "s_grdstand");

            // Walking animation frames.
            _spriteAnimation.addFrame("s_grdpath1", 58, 59, 60, 61, 62, 63, 64, 65, 20, "s_grdpath1s");
            _spriteAnimation.addFrame("s_grdpath1s", 58, 59, 60, 61, 62, 63, 64, 65, 5, "s_grdpath2");
            _spriteAnimation.addFrame("s_grdpath2", 66, 67, 68, 69, 70, 71, 72, 73, 15, "s_grdpath3");
            _spriteAnimation.addFrame("s_grdpath3", 74, 75, 76, 77, 78, 79, 80, 81, 20, "s_grdpath3s");
            _spriteAnimation.addFrame("s_grdpath3s", 74, 75, 76, 77, 78, 79, 80, 81, 5, "s_grdpath4");
            _spriteAnimation.addFrame("s_grdpath4", 82, 83, 84, 85, 86, 87, 88, 89, 15, "s_grdpath1");

            // Pain frames.
            _spriteAnimation.addFrame("s_grdpain1", 90, 10, "s_grdchase1");
            _spriteAnimation.addFrame("s_grdpain2", 94, 10, "s_grdchase1");

            // Shooting frames.
            _spriteAnimation.addFrame("s_grdshoot1", 96, 20, "s_grdshoot2");
            _spriteAnimation.addFrame("s_grdshoot2", 97, 20, "s_grdshoot3");
            _spriteAnimation.addFrame("s_grdshoot3", 98, 20, "s_grdchase1");

            // Chase frames.
            _spriteAnimation.addFrame("s_grdchase1", 58, 59, 60, 61, 62, 63, 64, 65, 10, "s_grdchase1s");
            _spriteAnimation.addFrame("s_grdchase1s", 58, 59, 60, 61, 62, 63, 64, 65, 3, "s_grdchase2");
            _spriteAnimation.addFrame("s_grdchase2", 66, 67, 68, 69, 70, 71, 72, 73, 8, "s_grdchase3");
            _spriteAnimation.addFrame("s_grdchase3", 74, 75, 76, 77, 78, 79, 80, 81, 10, "s_grdchase3s");
            _spriteAnimation.addFrame("s_grdchase3s", 74, 75, 76, 77, 78, 79, 80, 81, 3, "s_grdchase4");
            _spriteAnimation.addFrame("s_grdchase4", 82, 83, 84, 85, 86, 87, 88, 89, 8, "s_grdchase1");

            // Death Frames.
            _spriteAnimation.addFrame("s_grddie1", 91, 10, "s_grddie2");
            _spriteAnimation.addFrame("s_grddie2", 92, 10, "s_grddie3");
            _spriteAnimation.addFrame("s_grddie3", 93, 10, "s_grddie4");
            _spriteAnimation.addFrame("s_grddie4", 95, 0, "s_grddie4");

            _spriteAnimation.setCurrentFrame("s_grdstand");
        }
    }
}
