namespace WpfApplication1
{
    public interface IDuckDuckGoApiService
    {
        IDuckDuckGoApi Background { get; }
        IDuckDuckGoApi Speculative { get; }
        IDuckDuckGoApi UserInitiated { get; }
    }
}
