using Kafe.Core.Diagnostics;

namespace Kafe.Data;

public static class ErrExtensions
{
    extension<T>(Err<T> err) where T : IEntity
    {
        public Err<T> HandleExistingEntity(ExistingEntityHandling mode)
        {
            return mode switch
            {
                ExistingEntityHandling.Insert when err.HasValue
                    => Err.Fail<T>(new AlreadyExistsDiagnostic(typeof(T), err.Value.Id)),
                ExistingEntityHandling.Update when err.HasError
                    => err,
                ExistingEntityHandling.Upsert when err is { HasError: true, Diagnostic.Payload: NotFoundDiagnostic nfd }
                    => Err.Fail(new NotCreatedDiagnostic(nfd.EntityType, nfd.Id)),
                _ => err
            };
        }
    }
}
