using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Ruv;

public record SaveAuthorParams(
    string PersonalNumber,
    string FirstName,
    string LastName,
    string DegreeBeforeName = "",
    string DegreeAfterName = "",
    int OrgUnit = RuvConst.DefaultOrgUnit
);
