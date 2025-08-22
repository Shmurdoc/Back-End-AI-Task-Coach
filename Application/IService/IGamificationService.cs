namespace Application.IService;

public interface IGamificationService
{
    // Add method signatures as needed, e.g.:
    // Task AwardPointsAsync(Guid userId, int points);

    // Controller-compatible stub
    Task<bool> DetectRelapseAsync(Guid userId, CancellationToken cancellationToken = default);
}
