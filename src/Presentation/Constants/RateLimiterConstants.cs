namespace Presentation.Constants;

public static class RateLimiterConstants
{
    public const string AnonymousUserPolicy = "anonymous";
    public const short AnonymousUserPermitLimit = 30;
    public const short AnonymousUserWindowSeconds = 10;
}
