using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kafe;

public class LocalizedStringBinder : IModelBinder
{
    private readonly ILoggerFactory loggerFactory;
    private readonly DictionaryModelBinder<string, string> inner;

    public LocalizedStringBinder(ILoggerFactory? loggerFactory = null)
    {
        this.loggerFactory ??= NullLoggerFactory.Instance;
        inner = new DictionaryModelBinder<string, string>(
            keyBinder: new SimpleTypeModelBinder(typeof(string), this.loggerFactory),
            valueBinder: new SimpleTypeModelBinder(typeof(string), this.loggerFactory),
            loggerFactory: this.loggerFactory
        );
    }

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        LocalizedString? result = null;
        using (var nestedScope = bindingContext.EnterNestedScope(
                modelMetadata: bindingContext.ModelMetadata.GetMetadataForType(typeof(Dictionary<string, string>)),
                fieldName: bindingContext.FieldName,
                modelName: bindingContext.ModelName,
                model: new Dictionary<string, string>()
            ))
        {
            await inner.BindModelAsync(bindingContext);
            if (bindingContext.Result is { IsModelSet: true, Model: IDictionary<string, string> { Count: > 0 } dictionary })
            {
                result = LocalizedString.Create(dictionary);
                bindingContext.ValidationState[result] = bindingContext.ValidationState[dictionary];
            }
        }

        if (result is not null)
        {
            bindingContext.Result = ModelBindingResult.Success(result);
            return;
        }

        bindingContext.Result = ModelBindingResult.Failed();
    }
}
