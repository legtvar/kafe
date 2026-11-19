using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Xunit;

namespace Kafe.Tests;

public class ProjectBlueprintTests
{
    [Fact]
    public void TemporaryLemmaProjectBlueprint_LicenseFiles_HasExpectedBlobShardAndLimits()
    {
        var licenseFiles = ProjectBlueprint.TemporaryLemmaProjectBlueprint
            .ArtifactBlueprints[Kafe.Const.LicenseFileBlueprintSlot];

        Assert.Equal(0, licenseFiles.Arity.Min);
        Assert.Equal(50, licenseFiles.Arity.Max);
        Assert.Equal(10L << 20, ProjectService.LicenseFileMaxFileLength);

        var shard = Assert.Single(licenseFiles.ShardBlueprints);
        Assert.Equal(ShardKind.Blob, shard.Key);
        Assert.Equal(ArgumentArity.ExactlyOne, shard.Value.Arity);
    }
}
