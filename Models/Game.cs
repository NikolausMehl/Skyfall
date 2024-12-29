namespace Skyfall.Models;
public enum GameStates
{
    Recruting,
    Playing,
    Voting,
    Ended
}

public class Game
{
    public string Answerer { get; internal set; } = "";
    public bool CanCancelGame => State == GameStates.Recruting;
    public bool CanJoinGame => Players.Count < 9;
    public bool CanStartGame => State == GameStates.Recruting && Players.Count > 2;
    public List<Message> Chat { get; init; } = [];
    public string CommonWord { get; set; } = "";
    public required Category CurrentCategory { get; set; }
    public Player CurrentPlayer => Players[QuestionerId];
    public int CurrentRound { get; set; } = 1;
    public int VoteCount => Players.Count(x => x.HasVoted);
    public string GameId { get; init; } = GenerateGameCode(6);
    public string ImposterWord { get; set; } = "";
    public bool IsVotingPhase => State == GameStates.Voting;
    public Message LastQuestion => Chat.Last(x => x.IsQuestion);
    public int NumberOfRounds { get; set; }
    public List<Player> Players { get; set; } = [];
    public Dictionary<string, int> PlayerVotes { get; set; } = [];
    public int QuestionerId { get; set; }
    public GameStates State { get; set; } = GameStates.Recruting;
    public List<string> UsedImposterWordSet { get; set; } = [];
    public List<string> UsedWordSet { get; set; } = [];
    public int FirstQuestioner { get; internal set; }

    public static string GenerateGameCode(int length)
    {
        Random random = new();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(0, length)
           .Select(s => chars[random.Next(chars.Length)]).ToArray());
    }

    public string GetNextMove()
    {
        if (State != GameStates.Playing) return "";
        return Answerer.Length > 0
            ? $"{PlayerName(LastQuestion.RecipientId)} should answer {PlayerName(LastQuestion.SenderId)}"
            : $"{CurrentPlayer.Name} to lead the interogation";
    }

    public string GetOutcome()
    {
        if (State != GameStates.Ended) return string.Empty;
        if (IsVotingPhase) return "Waiting on final votes";
        var imposter = Players.Find(x => x.IsImposter);
        if (imposter == null) return "No imposter found";
        var imposterVotes = PlayerVotes.Count(x => x.Key == imposter.Id);
        var imposterWon = PlayerVotes.Count - imposterVotes > imposterVotes;
        var outcome = imposterWon ? "and evaided detection" : "were successfully identified";
        return $"{imposter.Name} was the imposter {outcome}";
    }

    public string PlayerName(string? playerId)
                => Players.FirstOrDefault(x => x.Id == playerId)?.Name ?? "everyone";
}
