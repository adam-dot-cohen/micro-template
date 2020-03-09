using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Laso.DataImport.Domain.Quarterspot.Enumerations
{
    public interface IEnumeration
    {
        object Value { get; }
        string DisplayName { get; }        
    }    

    [Serializable]
    [DebuggerDisplay("{DisplayName} - {Value}")]
    public abstract class Enumeration<TEnumeration> : Enumeration<TEnumeration, int>
        where TEnumeration : Enumeration<TEnumeration>
    {
        protected Enumeration(int value, string displayName)
            : base(value, displayName)
        {
        }
    }

    [Serializable]
    [DebuggerDisplay("{DisplayName} - {Value}")]
    public abstract class Enumeration<TEnumeration, TValue> : IComparable<TEnumeration>, IEquatable<TEnumeration>, IEnumeration
        where TEnumeration : Enumeration<TEnumeration, TValue>
        where TValue : IComparable
    {
        private static readonly Lazy<TEnumeration[]> Enumerations = new Lazy<TEnumeration[]>(GetEnumerations);        

        protected Enumeration(TValue value, string displayName)
        {
            Value = value;
            DisplayName = displayName;
        }

        public TValue Value { get; }
        object IEnumeration.Value => Value;
        public string DisplayName { get; }

        public static TEnumeration[] GetAll()
        {
            return Enumerations.Value;
        }      

        public static bool operator ==(Enumeration<TEnumeration, TValue> left, Enumeration<TEnumeration, TValue> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Enumeration<TEnumeration, TValue> left, Enumeration<TEnumeration, TValue> right)
        {
            return !Equals(left, right);
        }

        public static implicit operator Enumeration<TEnumeration, TValue>(TValue value)
        {
            return FromValue(value);
        }

        public static implicit operator TValue(Enumeration<TEnumeration, TValue> value)
        {
            return value.Value;
        }

        public static TEnumeration FromValue(TValue value)
        {
            return Parse(value, "value", item => item.Value.Equals(value));
        }

        public static bool TryFromValue(TValue value, out TEnumeration result)
        {
            return TryParse(e => e.Value.Equals(value), out result);
        }

        public static TEnumeration FromDisplayName(string displayName)
        {
            return Parse(displayName, "display name", item => item.DisplayName.Equals(displayName));
        }

        public static bool TryFromDisplayName(string displayName, out TEnumeration result)
        {
            return TryParse(e => e.DisplayName.Equals(displayName), out result);
        }

        public static TEnumeration FromDisplayNameIgnoreCase(string displayName)
        {
            return Parse(displayName, "display name", item => String.Equals(item.DisplayName, displayName, StringComparison.OrdinalIgnoreCase));
        }

        public static bool TryFromDisplayNameIgnoreCase(string displayName, out TEnumeration result)
        {
            return TryParse(e => String.Equals(e.DisplayName, displayName, StringComparison.OrdinalIgnoreCase), out result);
        }

        public int CompareTo(TEnumeration other)
        {
            return Value.CompareTo(other.Value);
        }

        public override sealed string ToString()
        {
            return DisplayName;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TEnumeration);
        }

        public bool Equals(TEnumeration other)
        {
            return other != null && Value.Equals(other.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        protected static TEnumeration Parse(object value, string description, Func<TEnumeration, bool> predicate)
        {
            if (TryParse(predicate, out var result))
                return result;

            throw new ArgumentException($"'{value}' is not a valid {description} value in {typeof(TEnumeration)}");
        }

        private static bool TryParse(Func<TEnumeration, bool> predicate, out TEnumeration result)
        {
            result = GetAll().FirstOrDefault(predicate);
            return result != null;
        }

        private static TEnumeration[] GetEnumerations()
        {
            var enumerationType = typeof(TEnumeration);
            return enumerationType
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(info => enumerationType.IsAssignableFrom(info.FieldType))
                .Select(info => info.GetValue(null))
                .Cast<TEnumeration>()
                .ToArray();
        }
    }

    public static class Enumeration
    {
        public static IEnumerable<IEnumeration> GetAll(Type enumerationType)
        {
            var getAllMethod = enumerationType.GetMethod("GetAll", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (getAllMethod == null)
                throw new InvalidOperationException();

            return ((IEnumerable)getAllMethod.Invoke(null, new object[0])).Cast<IEnumeration>();
        }
     
        public static IEnumeration FromValue(Type enumerationType, object value)
        {
            var fromValueMethod = enumerationType.GetMethod("FromValue", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (fromValueMethod == null)
                throw new InvalidOperationException();

            return (IEnumeration)fromValueMethod.Invoke(null, new[] { value });
        }

        public static IEnumeration FromDisplayName(Type enumerationType, string displayName)
        {
            var fromDisplayNameMethod = enumerationType.GetMethod("FromDisplayName", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy, null, new[] { typeof(string) }, null);
            if (fromDisplayNameMethod == null)
                throw new InvalidOperationException();

            return (IEnumeration)fromDisplayNameMethod.Invoke(null, new object[] { displayName });
        }

        public static bool TryParse(Type enumerationType, string displayName, out IEnumeration enumeration)
        {
            var tryParseMethod = enumerationType.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string), enumerationType.MakeByRefType() }, null);
            if (tryParseMethod == null)
                throw new InvalidOperationException();

            var parameters = new object[] { displayName, null };
            var result = (bool)tryParseMethod.Invoke(null, parameters);

            enumeration = (IEnumeration)parameters[1];

            return result;
        }

        public static FieldInfo GetField(Type enumerationType, object enumeration)
        {
            return enumerationType
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .First(x => enumeration == x.GetValue(null));
        }
    }
}