using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AddressFulfilment.Shared.Utilities
{
    public static class TraceLogger
    {
        private const string MessageFormat = "[{0}{1}] {2} {3}";

        public static void Debug(string msg, string typeName, [CallerMemberName] string methodName = null)
        {
            Trace.TraceInformation(GetMessage(msg, typeName, methodName));
        }

        public static void Info(string msg, string typeName, [CallerMemberName] string methodName = null)
        {
            Trace.TraceInformation(GetMessage(msg, typeName, methodName));
        }

        public static void Warn(string msg, string typeName, Exception exception = null, [CallerMemberName] string methodName = null)
        {
            Trace.TraceWarning(GetMessage(msg, typeName, methodName, GetExceptionMessage(exception)));
        }

        public static void Error(string msg, string typeName, Exception exception = null, [CallerMemberName] string methodName = null)
        {
            Trace.TraceError(GetMessage(msg, typeName, methodName, GetExceptionMessage(exception)));
        }

        public static void ConsoleTitle(string message)
        {
            lock (Console.Out)
            {
                Console.Title = message;
            }
        }

        private static string GetMessage(string message, string typeName, string methodName = null, string exception = "")
        {
            return string.Format(MessageFormat, typeName, methodName == null ? string.Empty : "." + methodName, message, exception);
        }

        private static string GetExceptionMessage(Exception e)
        {
            switch (e)
            {
                case null:
                    return string.Empty;

                case TargetInvocationException _:
                    return GetExceptionMessage(e.InnerException);

                default:
                    return e.ToString();
            }
        }
    }
}