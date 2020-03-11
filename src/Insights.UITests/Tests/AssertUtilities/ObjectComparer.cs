using System;
using NUnit.Framework;

namespace Insights.UITests.Tests.AssertUtilities
{
    public class AssertObjectComparer<T> 
    {
        
        public void Compare (T actual, T expected,String[] ignoreMembers = null) 
        {
            var comparer = new ObjectsComparer.Comparer<T>();

            if (ignoreMembers != null)
            {
                foreach (string eachIgnoreMember in ignoreMembers)
                {
                    comparer.IgnoreMember(eachIgnoreMember);
                }
            }
        
            bool isEqual = comparer.Compare(actual, expected,
                out var differences);
            string dif = string.Join(Environment.NewLine, differences);
            dif = dif.Replace("Value1", "Actual").Replace("Value2", "Expected");
            Assert.True(isEqual,
                "The comparison between the expected and actual values for the object type "+actual.GetType().Name +" resulted in differences " +
                dif);
        }

    }
}
