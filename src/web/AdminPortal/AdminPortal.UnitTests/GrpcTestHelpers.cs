using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;


namespace Laso.AdminPortal.UnitTests
{
    public static class GrpcTestHelpers
    {
        public static AsyncUnaryCall<TReply> AsGrpcCall<TReply>(this TReply reply,
            Metadata responseHeaders = null, 
            Status? status = null,
            Metadata trailers = null
        )
            where TReply : IMessage<TReply>
        {
            responseHeaders ??= new Metadata();
            Func<Status> getStatusFunc = () => Status.DefaultSuccess;
            if (status.HasValue)
            {
                getStatusFunc = () => status.Value;
            }
            Func<Metadata> getTrailersFunc = () => new Metadata();
            if (trailers != null)
            {
                getTrailersFunc = () => trailers;
            }
            var unaryCall = new AsyncUnaryCall<TReply>(
                Task.FromResult(reply),
                Task.FromResult(responseHeaders),
                getStatusFunc,
                getTrailersFunc,
                () => { });


            return unaryCall;
        }
    }
}