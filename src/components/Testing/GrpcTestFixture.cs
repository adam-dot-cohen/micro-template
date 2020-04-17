﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace Laso.Testing
{
    public class GrpcTestFixture
    {
        private readonly Lazy<HostTestFixture> _hostFixture;
        private readonly Lazy<HttpClient> _httpClient;

        public GrpcTestFixture() : this(new HostTestFixture())
        {
        }

        public GrpcTestFixture(HostTestFixture hostFixture)
        {
            _hostFixture = new Lazy<HostTestFixture>(() => hostFixture);
            _httpClient = new Lazy<HttpClient>(CreateHttpClient);
        }

        public IHost Host => _hostFixture.Value.Host;
        public HttpClient Client => _httpClient.Value;
        public GrpcChannel Channel => CreateGrpcChannel();

        public static void ConfigureHost(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseTestServer());
        }

        private HttpClient CreateHttpClient()
        {
            var testServer = Host.GetTestServer();

            // Need to set the response version to 2.0.
            // Required because of this TestServer issue - https://github.com/aspnet/AspNetCore/issues/16940
            var responseVersionHandler = new ResponseVersionHandler
            {
                InnerHandler = testServer.CreateHandler()
            };

            var httpClient = new HttpClient(responseVersionHandler)
            {
                BaseAddress = testServer.BaseAddress
            };

            return httpClient;
        }

        private GrpcChannel CreateGrpcChannel()
        {
            return GrpcChannel.ForAddress(Client.BaseAddress, new GrpcChannelOptions { HttpClient = Client });
        }

        private class ResponseVersionHandler : DelegatingHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = await base.SendAsync(request, cancellationToken);
                response.Version = request.Version;

                return response;
            }
        }
    }
}