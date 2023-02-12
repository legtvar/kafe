using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe;

public class Const
{
    public static readonly LocalizedString UnknownAuthor
        = LocalizedString.Create(
            (CultureInfo.InvariantCulture, "Unknown author"),
            (CultureInfo.CreateSpecificCulture("cs"), "Neznámý autor"));
}
