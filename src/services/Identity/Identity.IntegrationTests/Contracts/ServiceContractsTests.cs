using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Identity.Api.V1;
using Laso.Identity.Core.Extensions;
using Laso.Identity.Core.IntegrationEvents;
using Laso.Identity.Infrastructure.Extensions;
using Xunit;

namespace Laso.Identity.IntegrationTests.Contracts
{
    public class ServiceContractsTests
    {
        [Fact]
        public void Proto_files_should_not_have_breaking_changes()
        {
            string GetType(FieldDescriptor field)
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
                            Type = GetType(z),
                            z.FieldNumber,
                            z.IsRequired,
                            z.IsRepeated,
                            // z.IsPacked, This seems to be bugged
                            z.IsMap,
                            ContainingOneof = z.ContainingOneof?.Name
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
                .ApproveIf(x => x.Approved.Methods.All(y => x.Received.Methods.Contains(y))
                                && x.Approved.Fields.All(y => x.Received.Fields.Contains(y))
                                && x.Approved.EnumValues.All(y => x.Received.EnumValues.Contains(y))
                                && x.Received.EnumValues.All(y => x.Approved.EnumValues.Contains(y)));
        }

        [Fact]
        public void Integration_events_should_not_have_breaking_changes()
        {
            void VisitMessageTypes(Type type, ICollection<Type> messageTypes, ICollection<Type> enumTypes)
            {
                if (messageTypes.Contains(type))
                    return;

                messageTypes.Add(type);

                type.GetProperties()
                    .Select(x => x.PropertyType)
                    .ForEach(x =>
                    {
                        if (x.IsPrimitive || x == typeof(string) || x == typeof(DateTime) || x == typeof(Guid))
                            return;

                        if (x.IsEnum)
                            enumTypes.Add(x);

                        if (x.Closes(typeof(IEnumerable<>), out var args))
                            VisitMessageTypes(args[0], messageTypes, enumTypes);

                        VisitMessageTypes(x, messageTypes, enumTypes);
                    });
            }

            typeof(IIntegrationEvent).Assembly.GetTypes()
                .Where(x => typeof(IIntegrationEvent).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .ToList()
                .To(x =>
                {
                    var messageTypes = new HashSet<Type>();
                    var enumTypes = new HashSet<Type>();

                    x.ForEach(y => VisitMessageTypes(y, messageTypes, enumTypes));

                    return new
                    {
                        Fields = messageTypes.SelectMany(y => y.GetProperties(), (y, z) => new
                        {
                            MessageType = y.FullName,
                            z.Name,
                            Type = z.PropertyType.FullName
                        }),
                        EnumValues = enumTypes.SelectMany(y => Enum.GetValues(y).Cast<Enum>(), (y, z) => new
                        {
                            EnumType = y.FullName,
                            Name = z.ToString(),
                            Value = z.GetValue()
                        })
                    };
                })
                .ApproveIf(x => x.Approved.Fields.All(y => x.Received.Fields.Contains(y))
                                && x.Approved.EnumValues.All(y => x.Received.EnumValues.Contains(y))
                                && x.Received.EnumValues.All(y => x.Approved.EnumValues.Contains(y)));
        }
    }
}
