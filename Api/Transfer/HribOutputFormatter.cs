using System;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Kafe.Api.Transfer;

public class HribOutputFormatter : TextOutputFormatter
{
    public HribOutputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeNames.Text.Plain);

        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    protected override bool CanWriteType(Type? type)
    {
        return type == typeof(Hrib);
    }

    public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        if (context.Object is null)
        {
            throw new InvalidOperationException("API tried to respond with a null HRIB. This is a bug.");
        }
        if (context.Object is not Hrib hrib)
        {
            throw new NotSupportedException("Cannot format objects that are not HRIBs.");
        }

        return context.HttpContext.Response.WriteAsync(hrib.ToString(), selectedEncoding);
    }
}
