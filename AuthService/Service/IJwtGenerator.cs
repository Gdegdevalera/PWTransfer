namespace AuthService.Service
{
    public interface IJwtGenerator
    {
        string GenerateJwt(Data.User user);
    }
}
