using System;
using System.Collections.Generic;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Laso.AdminPortal.Web.Api.Filters
{
    public class HandleRpcExceptionsAttribute : ActionFilterAttribute
    {
        private static readonly Dictionary<StatusCode, Func<RpcException, IActionResult>> StatusCodeMapping = 
            new Dictionary<StatusCode, Func<RpcException, IActionResult>>
            {
                { StatusCode.Unauthenticated, e => new UnauthorizedResult() },
                { StatusCode.PermissionDenied, e => new ForbidResult() },
                { StatusCode.AlreadyExists, e => new ConflictObjectResult(e.ToErrorResult()) },
                { StatusCode.NotFound, e => new NotFoundObjectResult(e.ToErrorResult()) },

                // Bad Request
                { StatusCode.FailedPrecondition, e => new BadRequestObjectResult(e.ToErrorResult()) },
                { StatusCode.InvalidArgument, e => new BadRequestObjectResult(e.ToErrorResult()) },
                { StatusCode.OutOfRange, e => new BadRequestObjectResult(e.ToErrorResult()) },
            };

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is RpcException rpcException && StatusCodeMapping.TryGetValue(rpcException.Status.StatusCode, out var resultFunc))
            {
                context.Result = resultFunc(rpcException);
                context.Exception = null; // mark as handled
            }

            base.OnActionExecuted(context);
        }
    }
}