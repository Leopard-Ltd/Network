namespace BaseWithBestHttp.Interfaces
{
    public interface IWrapResponse
    {
        string RootResponse { get; }
    }

    public interface IWrapRequest
    {
        string RootRequest { get; }
    }
}