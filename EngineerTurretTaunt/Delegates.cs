using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineerTurretTaunt
{
	public delegate void PlayAnimationOrig(string animationName, int pos);

	public delegate void PlayAnimationHandler(PlayAnimationOrig orig, string animationName, int pos);
}
