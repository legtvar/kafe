using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Kafe.Api;

public class KafeProblemDetailsWriter : IProblemDetailsWriter
{
    public bool CanWrite(ProblemDetailsContext context)
    {
        throw new System.NotImplementedException();
    }

    public ValueTask WriteAsync(ProblemDetailsContext context)
    {
        throw new System.NotImplementedException();
    }
}
