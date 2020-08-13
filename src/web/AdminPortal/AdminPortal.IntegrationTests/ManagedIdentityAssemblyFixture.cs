using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

//// [assembly: TestFramework(IntegrationTestFramework.TypeName, IntegrationTestFramework.AssemblyName)]

namespace Laso.AdminPortal.IntegrationTests
{
    /// <summary>
    /// <para>
    /// This is an experiment in determining startup/finish of all tests executed in an
    /// assembly for xUnit.
    /// </para>
    /// <para>
    /// Uncommenting the [assembly] attribute will enable a proxy fixture which can
    /// process startup (constructor) and completion (OnAssemblyTestsComplete) tasks.
    /// This code was hosted with NCrunch, and seemed to work fine. Didn't test it
    /// with other runners, yet.
    /// </para>
    /// </summary>
    public class IntegrationTestFramework : ITestFramework
    {
        public const string AssemblyName = "Laso.AdminPortal.IntegrationTests";
        public const string TypeName = "Laso.AdminPortal.IntegrationTests.IntegrationTestFramework";

        private readonly XunitTestFramework _innerFramework;

        public IntegrationTestFramework(IMessageSink messageSink)
        {
            // Move this to a Lazy action which fires event in GetExecutor?
            // (i.e. closer to start of test execution) [jay_mclain]
            Console.WriteLine("+IntegrationTestFramework");
            _innerFramework = new XunitTestFramework(messageSink);
        }

        public ISourceInformationProvider SourceInformationProvider
        {
            set => _innerFramework.SourceInformationProvider = value;
        }

        public void Dispose()
        {
            _innerFramework.Dispose();
        }

        public ITestFrameworkDiscoverer GetDiscoverer(IAssemblyInfo assembly)
        {
            return _innerFramework.GetDiscoverer(assembly);
        }

        public ITestFrameworkExecutor GetExecutor(AssemblyName assemblyName)
        {
            var testFrameworkExecutor = _innerFramework.GetExecutor(assemblyName);
            return new IntegrationTestFrameworkExecutor(testFrameworkExecutor, OnAssemblyTestsComplete);
        }

        private static void OnAssemblyTestsComplete(string assemblyName, int testsRun, int testsFailed, int testsSkipped, TimeSpan executionTime)
        {
            Console.WriteLine("-IntegrationTestFramework");
        }

        private class IntegrationTestFrameworkExecutor : ITestFrameworkExecutor
        {
            private readonly ITestFrameworkExecutor _innerFrameworkExecutor;
            private readonly Action<string, int, int, int, TimeSpan> _assemblyTestsComplete;

            public IntegrationTestFrameworkExecutor(ITestFrameworkExecutor innerFrameworkExecutor, Action<string, int, int, int, TimeSpan> assemblyTestsComplete)
            {
                _innerFrameworkExecutor = innerFrameworkExecutor;
                _assemblyTestsComplete = assemblyTestsComplete;
            }

            public void Dispose()
            {
                _innerFrameworkExecutor.Dispose();
            }

            public ITestCase Deserialize(string value)
            {
                return _innerFrameworkExecutor.Deserialize(value);
            }

            public void RunAll(
                IMessageSink executionMessageSink,
                ITestFrameworkDiscoveryOptions discoveryOptions,
                ITestFrameworkExecutionOptions executionOptions)
            {
                _innerFrameworkExecutor.RunAll(executionMessageSink, discoveryOptions, executionOptions);
            }

            public void RunTests(
                IEnumerable<ITestCase> testCases,
                IMessageSink executionMessageSink,
                ITestFrameworkExecutionOptions executionOptions)
            {
                var integrationTestMessageSink = new IntegrationTestMessageSink(executionMessageSink, OnAssemblyTestsComplete);
                _innerFrameworkExecutor.RunTests(testCases, integrationTestMessageSink, executionOptions);
            }

            private void OnAssemblyTestsComplete(TestAssemblyFinished message)
            {
                _assemblyTestsComplete(
                    message.TestAssembly.Assembly.Name,
                    message.TestsRun,
                    message.TestsFailed,
                    message.TestsSkipped,
                    TimeSpan.FromSeconds((double)message.ExecutionTime));
            }

            private class IntegrationTestMessageSink : IMessageSink
            {
                private readonly IMessageSink _innerMessageSink;
                private readonly Action<TestAssemblyFinished> _completionCallback;

                public IntegrationTestMessageSink(IMessageSink innerMessageSink, Action<TestAssemblyFinished> completionCallback)
                {
                    _innerMessageSink = innerMessageSink;
                    _completionCallback = completionCallback;
                }

                public bool OnMessage(IMessageSinkMessage message)
                {
                    var result = _innerMessageSink.OnMessage(message);
                    
                    if (message is TestAssemblyFinished executionFinished)
                        _completionCallback(executionFinished);

                    return result;
                }
            }
        }
    }
}
