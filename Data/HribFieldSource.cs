using Marten;
using Marten.Linq.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data;

public class HribFieldSource : IFieldSource
{
    public bool TryResolve(
        string dataLocator,
        StoreOptions options,
        ISerializer serializer,
        Type documentType,
        MemberInfo[] members,
        out IField field)
    {
        if (members.All(m => m is PropertyInfo prop && prop.PropertyType == typeof(Hrib)))
        {
            field = new StringField(dataLocator, serializer.Casing, members);
            return true;
        }

        field = null!;
        return false;
    }
}
