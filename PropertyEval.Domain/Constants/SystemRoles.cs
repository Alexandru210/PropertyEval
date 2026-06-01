namespace PropertyEval.Domain.Constants;

public static class SystemRoles
{
    public const int ClientId = 1;
    public const int AdminId = 2;

    public const string Client = "Client";
    public const string Admin = "Admin";

    public static readonly string[] AssignableRoles = [Client, Admin];
}
