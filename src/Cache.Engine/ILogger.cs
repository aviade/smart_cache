namespace Cache.Engine
{
    public interface ILogger
    {
        void TraceInfo(string s);
        void TraceError(string s);
    }
}