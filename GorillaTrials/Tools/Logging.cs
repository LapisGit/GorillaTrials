using BepInEx.Logging;

namespace GorillaTrials.Tools
{
    internal class Logging
    {
        public static void Message(object message) => Log(LogLevel.Message, message);

        public static void Info(object message) => Log(LogLevel.Info, message);

        public static void Warning(object message) => Log(LogLevel.Warning, message);

        public static void Error(object message) => Log(LogLevel.Error, message);

        public static void Fatal(object message) => Log(LogLevel.Fatal, message);

        public static void Log(LogLevel level, object message)
        {
#if DEBUG
            Plugin.Logger?.Log(level, message);
#endif
        }
    }
}