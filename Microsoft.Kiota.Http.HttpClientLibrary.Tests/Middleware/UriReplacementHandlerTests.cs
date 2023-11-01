using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
using Moq;
using Xunit;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Tests.Middleware;

public class UriReplacementOptionTests {
    [Fact]
    public void Does_Nothing_When_Url_Replacement_Is_Disabled()
    {
        var uri = new Uri("http://localhost/test");
        var disabled = new UriReplacementHandlerOption(false, new Dictionary<string, string>());

        Assert.Equal(uri, disabled.Replace(uri));

        disabled = new UriReplacementHandlerOption(false, new Dictionary<string, string>{
            {"test", ""}
        });

        Assert.Equal(uri, disabled.Replace(uri));
    }

    [Fact]
    public void Returns_Null_When_Url_Provided_Is_Null()
    {
        var disabled = new UriReplacementHandlerOption(false, new Dictionary<string, string>());

        Assert.Null(disabled.Replace(null));
    }

    [Fact]
    public void Replaces_Key_In_Path_With_Value()
    {
        var uri = new Uri("http://localhost/test");
        var disabled = new UriReplacementHandlerOption(true, new Dictionary<string, string>{{"test", ""}});

        Assert.Equal("http://localhost/", disabled.Replace(uri)!.ToString());
    }
}

public class UriReplacementHandlerTests
{
    [Fact]
    public async Task Calls_Uri_ReplacementAsync()
    {
        var mockReplacement = new Mock<IUriReplacementHandlerOption>();
        mockReplacement.Setup(static x => x.IsEnabled()).Returns(true);
        mockReplacement.Setup(static x => x.Replace(It.IsAny<Uri>())).Returns(new Uri("http://changed"));

        var handler = new UriReplacementHandler<IUriReplacementHandlerOption>(mockReplacement.Object)
        {
            InnerHandler = new FakeSuccessHandler()
        };
        var msg = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
        var client = new HttpClient(handler);
        await client.SendAsync(msg);

        mockReplacement.Verify(static x=> x.Replace(It.IsAny<Uri>()), Times.Once());
    }
}
