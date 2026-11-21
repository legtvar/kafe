using Microsoft.AspNetCore.Builder;

namespace Kafe.Api;

internal static class ApiSecurityHeaders
{
    internal static HeaderPolicyCollection Create() =>
        new HeaderPolicyCollection()
            .AddStrictTransportSecurityMaxAgeIncludeSubDomains(maxAgeInSeconds: 60 * 60 * 24 * 365)
            .AddContentSecurityPolicy(builder =>
            {
                builder.AddDefaultSrc().Self();
                builder.AddBaseUri().Self();
                builder.AddObjectSrc().None();
                builder.AddFrameAncestors().Self();
                builder.AddFrameSrc().None();
                builder.AddFormAction().Self();
                // Swagger UI requires inline scripts and styles.
                builder.AddScriptSrc().Self().UnsafeInline();
                // The landing pages also use inline styles and Google Fonts.
                builder.AddStyleSrc().Self().UnsafeInline().From("https://fonts.googleapis.com");
                builder.AddFontSrc().Self().Data().From("https://fonts.gstatic.com");
                builder.AddImgSrc().Self().Data().Blob();
                builder.AddMediaSrc().Self().Blob();
                builder.AddConnectSrc().Self();
                builder.AddWorkerSrc().Self().Blob();
                builder.AddManifestSrc().Self();
                builder.AddUpgradeInsecureRequests();
            })
            .AddFrameOptionsSameOrigin()
            .AddContentTypeOptionsNoSniff()
            .AddReferrerPolicyStrictOriginWhenCrossOrigin()
            .AddPermissionsPolicy(builder =>
            {
                builder.AddDefaultSecureDirectives();
                builder.AddAutoplay().Self();
                builder.AddEncryptedMedia().Self();
                builder.AddFullscreen().Self();
                builder.AddPictureInPicture().Self();
            });
}
