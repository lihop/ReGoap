#define DEBUG
using System;

namespace ReGoap.Utilities
{
    public static class ReGoapLogger
    {
#if UNITY_5_3_OR_NEWER
        private class UnityTraceListener : IListener
        {
            public void Write(string message)
            {
                Write(message, "");
            }

            public void Write(string message, string category)
            {
                switch (category)
                {
                    case "error":
                        UnityEngine.Debug.LogError(message);
                        break;
                    case "warning":
                        UnityEngine.Debug.LogWarning(message);
                        break;
                    default:
                        UnityEngine.Debug.Log(message);
                        break;
                }
            }
        }
#elif GODOT
        private class GodotTraceListener : IListener
        {
            public void Write(string message)
            {
                Write(message, "");
            }

            public void Write(string message, string category)
            {
                switch (category)
                {
                    case "error":
                        global::Godot.GD.PushError(message);
                        global::Godot.GD.PrintErr(message);
                        break;
                    case "warning":
                        global::Godot.GD.PushWarning(message);
                        global::Godot.GD.Print(message);
                        break;
                    default:
                        global::Godot.GD.Print(message);
                        break;
                }
            }
        }
#else
    private class GenericTraceListener : IListener
    {
        public void Write(string message)
        {
            Write(message, "");
        }

        public void Write(string message, string category)
        {
            Console.WriteLine(message);
        }
    }
#endif

        [Flags]
        public enum DebugLevel
        {
            Off, Fatal, Error, Warn, Info, Debug, Trace,
        }
        public static DebugLevel Level = DebugLevel.Info;
        public static bool RunOnlyOnMainThread = true;

        private static readonly IListener listener;

        private static readonly int mainThreadId;

        static ReGoapLogger()
        {
            mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

#if UNITY_5_3_OR_NEWER
            listener = new UnityTraceListener();
#elif GODOT
            listener = new GodotTraceListener();
#else
            listener = new GenericTraceListener();
#endif
        }

        private static bool InMainThread()
        {
            return !RunOnlyOnMainThread || System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId;
        }

        public static void LogTrace(string message)
        {
            if (Level < DebugLevel.Trace || !InMainThread()) return;
            listener.Write(message, "trace");
        }

        public static void LogDebug(string message)
        {
            if (Level < DebugLevel.Debug || !InMainThread()) return;
            listener.Write(message, "debug");
        }

        [Obsolete("Log is deprecated. Use the LogInfo method instead.")]
        public static void Log(string message) => LogInfo(message);

        public static void LogInfo(string message)
        {
            if (Level < DebugLevel.Info || !InMainThread()) return;
            listener.Write(message, "info");
        }

        public static void LogWarning(string message)
        {
            if (Level < DebugLevel.Warn || !InMainThread()) return;
            listener.Write(message, "warning");
        }

        public static void LogError(string message)
        {
            if (Level < DebugLevel.Error || !InMainThread()) return;
            listener.Write(message, "error");
        }
    }

    internal interface IListener
    {
        void Write(string text);
        void Write(string text, string category);
    }
}
