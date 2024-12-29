
namespace Skyfall.Models;

public record Message(string GameId, string SenderId, string Text)
{
    public string? RecipientId { get; set; }
    public DateTime Timestamp { get; internal set; } = DateTime.Now;
    public bool IsQuestion { get; internal set; }
    public bool IsAnswer { get; internal set; }
    public static Message Send(Player sender, string message, Player? recipient)
        => new(sender.GameId, sender.Id, message) { RecipientId = recipient?.Id };
    public static Message Question(Player sender, Player recipient, string question)
        => new(sender.GameId, sender.Id, question) { RecipientId = recipient?.Id, IsQuestion = true };
    public static Message Answer(Player sender, string recipientId, string question)
        => new(sender.GameId, sender.Id, question) { RecipientId = recipientId, IsAnswer = true };

}
