using MelonLoader;

namespace GorillaTrials.Tools
{
    internal class Logging
    {
        public static void Message(object message) => Log(message, "Message");

        public static void Info(object message) => Log(message, "Info");

        public static void Warning(object message) => Log(message, "Warning");

        public static void Error(object message) => Log(message, "Error");

        public static void Fatal(object message) => Log(message, "Fatal");

        private static void Log(object message, string level)
        {
            switch (level)
            {
                case "Warning":
                    MelonLogger.Warning(message.ToString());
                    break;
                case "Error":
                    MelonLogger.Error(message.ToString());
                    break;
                case "Fatal":
                    MelonLogger.Error(message.ToString());
                    break;
                case "Info":
                    MelonLogger.Msg(message.ToString());
                    break;
                case "Message":
                    MelonLogger.Msg(message.ToString());
                    break;
                default:
                    MelonLogger.Msg(message.ToString());
                    break;
            }
        }
    }
}