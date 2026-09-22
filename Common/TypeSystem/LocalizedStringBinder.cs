using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kafe;

public class LocalizedStringBinder : CollectionModelBinder<KeyValuePair<string, string>>
{
    [Obsolete("Obsolete. Added only to fulfill the interface contract.")]
    public LocalizedStringBinder() : this(NullLoggerFactory.Instance)
    {
    }

    public LocalizedStringBinder(ILoggerFactory loggerFactory)
        : base(
            new KeyValuePairModelBinder<string, string>(
                keyBinder: new SimpleTypeModelBinder(typeof(string), loggerFactory),
                valueBinder: new SimpleTypeModelBinder(typeof(string), loggerFactory),
                loggerFactory: loggerFactory
            ),
            loggerFactory
        )
    {
    }

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var dictionaryContext = DefaultModelBindingContext.CreateBindingContext(
            bindingContext.ActionContext,
            bindingContext.ValueProvider,
            bindingContext.ModelMetadata,
            new BindingInfo()
            {
                BinderModelName = bindingContext.BinderModelName,
                BindingSource = bindingContext.BindingSource
            },
            modelName: bindingContext.ModelName
        );
        await base.BindModelAsync(bindingContext);
        var result = bindingContext.Result;

        if (result is { IsModelSet: true, Model: IDictionary<string, string> { Count: > 0 } dictionary })
        {
            // easy route; base
            bindingContext.Result = ModelBindingResult.Success(LocalizedString.Create(dictionary));
            return;
        }
    }
}
