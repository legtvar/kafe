# `KafeProblemDetails`

It took more than a week of on-and-off work, but I finally moulded KAFE's error model into something consistent.

Up until now, KAFE sometimes returned at least:
* Custom-made instances of [`ProblemDetails`] from our `SemanticExceptionFilter`.
* MVC's [`ValidationProblemDetails`].
* A list of our `Error` struct.

These got all unified into `KafeProblemDetails` an extension of [`ProblemDetails`]
that may contain a list of our `Error`s.

But first... what are problem details?


## IETF RFC 7807

Problem details are not something specific to ASP.NET Core.
It's an internet stardard -- [IETF RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807).
The RFC specified a (most frequently) JSON structure with several mandatory fields
and an option to add any application-specific extension.

That's all great but forcing ASP.NET Core and MVC to relinquish *their* `ProblemDetails` and use our own
is a lot more complicated that the RFC itself.

In ASP.NET Core, `ProblemDetails` can be made in one of two ways: using `ProblemDetailsFactory` or `IProblemDetailsService`.


## MVC's `ProblemDetailsFactory`

An implementation of MVC's `ProblemDetailsFactory` is used for:

* Model binding errors.
* Model validation errors (meaning that `HttpContext.ModelState` gets turned into a [`ValidationProblemDetails`]).
* When the controller returns an `ActionResult` using the `Problem` or `ValidationProblem` factory functions.
* When the controller returns any other negative `ActionResult`, but in this case `ProblemDetailsFactory`
  is used indirectly through their implementation of `IClientErrorFactory`.


## ASP.NET Core's `IProblemDetailsService` and `IProblemDetailsWriter`

Everything in ASP.NET Core that isn't MVC, uses `IProblemDetailsService`,
which in turn calls `IProblemDetailsWriter`s until one of them writes somtheing to `HttpContext`s response.
The components that use this mechanisms include:

* [`StatusCodePagesMiddleware`]
* [`ExceptionHandlerMiddleware`]
* [`DeveloperExceptionPageMiddleware`]

In other words, if something goes wrong in an MVC endpoint or the endpoint fails to write a response, `IProblemDetailsService` probably gets invoked.

These two mechanisms for the creation of problem details seem completely separate... and for the most part they are.
Except... MVC adds its own `DefaultApiProblemDetailsWriter`
that forces all instances of `ProblemDetails` to be recreated using their `ProblemDetailsFactory`
so that they are made consistently.


## Tearing `ProblemDetailsFactory` apart

Unfortunately, `ProblemDetailsFactory` is tied to [`ValidationProblemDetails`] which we wanted to replace.
We could not just extend them because they already contain an `errors` field
as a dictionary of RequestParameter-ErrorList pairs.
Instead our `KafeProblemDetails` work like this:

### `UnsupportedKafeProblemDetailsFactory`

We have a `UnsupportedKafeProblemDetailsFactory` that prevents any creation of `ValidationProblemDetails`
by throwing an `UnsupportedOperationException`.
**This prevents the use of `Problem` and `ValidationProblem` in controllers.**
However, we now have our own extension methods, `KafeErrResult` and `KafeErrorResult`
that take `Err<T>` and `Error` respectively.

### `KafeProblemDetailsClientErrorFactory`

As mentioned above, MVC uses the `ProblemDetailsFactory` in their `IClientErrorFactory`, so we have to provide our own
to re-enable controllers to return things like `Unauthorized` and `Conflict`.
For this we have `KafeProblemDetailsClientErrorFactory` which internally creates instances of `KafeProblemDetails`.

### `InvalidModelStateResponseFactory`

By setting the `ApiBehaviorOptions.InvalidModelStateResponseFactory` property we let MVC's model binding and validation
unwittingly create not just any `ProblemDetails` but `KafeProblemDetails`
while avoiding `ValidationProblemDetailsFactory`.

### `KafeProblemDetailsExceptionHandler`

`KafeProblemDetailsExceptionHandler` is fairly simple, it converts any unhandled exception in KAFE into an `Error`
(thus preserving its stack trace) and wraps it in a neat `KafeProblemDetails`, which it hands over to an injected
`IProblemDetailsService`.

### `KafeProblemDetailsService`

Yes, we need to have our own `IProblemDetailsService`.
It works the same as the default ASP.NET Core implementation
except it converts everything to `KafeProblemDetails` first.

### `KafeProblemDetailsWriter`

Finally, our version of `IProblemDetailsWriter` is there just to avoid a bunch of defaults
that [`DefaultProblemDetailsWriter`] and [`DefaultApiProblemDetailsWriter`] have.
First, it avoids calling `ProblemDetailsFactory` since that would just throw an exception
because we'd get injected with out `UnsupportedProblemDetailsFactory`.
And second, it removes the `application/problem+xml` content type, because I don't want to think about serializing
our problem details into XML.


## Avoding stack trace leakage

On production, it's really not a good idea to send stack traces of errors to the client.
To stop that, I once again used a modifier for `System.Text.Json`'s `TypeInfoResolver`
that removes the `StackTrace` property on production.


## Swagger

Finally, to let even Swagger know about `KafeProblemDetails`, I added the `AssemblyAttributes` file,
which currently has just this attribute:

```csharp
[assembly: ProducesErrorResponseType(typeof(KafeProblemDetails))]
```

Swagger, thank god, can respect this attribute.


## References

* [Handle errors in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-9.0)
* [Model binding in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-9.0)
* [Model validation in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-9.0)

[`ProblemDetails`]: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.problemdetails?view=aspnetcore-9.0
[`ValidationProblemDetails`]: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.validationproblemdetails?view=aspnetcore-9.0
[`DefaultProblemDetailsWriter`]: https://github.com/dotnet/aspnetcore/blob/a536565375648e7c5dca98bba13c7c39b9187090/src/Http/Http.Extensions/src/DefaultProblemDetailsWriter.cs#L12
[`DefaultApiProblemDetailsWriter`]: https://github.com/dotnet/aspnetcore/blob/a536565375648e7c5dca98bba13c7c39b9187090/src/Mvc/Mvc.Core/src/Infrastructure/DefaultApiProblemDetailsWriter.cs#L10
[`StatusCodePagesMiddleware`]: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-9.0#usestatuscodepages
[`ExceptionHandlerMiddleware`]: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-9.0#exception-handler-page
[`DeveloperExceptionPageMiddleware`]: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-9.0#developer-exception-page
