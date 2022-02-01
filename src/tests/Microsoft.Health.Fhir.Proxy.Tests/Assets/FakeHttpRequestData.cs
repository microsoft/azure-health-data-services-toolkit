using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Fhir.Proxy.Tests.Assets
{
    public class FakeHttpRequestData : HttpRequestData
    {
        public FakeHttpRequestData(FunctionContext context, string method, string uriString, Stream stream, HttpHeadersCollection headers)
            : base(context)
        {
            this.method = method;
            this.uriString = uriString;
            this.stream = stream;
            this.headers = headers;
        }

        private readonly string method;
        private readonly string uriString;
        private readonly Stream stream;
        private readonly HttpHeadersCollection headers;

        public override Stream Body => stream;

        public override HttpHeadersCollection Headers => headers;

        public override IReadOnlyCollection<IHttpCookie> Cookies => throw new NotImplementedException();

        public override Uri Url => new(uriString);

        public override IEnumerable<ClaimsIdentity> Identities => throw new NotImplementedException();

        public override string Method => method;

        public override HttpResponseData CreateResponse()
        {
            return new FakeHttpResponseData(new FakeFunctionContext(), System.Net.HttpStatusCode.Created);
        }
    }
}
