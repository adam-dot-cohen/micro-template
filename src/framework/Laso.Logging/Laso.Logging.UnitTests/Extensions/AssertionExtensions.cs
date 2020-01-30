using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Laso.Logging.UnitTests.Extensions
{
    public static class AssertionExtensions
    {
        public static void ShouldBeTrue(this bool condition, string userMessage=null)
        {
            if (userMessage == null)
            {
                Assert.True(condition);
            }
            else
            {
                Assert.True(condition, userMessage);
            }

        }
        public static void ShouldBeFalse(this bool condition, string userMessage=null)
        {
            if (userMessage == null)
            {
                Assert.False(condition);
            }
            else
            {
                Assert.False(condition, userMessage);
            }
        }
        
        public static void ShouldEqual<T>(this T expected, T actual)
        {
            Assert.Equal<T>(expected, actual);
        }


    }
}
