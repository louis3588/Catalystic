using CatalysticBackend.Service;
using System.Security.Claims;

namespace CatalysticBackend.Security;

public static class AssessmentEndpoints
{
    public static void MapAssessmentEndpoints(this WebApplication app)
    {
        app.MapGet("/api/client/assessments", async (AssessmentService service, HttpContext context, CancellationToken token) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            if (!int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) || id <= 0) return Results.Unauthorized();
            return Results.Ok(await service.ListClientAsync(id, token));
        }).RequireAuthorization("Client");
        var group = app.MapGroup("/api/admin/clients/{clientId:int}/assessments").RequireAuthorization("Admin");
        group.MapGet("/", async (int clientId, AssessmentService service, HttpContext context, CancellationToken token) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            return Results.Ok(await service.ListAsync(clientId, token));
        });
    }
}
