using CatalysticBackend.Data;
using CatalysticBackend.Domain;
using CatalysticBackend.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CatalysticBackend.Service;



public sealed class AssessmentService(CatalysticDbContext db, TimeProvider clock)
{
    public async Task<List<object>> ListClientAsync(int clientId, CancellationToken token)
    {
        var records = await db.WellnessAssessments.AsNoTracking().Where(a => a.ClientData.UserId == clientId)
            .OrderBy(a => a.AssessmentDate).ThenBy(a => a.Id).ToListAsync(token);
        return records.Select(a => (object)new {
            a.AssessmentDate,
            Score = (a.Physical + a.Dietary + a.Social + a.Emotional + a.Intellectual + a.Habitual + a.Financial) / 7m,
        }).ToList();
    }

    public async Task<List<object>> ListAsync(int clientId, CancellationToken token)
    {
        var records = await db.WellnessAssessments.AsNoTracking().Where(a => a.ClientData.UserId == clientId)
            .OrderBy(a => a.AssessmentDate).ThenBy(a => a.Id).ToListAsync(token);
        return records.Select(a => {
            var scores = new WellnessScores(a.Physical, a.Dietary, a.Social, a.Emotional, a.Intellectual, a.Habitual, a.Financial, a.PREOS);
            return (object)new { a.Id, a.AssessmentDate, a.Physical, a.Dietary, a.Social, a.Emotional, a.Intellectual, a.Habitual, a.Financial, a.PREOS, scores.Total, scores.Mean };
        }).ToList();
    }

    public static bool IsValid(AssessmentInput input)
    {
        decimal?[] scores = [input.Physical, input.Dietary, input.Social, input.Emotional, input.Intellectual, input.Habitual, input.Financial, input.PREOS];
        return input.AssessmentDate.HasValue && scores.All(value => value.HasValue && value >= 0 && value <= 10 && decimal.Round(value.Value, 2) == value.Value);
    }

    public async Task<string?> CreateAsync(int clientId, AssessmentInput input, CancellationToken token)
    {
        if (!IsValid(input)) return "Invalid";
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == clientId && u.Role == "Client", token);
        if (user is null) return "NotFound";
        var clientData = await db.ClientData.SingleOrDefaultAsync(data => data.UserId == clientId, token);
        if (clientData is null)
        {
            clientData = new ClientData { UserId = user.Id, FirstName = user.FirstName, LastName = user.LastName };
            db.ClientData.Add(clientData);
        }
        if (await db.WellnessAssessments.AnyAsync(a => a.ClientData.UserId == clientId && a.AssessmentDate == input.AssessmentDate, token)) return "Duplicate";
        db.WellnessAssessments.Add(new WellnessAssessment {
            ClientData = clientData, AssessmentDate = input.AssessmentDate!.Value,
            Physical = input.Physical!.Value, Dietary = input.Dietary!.Value, Social = input.Social!.Value,
            Emotional = input.Emotional!.Value, Intellectual = input.Intellectual!.Value,
            Habitual = input.Habitual!.Value, Financial = input.Financial!.Value, PREOS = input.PREOS!.Value,
            CreatedAt = clock.GetUtcNow().UtcDateTime,
        });
        try { await db.SaveChangesAsync(token); }
        catch (DbUpdateException error) when (error.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return "Duplicate";
        }
        return null;
    }
}
