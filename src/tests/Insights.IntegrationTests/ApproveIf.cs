using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Laso.Insights.IntegrationTests.Extensions;
using Laso.TableStorage.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Laso.Insights.IntegrationTests
{
    public static partial class ApprovalExtensions
    {
        public static void ApproveIf<T>(this T received, Func<(T Received, T Approved), bool> predicate, Func<(T Received, T Approved), bool> overwriteApproval = null) where T : class
        {
            if (received == null)
                throw new ArgumentNullException(nameof(received));

            var serializerIo = new Serializer(new JsonSerializerSettings { Formatting = Formatting.Indented });

            ApproveIf(received, predicate, overwriteApproval, serializerIo, serializerIo);
        }

        private static void ApproveIf<T>(T received, Func<(T Received, T Approved), bool> predicate, Func<(T Received, T Approved), bool> overwriteApproval, Serializer serializer, Serializer deserializer) where T : class
        {
            var approvalNamer = new EnvironmentAwareNamer();

            var approvedPath = Path.Combine(approvalNamer.SourcePath, approvalNamer.Name) + ".approved.txt";

            T approved = null;

            string approvedText = null;

            if (File.Exists(approvedPath))
            {
                approvedText = File.ReadAllText(approvedPath);

                approved = deserializer.Deserialize<T>(approvedText);
            }

            var receivedText = serializer.Serialize(received);

            if (approved == null || predicate((received, approved)))
            {
                if (receivedText != approvedText && (approved == null || overwriteApproval == null || overwriteApproval((received, approved))))
                    File.WriteAllText(approvedPath, receivedText);
            }
            else
            {
                receivedText.Approve();
            }
        }

        public static void ApproveState<T>(this T received, Expression<Func<T, object>> ignore, params Expression<Func<T, object>>[] ignores) where T : class
        {
            received.ApproveState(true, ignore, ignores);
        }

        public static void ApproveState<T>(this T received, bool overwriteApproval, Expression<Func<T, object>> ignore, params Expression<Func<T, object>>[] ignores) where T : class
        {
            var ignoredProperties = ignore.Concat(ignores).Select(x => x.GetProperty()).ToLookup(x => x.Name);

            received.ApproveState(overwriteApproval, x => ignoredProperties[x.PropertyName].Any(y => y.DeclaringType == x.DeclaringType));
        }

        public static void ApproveState<T>(this T received, params string[] ignore) where T : class
        {
            received.ApproveState(true, ignore);
        }

        public static void ApproveState<T>(this T received, bool overwriteApproval, params string[] ignore) where T : class
        {
            ApproveState(received, overwriteApproval, x => ignore.Contains(x.PropertyName));
        }

        public static void ApproveState<T>(this T received, bool overwriteApproval, Func<JsonProperty, bool> ignore) where T : class
        {
            if (received == null)
                throw new ArgumentNullException(nameof(received));

            var specifiedProperties = new Dictionary<Type, ICollection<SpecifiedProperty>>();

            var ignoredProperties = new List<PropertyInfo>(typeof(TableStorageEntity).GetProperties());

            var deserializer = new Serializer(new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects }, (x, y) =>
            {
                PopulateProperties(x, y, specifiedProperties);

                if (!overwriteApproval)
                    ignoredProperties.AddRange(specifiedProperties.SelectMany(a => a.Key.GetProperties().Where(b => a.Value.All(c => c.Name != b.Name))));
            });

            var serializer = new Serializer(new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new PropertyNameContractResolver(x => ignore(x) || ignoredProperties.Any(y => x.PropertyName == y.Name && x.DeclaringType == y.DeclaringType)),
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });

            ApproveIf(received, x => MatchesSpecification(x.Received, x.Approved, specifiedProperties, new HashSet<object>(), true), x => overwriteApproval, serializer, deserializer);
        }

        private static bool MatchesSpecification(object received, object approved, IDictionary<Type, ICollection<SpecifiedProperty>> specifiedProperties, ISet<object> visitedObjects, bool isVisiting)
        {
            if (isVisiting)
                visitedObjects.Add(approved);

            var properties = specifiedProperties.Get(approved.GetType());

            return properties == null || properties
               .All(x =>
               {
                   if (!x.IsStillPresent)
                       return false;

                   var receivedValue = x.GetValue(received);
                   var approvedValue = x.GetValue(approved);

                   bool IsValueMatch(object y, object z) => ReferenceEquals(y, null) && ReferenceEquals(z, null) || z?.Equals(y) == true;

                   if (IsValueType(x.Type))
                   {
                       return IsValueMatch(receivedValue, approvedValue);
                   }

                   if (x.Type.Closes(typeof(IList<>), out var genericTypeArguments))
                   {
                       var receivedList = ((IList) receivedValue).Cast<object>().ToList();
                       var approvedList = ((IList) approvedValue).Cast<object>().ToList();

                       if (receivedList.Count < approvedList.Count)
                           return false;

                       var zip = approvedList.Zip(receivedList.Take(approvedList.Count), (y, z) => new { Received = z, Approved = y });

                       return IsValueType(genericTypeArguments[0])
                           ? zip.All(y => IsValueMatch(y.Received, y.Approved))
                           : zip.All(y => visitedObjects.Contains(y.Approved) || MatchesSpecification(y.Received, y.Approved, specifiedProperties, visitedObjects, isVisiting));
                   }

                   if (x.Type.Closes(typeof(IEnumerable<>), out genericTypeArguments))
                   {
                       var receivedList = ((IEnumerable) receivedValue).Cast<object>().ToList();
                       var approvedList = ((IEnumerable) approvedValue).Cast<object>().ToList();

                       if (receivedList.Count < approvedList.Count)
                           return false;

                       if (IsValueType(genericTypeArguments[0]))
                           return approvedList.All(y => receivedList.Any(z => IsValueMatch(z, y)));

                       var matches = approvedList.All(y => visitedObjects.Contains(y) || receivedList.Any(z => MatchesSpecification(z, y, specifiedProperties, visitedObjects, false)));

                       if (isVisiting)
                           approvedList.ForEach(y => visitedObjects.Add(y));

                       return matches;
                   }

                   return visitedObjects.Contains(approvedValue) || MatchesSpecification(receivedValue, approvedValue, specifiedProperties, visitedObjects, isVisiting);
               });
        }

        private static void PopulateProperties(Type type, JToken obj, IDictionary<Type, ICollection<SpecifiedProperty>> specifiedProperties)
        {
            if (specifiedProperties.ContainsKey(type))
                return;

            var jsonProperties = obj
                .Children<JProperty>()
                .ToList();

            if (jsonProperties.Count == 1 && jsonProperties[0].Name == "$ref")
                return;

            var properties = type.GetProperties().ToDictionary(x => x.Name);

            specifiedProperties.Add(type, jsonProperties
                .Where(x => !x.Name.StartsWith("$"))
                .Select(x => new SpecifiedProperty { Name = x.Name, Property = properties.Get(x.Name) })
                .ToList());

            jsonProperties
                .Where(x => x.Value.Type == JTokenType.Object && properties.ContainsKey(x.Name) && !IsValueType(properties[x.Name].PropertyType))
                .ForEach(x => PopulateProperties(properties[x.Name].PropertyType, x.Value, specifiedProperties));

            jsonProperties
                .Where(x => x.Value.Type == JTokenType.Array && properties.ContainsKey(x.Name))
                .Select(x => new { x.Name, FirstChild = x.Value.ToObject<JArray>().FirstOrDefault(y => y.Children<JProperty>().Any(z => !z.Name.StartsWith("$"))) })
                .Where(x => x.FirstChild != null && !IsValueType(properties[x.Name].PropertyType.GetListType()))
                .ForEach(x => PopulateProperties(properties[x.Name].PropertyType.GetListType(), x.FirstChild, specifiedProperties));
        }

        private static bool IsValueType(Type type)
        {
            //TODO: specify more value types that can be compared with .Equals()
            return type.IsPrimitive || type == typeof(string) || type.IsEnum;
        }

        private class SpecifiedProperty
        {
            public string Name { get; set; }
            public PropertyInfo Property { private get; set; }

            public bool IsStillPresent => Property != null;

            public Type Type => Property?.PropertyType;

            public object GetValue(object obj)
            {
                return Property?.GetValue(obj);
            }
        }

        private class PropertyNameContractResolver : DefaultContractResolver
        {
            private readonly Func<JsonProperty, bool> _ignore;

            public PropertyNameContractResolver(Func<JsonProperty, bool> ignore)
            {
                _ignore = ignore;
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var properties = base.CreateProperties(type, memberSerialization);

                return properties.Where(p => !_ignore(p)).ToList();
            }
        }

        private class Serializer
        {
            private readonly JsonSerializerSettings _settings;
            private readonly Action<Type, JToken> _onDeserialize;

            public Serializer(JsonSerializerSettings settings, Action<Type, JToken> onDeserialize = null)
            {
                _settings = settings;
                _onDeserialize = onDeserialize;
            }

            public T Deserialize<T>(string value)
            {
                _onDeserialize?.Invoke(typeof(T), JObject.Parse(value));

                return JsonConvert.DeserializeObject<T>(value, _settings);
            }

            public string Serialize(object received)
            {
                return JsonConvert.SerializeObject(received, _settings);
            }
        }
    }
}