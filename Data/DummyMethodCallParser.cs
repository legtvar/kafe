using Kafe.Data.Capabilities;
using Marten;
using Marten.Linq.Fields;
using Marten.Linq.Filters;
using Marten.Linq.Parsing;
using System.Linq;
using System.Linq.Expressions;
using Weasel.Postgresql.SqlGeneration;

namespace Kafe.Data;

public class DummyMethodCallParser : IMethodCallParser
{
    public bool Matches(MethodCallExpression expression)
    {
        return expression.Method.Name == "Contains"
            && expression.Arguments.Count == 1
            && expression.Arguments[0].Type.IsAssignableTo(typeof(AccountCapability));
    }

    public ISqlFragment Parse(IFieldMapping mapping, ISerializer serializer, MethodCallExpression expression)
    {
        var capabilityString = AccountCapability.Serialize((AccountCapability)expression.Arguments.Single().Value());
        var fragment = ContainmentWhereFragment.SimpleArrayContains(
            FindMembers.Determine(expression.Object),
            serializer,
            expression.Object,
            capabilityString);
        return fragment;
    }
}