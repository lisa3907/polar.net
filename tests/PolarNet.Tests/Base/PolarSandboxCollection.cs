using Xunit;

namespace PolarNet.Tests.Base
{
    /// <summary>
    /// Collection definition for sharing PolarSandboxFixture across multiple test classes.
    /// </summary>
    [CollectionDefinition(Name)]
    public class PolarSandboxCollection : ICollectionFixture<PolarSandboxFixture>
    {
        public const string Name = "PolarSandboxCollection";
    }
}