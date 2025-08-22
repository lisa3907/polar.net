using System;
using PolarNet;
using Xunit;

namespace PolarNet.Tests;

public class PolarClientTests
{
    [Fact]
    public void Ctor_Throws_When_AccessToken_Missing()
    {
    Assert.Throws<ArgumentException>(() => new PolarClient(new PolarClientOptions{ AccessToken = "" }));
    }
}
