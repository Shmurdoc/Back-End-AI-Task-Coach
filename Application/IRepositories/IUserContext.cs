namespace Application.IRepositories
{
    public interface IUserContext
    {
        Guid GetCurrentUserId();
    }
}
