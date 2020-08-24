using Grpc.Net.Client;

namespace Laso.Testing
{
    public class GrpcTestFixture : HttpTestFixture
    {
        public GrpcTestFixture() 
            : this(new HostTestFixture())
        {
        }

        public GrpcTestFixture(HostTestFixture hostFixture)
            : base(hostFixture)
        {
        }

        public GrpcTestFixture(HttpTestFixture httpFixture)
        {
            // TODO: Replace underlying HttpClient
        }

        public GrpcChannel Channel => CreateGrpcChannel();

        private GrpcChannel CreateGrpcChannel()
        {
            return GrpcChannel.ForAddress(Client.BaseAddress, new GrpcChannelOptions { HttpClient = Client });
        }
    }
}