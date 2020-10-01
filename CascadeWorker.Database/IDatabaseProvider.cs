namespace CascadeWorker.Database
{
    public interface IDatabaseProvider
    {
        DatabaseConnection GetConnection();
    }
}