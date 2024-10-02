using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOD.Narcotics.AddictionCore.Addictions
{
    internal interface IAddiction
    {
        Action<bool> MildStageAction();
        Action<bool> SevereStageAction();
        Action<bool> ExtremeStageAction();
    }
}
