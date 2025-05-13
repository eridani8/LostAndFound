namespace LostAndFound;

public class AppSettings
{
    public string ReportsDirectory { get; init; } = "Reports";

    public string ConnectionString { get; init; } = "Server=localhost;Database=LostAndFound;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;";
} 