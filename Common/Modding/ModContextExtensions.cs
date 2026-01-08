using System;
using System.Reflection;

namespace Kafe;

public static class ModContextExtensions
{
    extension(ModContext ctx)
    {
        public KafeType AddScalar<T>(ScalarRegistrationOptions? options = null)
            where T : IKafeTypeMetadata
        {
            options ??= new ScalarRegistrationOptions();
            PopulateRegistrationOptions<T>(options);
            return ctx.AddTypeRaw(
                type: typeof(T),
                category: IScalar.TypeCategory,
                options: options,
                extension: new ScalarTypeMetadata(
                    DefaultRequirements: [..options.DefaultRequirements]
                )
            );
        }

        public KafeType AddScalar(Type type, ScalarRegistrationOptions? options = null)
        {
            options ??= new ScalarRegistrationOptions();

            if (type.IsAssignableTo(typeof(IScalar)))
            {
                PopulateRegistrationOptions(type, options);
            }

            return ctx.AddTypeRaw(
                type: type,
                category: IScalar.TypeCategory,
                options: options,
                extension: new ScalarTypeMetadata(
                    DefaultRequirements: [..options.DefaultRequirements]
                )
            );
        }

        public KafeType AddRequirement<T>(RequirementRegistrationOptions? options = null)
            where T : IRequirement
        {
            options ??= new RequirementRegistrationOptions();
            PopulateRegistrationOptions<T>(options);
            return ctx.AddTypeRaw(
                type: typeof(T),
                category: IRequirement.TypeCategory,
                options: options,
                extension: new RequirementTypeMetadata(
                    HandlerTypes: [..options.HandlerTypes]
                )
            );
        }

        public KafeType AddDiagnosticPayload<T>(DiagnosticPayloadRegistrationOptions? options = null)
            where T : IDiagnosticPayload
        {
            options ??= new DiagnosticPayloadRegistrationOptions();
            PopulateRegistrationOptions<T>(options);
            return ctx.AddTypeRaw(
                type: typeof(T),
                category: IDiagnosticPayload.TypeCategory,
                options: options,
                extension: options.OverrideDescriptor ?? DiagnosticDescriptor.FromPayloadType<T>()
            );
        }

        public KafeType AddShardPayload<T>(ShardPayloadRegistrationOptions? options = null)
            where T : IShardPayload
        {
            options ??= new ShardPayloadRegistrationOptions();
            PopulateRegistrationOptions<T>(options);
            return ctx.AddTypeRaw(
                type: typeof(T),
                category: IShardPayload.TypeCategory,
                options: options,
                extension: new ShardPayloadTypeMetadata(
                    AnalyzerTypes: [..options.AnalyzerTypes]
                )
            );
        }

        private KafeType AddTypeRaw(
            Type type,
            string category,
            KafeTypeRegistrationOptions options,
            object extension
        )
        {
            var typeName = options.Moniker;
            if (string.IsNullOrWhiteSpace(typeName))
            {
                typeName = Naming.ToDashCase(type.Name);
            }

            var kafeType = new KafeType(
                Mod: ctx.Name,
                Category: category,
                Moniker: typeName
            );

            ctx.TypeRegistry.Register(
                new KafeTypeMetadata(
                    KafeType: kafeType,
                    DotnetType: type,
                    Accessibility: options.Accessibility,
                    Title: options.Title,
                    Converter: options.Converter,
                    Extension: extension
                )
            );
            return kafeType;
        }
    }

    private static void PopulateRegistrationOptions(Type type, KafeTypeRegistrationOptions options)
    {
        if (!type.IsAssignableTo(typeof(IKafeTypeMetadata)))
        {
            throw new ArgumentException(
                "Cannot populate KafeType metadata from a type that does not implement "
                + $"'{nameof(IKafeTypeMetadata)}'.",
                nameof(type)
            );
        }

        typeof(ModContextExtensions).GetMethod(
            name: nameof(PopulateRegistrationOptions),
            genericParameterCount: 1,
            bindingAttr: BindingFlags.NonPublic | BindingFlags.Static,
            types: [typeof(KafeTypeRegistrationOptions)]
        )!.MakeGenericMethod(type).Invoke(null, [options]);
    }

    private static void PopulateRegistrationOptions<T>(KafeTypeRegistrationOptions options)
        where T : IKafeTypeMetadata
    {
        options.Moniker ??= T.Moniker;
        options.Title ??= T.Title;
    }
}
