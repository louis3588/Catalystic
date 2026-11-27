using System.Text;
using CatalysticBackend.Models;

namespace CatalysticBackend.Service;

public static class CalendarFile
{
    public static byte[] Create(CalendarEvent item, DateTime now)
    {
        var lines = new List<string>
        {
            "BEGIN:VCALENDAR", "VERSION:2.0", "PRODID:-//Asistym//Calendar//EN",
            "BEGIN:VEVENT", $"UID:event-{item.Id}@asistym", $"DTSTAMP:{now:yyyyMMddTHHmmssZ}",
            $"SUMMARY:{Escape(item.Title)}", $"DESCRIPTION:{Escape(item.Description)}"
        };

        if (item is TimedEvent timed)
        {
            lines.Add($"DTSTART:{timed.StartsAt:yyyyMMddTHHmmssZ}");
            lines.Add($"DTEND:{timed.EndsAt:yyyyMMddTHHmmssZ}");
        }
        else if (item is ClientTask { DueAt: not null } task)
        {
            lines.Add($"DTSTART;VALUE=DATE:{task.DueAt.Value:yyyyMMdd}");
            lines.Add($"DTEND;VALUE=DATE:{task.DueAt.Value.AddDays(1):yyyyMMdd}");
        }
        else throw new ArgumentException("The event has no calendar date.", nameof(item));

        lines.Add("END:VEVENT");
        lines.Add("END:VCALENDAR");
        return Encoding.UTF8.GetBytes(string.Join("\r\n", lines.Select(Fold)) + "\r\n");
    }

    private static string Escape(string value) => value.Replace("\\", "\\\\")
        .Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\\n")
        .Replace(";", "\\;").Replace(",", "\\,");

    private static string Fold(string line)
    {
        var result = new StringBuilder();
        var bytes = 0;
        foreach (var rune in line.EnumerateRunes())
        {
            if (bytes + rune.Utf8SequenceLength > 75)
            {
                result.Append("\r\n ");
                bytes = 1;
            }
            result.Append(rune.ToString());
            bytes += rune.Utf8SequenceLength;
        }
        return result.ToString();
    }
}
