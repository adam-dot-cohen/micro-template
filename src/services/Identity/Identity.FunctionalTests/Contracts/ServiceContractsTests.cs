using System;
using System.Linq;
using System.Reflection;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Identity.Api.V1;
using Laso.Identity.Core.Extensions;
using Xunit;

namespace Laso.Identity.FunctionalTests.Contracts
{
    public class ServiceContractsTests
    {
        [Fact]
        public void Proto_files_should_not_have_breaking_changes()
        {
            typeof(Partners).Assembly.GetTypes()
                .Select(x => x.GetCustomAttribute<BindServiceMethodAttribute>(false)?.BindType)
                .Where(x => x != null)
                .Select(x => ((ServiceDescriptor) x.GetProperty(nameof(Partners.Descriptor)).GetValue(null)).File)
                .Distinct(x => x.Name)
                .ToList()
                .To(x => new
                {
                    Methods = x
                        .SelectMany(y => y.Services, (y, z) => new { Proto = y.Name, Service = z })
                        .SelectMany(y => y.Service.Methods, (y, z) => new
                        {
                            y.Proto,
                            Service = y.Service.FullName,
                            z.Name,
                            InputType = z.InputType.FullName,
                            OutputType = z.OutputType.FullName
                        }),
                    Fields = x
                        .SelectMany(y => y.MessageTypes, (y, z) => new { Proto = y.Name, MessageType = z })
                        .SelectMany(y => y.MessageType.Fields.InFieldNumberOrder(), (y, z) => new
                        {
                            y.Proto,
                            MessageType = y.MessageType.FullName,
                            z.Name,
                            z.Index,
                            Type = GetType(z),
                            z.IsRequired,
                            z.IsRepeated
                        }),
                    EnumValues = x
                        .SelectMany(y => y.EnumTypes, (y, z) => new { Proto = y.Name, EnumType = z })
                        .SelectMany(y => y.EnumType.Values, (y, z) => new
                        {
                            y.Proto,
                            EnumType = y.EnumType.FullName,
                            z.Name,
                            z.Number
                        })
                })
                .ApproveIf(x =>
                {
                    return x.Approved.Methods.All(y => x.Received.Methods.Contains(y))
                           && x.Approved.Fields.All(y => x.Received.Fields.Contains(y))
                           && x.Approved.EnumValues.All(y => x.Received.EnumValues.Contains(y));
                });
        }

        private static string GetType(FieldDescriptor field)
        {
            switch (field.FieldType)
            {
                case FieldType.Message:
                    return field.MessageType.FullName;
                case FieldType.Enum:
                    return field.EnumType.FullName;
                case FieldType.Group:
                    throw new NotSupportedException();
                default:
                    return field.FieldType.ToString();
            }
        }
    }
}
