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
                .Where(x => x.GetCustomAttribute<BindServiceMethodAttribute>(false) != null)
                .Select(x => (ServiceDescriptor) x.DeclaringType.GetProperty(nameof(Partners.Descriptor)).GetValue(null))
                .Select(x => x.File)
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
                            InputType = z.InputType.Name,
                            OutputType = z.OutputType.Name
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
                case FieldType.Double:
                case FieldType.Float:
                case FieldType.Int64:
                case FieldType.UInt64:
                case FieldType.Int32:
                case FieldType.Fixed64:
                case FieldType.Fixed32:
                case FieldType.Bool:
                case FieldType.String:
                case FieldType.Bytes:
                case FieldType.UInt32:
                case FieldType.SFixed32:
                case FieldType.SFixed64:
                case FieldType.SInt32:
                case FieldType.SInt64:
                    return field.FieldType.ToString();
                case FieldType.Enum:
                    return field.EnumType.FullName;
                // case FieldType.Group: TODO
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
