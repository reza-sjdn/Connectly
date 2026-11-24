namespace Connectly.Domain.Entities.Stories;

public class StoryView
{
    public Guid StoryId { get; set; }
    public Guid ViewerId { get; set; }
    public DateTime ViewedAtUtc { get; set; }
}
