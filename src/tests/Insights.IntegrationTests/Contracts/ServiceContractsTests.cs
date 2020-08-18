using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Laso.Insights.IntegrationTests.Extensions;
using Laso.IntegrationEvents;
using Xunit;

namespace Laso.Insights.IntegrationTests.Contracts
{
    public class ServiceContractsTests
    {
        private static readonly ICollection<Type> AllReferencedTypes = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
            .Select(AssemblyName.GetAssemblyName)
            .Where(x => x.Name?.StartsWith("Laso") == true)
            .Select(Assembly.Load)
            .SelectMany(x => x.GetTypes())
            .ToList();

        [Fact]
        public void Proto_files_should_not_have_breaking_changes()
        {
            static string GetType(FieldDescriptor field)
            {
                return field.FieldType switch
                {
                    FieldType.Message => field.MessageType.FullName,
                    FieldType.Enum => field.EnumType.FullName,
                    FieldType.Group => throw new NotSupportedException(),
                    _ => field.FieldType.ToString()
                };
            }

            AllReferencedTypes
                .Select(x => x.GetCustomAttribute<BindServiceMethodAttribute>(false)?.BindType)
                .Where(x => x != null)
                .Select(x => ((ServiceDescriptor) x.GetProperty("Descriptor").GetValue(null)).File)
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
                            //z.IsPacked, This seems to be bugged
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
            static void VisitMessageTypes(Type type, ICollection<Type> messageTypes, ICollection<Type> enumTypes)
            {
                if (messageTypes.Contains(type))
                    return;

                messageTypes.Add(type);

                type.GetProperties()
                    .Select(x => x.PropertyType.GetNonNullableType())
                    .ForEach(x =>
                    {
                        //Add more "primitive" types here
                        if (x.IsPrimitive || x == typeof(string) || x == typeof(DateTime) || x == typeof(Guid) || x == typeof(DateTimeOffset))
                            return;

                        if (x.IsEnum)
                            enumTypes.Add(x);

                        VisitMessageTypes(x.Closes(typeof(IEnumerable<>), out var args) ? args.First()[0] : x, messageTypes, enumTypes);
                    });
            }

            AllReferencedTypes
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
                            Type = GetTypeName(z.PropertyType)
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

        private static string GetTypeName(Type type)
        {
            if (type != typeof(string) && type.Closes(typeof(IEnumerable<>), out var args))
               return $"List<{args.First()[0].FullName}>";

            return type.FullName;
        }
    }
}
