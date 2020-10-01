namespace CascadeWorker.Shared
{
    public static class StaticState
    {
        public static string LocalRemoteConfigUrl { get; } = "https://cdn.cascade.io/configuration/cascade-local-config.json";
        public static string RemoteConfigUrl { get; } = "https://cdn.cascade.io/configuration/cascade-config.json?v=3";
        public static int WorkerId { get; set; }
        public static string BugSnagApiKey = "******************************";
    }
}
