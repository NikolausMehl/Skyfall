namespace Skyfall.Services;

public interface IGameService
{
    event GameEventHandler? GameEvent;

    bool IsDevelopment { get; }

    void CancelGame(string gameId);
    void Chat(Message message);
    Player CreateGame(string userName, string categoryName, int rounds);
    Player CreateGame(Player player, string category, int rounds);
    void EndGame(string gameId);

    Game GetGame(string gameId);
    bool HasGame(string gameId);
    Player JoinGame(string gameId, string userName);
    Player JoinGame(string gameId, Player player);
    void NextRound(string gameId);
    void NextTurn(string gameId);
    void RemovePlayer(Player player);
    void RestartGame(string gameId);

    bool StartGame(string gameId);
    void SubmitVote(string gameId, string votedPlayerId, string playerId);
}

public class GameService(AppState appState, ICategoryService categoryService) : IGameService
{
    private static readonly Random _random = new();

    public event GameEventHandler? GameEvent;

    public bool IsDevelopment => appState.IsDevelopment;
    public void CancelGame(string gameId)
    {
        var game = GetGame(gameId);
        var player = game.Players[0];
        GameEvent?.Invoke(game, new(GameEvents.GameCancelled, player.Id, $"{player.Name} cancelled the game"));
        appState.Games.Remove(game.GameId);
    }

    public void Chat(Message message)
    {
        var game = GetGame(message.GameId);
        game.Chat.Add(message);


        if (message.IsQuestion)
        {
            var action = $"{game.PlayerName(message.SenderId)} asked {game.PlayerName(message.RecipientId)}";
            game.Answerer = message.RecipientId!;
            GameEvent?.Invoke(game, new(GameEvents.AskedQuestion, message.SenderId, action));
        }
        else if (message.IsAnswer)
        {
            var action = $"{game.PlayerName(message.SenderId)} answered {game.PlayerName(message.RecipientId)}";
            game.Answerer = "";
            GameEvent?.Invoke(game, new(GameEvents.AnsweredQuestion, message.SenderId, action));
            NextTurn(game.GameId);
        }
        else
        {
            var action = message.RecipientId == null
           ? $"{game.PlayerName(message.SenderId)} send a message"
           : $"{game.PlayerName(message.SenderId)} said to {game.PlayerName(message.RecipientId)}";
            GameEvent?.Invoke(game, new(GameEvents.SendMessage, message.SenderId, action));
        }
    }

    public Player CreateGame(string userName, string categoryName, int rounds)
    {
        var game = BuildGame(categoryName, rounds);
        var player = new Player(userName, game.GameId);
        game.Players.Add(player);
        appState.Games.Add(game.GameId, game);
        GameEvent?.Invoke(game, new(GameEvents.CreateGame, player.Id, $"{player.Name} created the game"));
        return player;
    }

    public Player CreateGame(Player existingPlayer, string categoryName, int rounds)
    {
        var game = BuildGame(categoryName, rounds);
        var player = existingPlayer with
        {
            GameId = game.GameId,
            IsImposter = false,
            HasVoted = false,
            VotedForPlayerId = ""
        };
        game.Players.Add(player);
        appState.Games.Add(game.GameId, game);
        GameEvent?.Invoke(game, new(GameEvents.CreateGame, player.Id, $"{player.Name} created the game"));
        return player;
    }

    public void EndGame(string gameId)
    {
        var game = GetGame(gameId);
        game.State = GameStates.Ended;
        var player = game.Players[0];
        GameEvent?.Invoke(game, new(GameEvents.GameEnded, player.Id, game.GetOutcome()));
    }

    public Game GetGame(string gameId)
    {
        if (!appState.Games.TryGetValue(gameId, out var game)) throw new InvalidOperationException("Game not found");
        return game;
    }

    public bool HasGame(string gameId)
    {
        return appState.Games.ContainsKey(gameId);
    }

    public Player JoinGame(string gameId, string userName)
    {
        var game = GetGame(gameId);
        if (game.Players.Any(p => p.Name == userName)) throw new InvalidOperationException("Duplicate user name");
        var player = new Player(userName, game.GameId);
        game.Players.Add(player);
        GameEvent?.Invoke(game, new(GameEvents.JoinGame, player.Id, $"{player.Name} joined the game"));
        return player;
    }

    public Player JoinGame(string gameId, Player player)
    {
        var game = GetGame(gameId);
        if (game.Players.Any(p => p.Id == player.Id && p.GameId == game.GameId)) return player;
        var newPlayer = player with
        {
            GameId = game.GameId,
            IsImposter = false,
            HasVoted = false,
            VotedForPlayerId = ""
        };
        game.Players.Add(newPlayer);
        GameEvent?.Invoke(game, new(GameEvents.JoinGame, player.Id, $"{player.Name} joined the game"));
        return newPlayer;
    }

