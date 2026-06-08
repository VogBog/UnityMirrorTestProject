namespace Game.Scripts.GameSystems.Repositories
{
    public static class PlayerRepositoryExtensions
    {
        public static bool IsMyPlayer(this IPlayerRepository repository, uint playerId)
            => repository.IsMyPlayer(repository.GetPlayerObject(playerId).Identity);
    }
}