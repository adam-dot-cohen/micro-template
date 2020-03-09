using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ApprovalTests;
using ApprovalTests.Core;
using ApprovalTests.Namers.StackTraceParsers;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;
using ApprovalTests.Reporters.Windows;
using ApprovalUtilities.CallStack;

namespace Laso.Identity.FunctionalTests
{
    public static partial class ApprovalExtensions
    {
        private const string ProjectName = "Identity.FunctionalTests";
        private static readonly object Lock = new object();
        private static readonly IApprovalFailureReporter DefaultReporter = new XUnit2Reporter();
        private static readonly IApprovalFailureReporter DefaultTestRunnerReporter = new VisualStudioReporter();
        private static readonly string[] TestRunnerProcessNames = { /*"JetBrains.ReSharper.TaskRunner", "nCrunch.TestHost",*/ "msvsmon", "devenv" };

        public static void Approve(this string data)
        {
            lock (Lock)
            {
                var approvalTextWriter = new ApprovalTextWriter(data);
                var namer = new EnvironmentAwareNamer();
                var reporter = GetReporter();

                Approvals.Verify(approvalTextWriter, namer, reporter);
            }
        }

        private static IApprovalFailureReporter GetReporter()
        {
            var reporter = Approvals.CurrentCaller.GetFirstFrameForAttribute<UseReporterAttribute>()?.Reporter;

            if (reporter != null)
                return reporter;

            var currentProcess = Process.GetCurrentProcess();
            var process = currentProcess.Parent() ?? currentProcess;
            var currentProcessName = Path.GetFileName(process.MainModule.FileName);

            if (TestRunnerProcessNames.Any(n => currentProcessName.StartsWith(n)))
                reporter = DefaultTestRunnerReporter;

            return reporter ?? DefaultReporter;
        }

        private static string FindIndexedProcessName(int pid)
        {
            var processName = Process.GetProcessById(pid).ProcessName;
            var processesByName = Process.GetProcessesByName(processName);
            string indexedProcessName = null;

            for (var i = 0; i < processesByName.Length; ++i)
            {
                indexedProcessName = i == 0 ? processName : processName + "#" + i;
                var processId = new PerformanceCounter("Process", "ID Process", indexedProcessName);

                if ((int) processId.NextValue() == pid)
                    return indexedProcessName;
            }

            return indexedProcessName;
        }

        private static Process FindPidFromIndexedProcessName(string indexedProcessName)
        {
            var parentId = new PerformanceCounter("Process", "Creating Process ID", indexedProcessName);

            return Process.GetProcessById((int)parentId.NextValue());
        }

        private static Process Parent(this Process process)
        {
            return FindPidFromIndexedProcessName(FindIndexedProcessName(process.Id));
        }

        private class EnvironmentAwareNamer : IApprovalNamer
        {
            private readonly IStackTraceParser _stackTraceParser;

            public string Name => _stackTraceParser.ApprovalName;
            public string SourcePath => _stackTraceParser.SourcePath;

            public EnvironmentAwareNamer()
            {
                Approvals.SetCaller();
                _stackTraceParser = new FunctionalTestStackParser();
                _stackTraceParser.Parse(Approvals.CurrentCaller.StackTrace);
            }
        }

        private class FunctionalTestStackParser : AttributeStackTraceParser
        {
            public override string GetAttributeType() => "Xunit.FactAttribute";

            public override string ForTestingFramework => "xUnit.net";

            public override string TypeName => GetMethodTypeName();

            public override string SourcePath => GetSourcePath();

            public override Caller FindApprovalFrame()
            {
                var asyncCaller = GetAsyncFactAttributeCaller(caller);
                return asyncCaller ?? base.FindApprovalFrame();
            }

            private Caller GetAsyncFactAttributeCaller(Caller caller)
            {
                var testAttributeType = GetAttributeType();

                var asyncMethods = caller.Callers.Where(c => c.Method.DeclaringType.GetInterfaces().Any(i => i == typeof(IAsyncStateMachine)));

                var asyncFactCaller = asyncMethods
                    .FirstOrDefault(c => c.Method.DeclaringType.DeclaringType.GetMethods()
                        .Any(m => (m.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType == c.Method.DeclaringType)
                                  && m.GetCustomAttributes(false).Any(a => a.GetType().FullName.StartsWith(testAttributeType))));

                return asyncFactCaller;
            }

            private string GetSourcePath()
            {
                var fileNameForStack = GetFileNameForStack(approvalFrame);
                var oldPath = Path.GetDirectoryName(fileNameForStack);

                var startIndex1 = oldPath.LastIndexOf(ProjectName) + ProjectName.Length + 1;
                var length = oldPath.Length - startIndex1;

                var subPath = length > 0 ? oldPath.Substring(startIndex1, length) : string.Empty;
                var startIndex2 = Environment.CurrentDirectory.IndexOf("bin" + Path.DirectorySeparatorChar + "Debug", StringComparison.InvariantCultureIgnoreCase);

                if (startIndex2 < 0)
                    throw new NotSupportedException(Environment.CurrentDirectory);

                var newPath = Path.GetFullPath(Environment.CurrentDirectory.Substring(0, startIndex2)) + subPath;

                return newPath;
            }

            private static Type GetMethodType(Caller caller)
            {
                var type = caller.Method.DeclaringType;

                if (type.GetInterfaces().Any(i => i == typeof(IAsyncStateMachine)))
                    type = type.DeclaringType;

                return type;
            }

            private string GetMethodTypeName()
            {
                var type = GetMethodType(approvalFrame);

                return GetTypeName(type);
            }

            private static string GetTypeName(Type type)
            {
                var name = type.Name;

                if (type.DeclaringType == null)
                    return name;

                var fullName = $"{GetTypeName(type.DeclaringType)}+{name}";

                return fullName;
            }

            public override string GetMethodName()
            {
                return approvalFrame.Method.DeclaringType.GetInterfaces().Any(i => i == typeof(IAsyncStateMachine))
                    ? approvalFrame.Method.DeclaringType.FullName.Split('<', '>')[1]
                    : base.GetMethodName();
            }
        }
    }
}