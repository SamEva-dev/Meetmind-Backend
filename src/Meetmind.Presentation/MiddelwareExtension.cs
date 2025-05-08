using Meetmind.Presentation.Extensions;
using Meetmind.Presentation.Hubs;

namespace Meetmind.Presentation
{
    public static class MiddelwareExtension
    {
        public static WebApplication AppPresentation(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseSwaggerDocumentation();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<StateHub>("/hub/state");
            app.MapGet("/health", () => Results.Ok("Healthy"));


            return app;
        }
    }
}