    public void NextRound(string gameId)
    {
        var game = GetGame(gameId);
        game.CurrentRound++;
        if (game.CurrentRound > game.NumberOfRounds)
        {
            game.State = GameStates.Voting;
            var player = game.Players[0];
            GameEvent?.Invoke(game, new(GameEvents.PlayComplete, player.Id, $"Players to vote"));
        }
        else
        {
            var player = game.CurrentPlayer;
            GameEvent?.Invoke(game, new(GameEvents.NextRound, player.Id, $"{player.Name} to play next"));
        }
    }

    public void NextTurn(string gameId)
    {
        var game = GetGame(gameId);
        var action = $"{game.CurrentPlayer.Name} has completed his turn";
        game.QuestionerId += 1;
        if (game.QuestionerId >= game.Players.Count)
            game.QuestionerId = 0;
        GameEvent?.Invoke(game, new(GameEvents.NextTurn, game.CurrentPlayer.Id, action));
        if (game.QuestionerId == game.FirstQuestioner)
            NextRound(gameId);
    }

    public void RemovePlayer(Player player)
    {
        var game = GetGame(player.GameId);
        game.Players.Remove(player);
        GameEvent?.Invoke(game, new(GameEvents.PlayerRemoved, player.Id, $"{player.Name} has disappeared"));
    }

    public void RestartGame(string gameId)
    {
        var game = GetGame(gameId);
        var availableCategories = categoryService.GetCategories().Where(x => x.Name != game.CurrentCategory.Name);
        int randomIndex = _random.Next(availableCategories.Count());
        game.CurrentCategory = availableCategories.ElementAt(randomIndex);
        game.State = GameStates.Recruting;
        game.PlayerVotes = [];
        game.ImposterWord = "";
        game.CommonWord = "";
        game.Answerer = "";
        game.QuestionerId = 0;
        var player = game.Players[0];
        GameEvent?.Invoke(game, new(GameEvents.CreateGame, player.Id, $"{player.Name} created the game"));
    }

    public bool StartGame(string gameId)
    {
        var game = GetGame(gameId);
        AssignRoles(game);
        var player = game.Players.First();
        game.State = GameStates.Playing;
        GameEvent?.Invoke(game, new(GameEvents.StartGame, player.Id, $"{player.Name} joined the game"));
        return true;
    }

    public void SubmitVote(string gameId, string votedPlayerId, string playerId)
    {
        var game = GetGame(gameId);

        Player player = game.Players.FirstOrDefault(p => p.Id == playerId) ?? throw new InvalidOperationException("Player not found");

        player.HasVoted = true;
        player.VotedForPlayerId = votedPlayerId;

        if (game.PlayerVotes.ContainsKey(votedPlayerId))
            game.PlayerVotes[votedPlayerId] += 1;
        else
            game.PlayerVotes.Add(votedPlayerId, 1);

        GameEvent?.Invoke(game, new(GameEvents.Voted, player.Id, $"{player.Name} voted"));

        if (game.Players.All(p => p.HasVoted))
            game.State = GameStates.Ended;
    }

    private static void AssignRoles(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        game.CommonWord = SelectCommonWord(game);
        game.ImposterWord = SelectImposterWord(game);
        SelectImposter(game);
        game.QuestionerId = game.FirstQuestioner = _random.Next(0, game.Players.Count);
    }

    private static string SelectCommonWord(Game game)
    {
        var unusedRegularWords = game.CurrentCategory.Words.Where(w => !game.UsedWordSet.Contains(w));
        if (!unusedRegularWords.Any())
        {
            game.UsedWordSet.Clear();
            unusedRegularWords = game.CurrentCategory.Words;
        }

        int randomIndex = _random.Next(unusedRegularWords.Count());
        string commonWord = unusedRegularWords.ElementAt(randomIndex);
        game.UsedWordSet.Add(commonWord);
        return commonWord;
    }

    private static void SelectImposter(Game game)
    {
        int imposterIndex = _random.Next(0, game.Players.Count);

        for (int i = 0; i < game.Players.Count; i++)
        {
            if (i == imposterIndex)
            {
                game.Players[i].IsImposter = true;
                game.Players[i].AssignedWord = game.ImposterWord;
            }
            else
            {
                game.Players[i].IsImposter = false;
                game.Players[i].AssignedWord = game.CommonWord;
            }
        }
    }

    private static string SelectImposterWord(Game game)
    {
        var unusedImposterWords = game.CurrentCategory.Words.Where(w => !game.UsedImposterWordSet.Contains(w) && w != game.CommonWord);
        if (unusedImposterWords.Any())
        {
            game.UsedImposterWordSet.Clear();
            unusedImposterWords = game.CurrentCategory.Words.Where(w => w != game.CommonWord);
            if (!unusedImposterWords.Any()) throw new InvalidOperationException("No available imposter words");
        }

        int randomIndex = _random.Next(unusedImposterWords.Count());
        string imposterWord = unusedImposterWords.ElementAt(randomIndex);
        game.UsedImposterWordSet.Add(imposterWord);
        return imposterWord;
    }

    private Game BuildGame(string categoryName, int rounds)
    {
        var category = categoryService.GetCategory(categoryName);
        if (category.Words == null) throw new NullReferenceException(nameof(category.Words));

        Game game = new() { CurrentCategory = category, NumberOfRounds = rounds };
        return game;
    }
}