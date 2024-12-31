namespace Skyfall.Models;

public enum GameEvents
{
    CreateGame,
    JoinGame,
    SentMessage,
    StartGame,
    AskedQuestion,
    Voted,
    GameEnded,
    PlayerRemoved,
    GameCancelled,
    NextTurn,
    AnsweredQuestion,
    NextRound,
    PlayComplete
}

public record GameEvent(GameEvents Event, string PlayerId, string Message);

public delegate void GameEventHandler(Game sender, GameEvent e);