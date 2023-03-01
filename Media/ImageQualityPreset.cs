using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Media;

public enum ImageQualityPreset
{
    Invalid = 0,
    Original = 1,

    /// <summary>
    /// 200px larger side
    /// </summary>
    Thumbnail = 2,

    /// <summary>
    /// 1024px larger side
    /// </summary>
    Large = 3
}
