using Marten;
using Marten.Linq.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data;

public class LocalizedStringFieldSource : IFieldSource
{
    public bool TryResolve(
        string dataLocator,
        StoreOptions options,
        ISerializer serializer,
        Type documentType,
        MemberInfo[] members,
        out IField field)
    {
        if (members.All(m => m is PropertyInfo prop && prop.PropertyType == typeof(LocalizedString)))
        {
            field = new DictionaryField(dataLocator, serializer.Casing, serializer.EnumStorage, members);
            return true;
        }

        field = null!;
        return false;
    }
}
