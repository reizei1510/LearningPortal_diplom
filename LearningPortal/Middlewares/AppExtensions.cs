namespace LearningPortal.Middlewares
{
    public static class AppExtensions
    {
        public static IApplicationBuilder UseHandleUser(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HandleUserMiddleware>();
        }
    }
}
