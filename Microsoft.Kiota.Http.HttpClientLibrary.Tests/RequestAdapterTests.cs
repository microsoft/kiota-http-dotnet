using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions.Store;
using Microsoft.Kiota.Http.HttpClientLibrary.Tests.Mocks;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Tests
{
    public class HttpClientRequestAdapterTests
    {
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly HttpClientRequestAdapter requestAdapter;

        public HttpClientRequestAdapterTests()
        {
            _authenticationProvider = new Mock<IAuthenticationProvider>().Object;
            requestAdapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider());
        }

        [Fact]
        public void ThrowsArgumentNullExceptionOnNullAuthenticationProvider()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new HttpClientRequestAdapter(null));
            Assert.Equal("authenticationProvider", exception.ParamName);
        }

        [Fact]
        public void EnablesBackingStore()
        {
            // Arrange
            var requestAdapter = new HttpClientRequestAdapter(_authenticationProvider);
            var backingStore = new Mock<IBackingStoreFactory>().Object;

            //Assert the that we originally have an in memory backing store
            Assert.IsAssignableFrom<InMemoryBackingStoreFactory>(BackingStoreFactorySingleton.Instance);

            // Act
            requestAdapter.EnableBackingStore(backingStore);

            //Assert the backing store has been updated
            Assert.IsAssignableFrom(backingStore.GetType(), BackingStoreFactorySingleton.Instance);
        }


        [Fact]
        public void GetRequestMessageFromRequestInformationWithBaseUrlTemplate()
        {
            // Arrange
            requestAdapter.BaseUrl = "http://localhost";
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = "{+baseurl}/me"
            };

            // Act
            var requestMessage = requestAdapter.GetRequestMessageFromRequestInformation(requestInfo);

            // Assert
            Assert.NotNull(requestMessage.RequestUri);
            Assert.Contains("http://localhost/me", requestMessage.RequestUri.OriginalString);
        }

        [Theory]
        [InlineData("select", new[] { "id", "displayName" }, "select=id,displayName")]
        [InlineData("count", true, "count=true")]
        [InlineData("skip", 10, "skip=10")]
        [InlineData("skip", null, "")]// query parameter no placed
        public void GetRequestMessageFromRequestInformationSetsQueryParametersCorrectlyWithSelect(string queryParam, object queryParamObject, string expectedString)
        {
            // Arrange
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = "http://localhost/me{?top,skip,search,filter,count,orderby,select}"
            };
            requestInfo.QueryParameters.Add(queryParam, queryParamObject);

            // Act
            var requestMessage = requestAdapter.GetRequestMessageFromRequestInformation(requestInfo);

            // Assert
            Assert.NotNull(requestMessage.RequestUri);
            Assert.Contains(expectedString, requestMessage.RequestUri.Query);
        }

        [Fact]
        public void GetRequestMessageFromRequestInformationSetsContentHeaders()
        {
            // Arrange
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.PUT,
                UrlTemplate = "https://sn3302.up.1drv.com/up/fe6987415ace7X4e1eF866337"
            };
            requestInfo.Headers.Add("Content-Length", "26");
            requestInfo.Headers.Add("Content-Range", "bytes 0-25/128");
            requestInfo.SetStreamContent(new MemoryStream(Encoding.UTF8.GetBytes("contents")));

            // Act
            var requestMessage = requestAdapter.GetRequestMessageFromRequestInformation(requestInfo);

            // Assert
            Assert.NotNull(requestMessage.Content);
            // Content length set correctly
            Assert.Equal(26,requestMessage.Content.Headers.ContentLength);
            // Content range set correctly
            Assert.Equal("bytes", requestMessage.Content.Headers.ContentRange.Unit);
            Assert.Equal(0, requestMessage.Content.Headers.ContentRange.From);
            Assert.Equal(25, requestMessage.Content.Headers.ContentRange.To);
            Assert.Equal(128,requestMessage.Content.Headers.ContentRange.Length);
            Assert.True(requestMessage.Content.Headers.ContentRange.HasRange);
            Assert.True(requestMessage.Content.Headers.ContentRange.HasLength);
            // Content type set correctly
            Assert.Equal("application/octet-stream", requestMessage.Content.Headers.ContentType.MediaType);

        }
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [InlineData(HttpStatusCode.Accepted)]
        [InlineData(HttpStatusCode.NonAuthoritativeInformation)]
        [InlineData(HttpStatusCode.PartialContent)]
        [Theory]
        public async void SendStreamReturnsUsableStream(HttpStatusCode statusCode) {
            var mockHandler = new Mock<HttpMessageHandler>();
            var client = new HttpClient(mockHandler.Object);
            mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = statusCode,
                Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("Test")))
            });
            var adapter = new HttpClientRequestAdapter(_authenticationProvider, httpClient: client);
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = "https://example.com"
            };

            var response = await adapter.SendPrimitiveAsync<Stream>(requestInfo);

            Assert.True(response.CanRead);
            Assert.Equal(4, response.Length);
        }
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [InlineData(HttpStatusCode.Accepted)]
        [InlineData(HttpStatusCode.NonAuthoritativeInformation)]
        [InlineData(HttpStatusCode.NoContent)]
        [Theory]
        public async void SendStreamReturnsNullForNoContent(HttpStatusCode statusCode) {
            var mockHandler = new Mock<HttpMessageHandler>();
            var client = new HttpClient(mockHandler.Object);
            mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = statusCode,
            });
            var adapter = new HttpClientRequestAdapter(_authenticationProvider, httpClient: client);
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = "https://example.com"
            };

            var response = await adapter.SendPrimitiveAsync<Stream>(requestInfo);

            Assert.Null(response);
        }
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [InlineData(HttpStatusCode.Accepted)]
        [InlineData(HttpStatusCode.NonAuthoritativeInformation)]
        [InlineData(HttpStatusCode.NoContent)]
        [InlineData(HttpStatusCode.PartialContent)]
        [Theory]
        public async void SendSNoContentDoesntFailOnOtherStatusCodes(HttpStatusCode statusCode) {
            var mockHandler = new Mock<HttpMessageHandler>();
            var client = new HttpClient(mockHandler.Object);
            mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = statusCode,
            });
            var adapter = new HttpClientRequestAdapter(_authenticationProvider, httpClient: client);
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = "https://example.com"
            };

            await adapter.SendNoContentAsync(requestInfo);
        }
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [InlineData(HttpStatusCode.Accepted)]
        [InlineData(HttpStatusCode.NonAuthoritativeInformation)]
        [InlineData(HttpStatusCode.NoContent)]
        [InlineData(HttpStatusCode.ResetContent)]
        [Theory]
        public async void SendReturnsNullOnNoContent(HttpStatusCode statusCode) {
            var mockHandler = new Mock<HttpMessageHandler>();
            var client = new HttpClient(mockHandler.Object);
            mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = statusCode,
            });
            var adapter = new HttpClientRequestAdapter(_authenticationProvider, httpClient: client);
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = "https://example.com"
            };

            var response = await adapter.SendAsync<MockEntity>(requestInfo, MockEntity.Factory);

            Assert.Null(response);
        }
        [Fact]
        public async void RetriesOnCAEResponse() {
            var mockHandler = new Mock<HttpMessageHandler>();
            var client = new HttpClient(mockHandler.Object);
            var methodCalled = false;
            mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>((mess, token) => {
                var response = new HttpResponseMessage {
                    StatusCode = methodCalled ? HttpStatusCode.OK : HttpStatusCode.Unauthorized,
                    Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("Test")))
                };
                if (!methodCalled)
                    response.Headers.WwwAuthenticate.Add(new("Bearer", "realm=\"\", authorization_uri=\"https://login.microsoftonline.com/common/oauth2/authorize\", client_id=\"00000003-0000-0000-c000-000000000000\", error=\"insufficient_claims\", claims=\"eyJhY2Nlc3NfdG9rZW4iOnsibmJmIjp7ImVzc2VudGlhbCI6dHJ1ZSwgInZhbHVlIjoiMTY1MjgxMzUwOCJ9fX0=\""));
                methodCalled = true;
                return Task.FromResult(response);
            });
            var adapter = new HttpClientRequestAdapter(_authenticationProvider, httpClient: client);
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = "https://example.com"
            };

            var response = await adapter.SendPrimitiveAsync<Stream>(requestInfo);

            Assert.NotNull(response);

            mockHandler.Protected().Verify("SendAsync", Times.Exactly(2), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }
    }
}
