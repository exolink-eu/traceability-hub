namespace ExoLink.TraceabilityHub.Client.Models;

public sealed class Article
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public List<string>? Varieties { get; init; }
}
