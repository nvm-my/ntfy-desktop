namespace NtfyDesktop.Features.Settings;

public record ActiveHours(bool Enabled, TimeOnly Start, TimeOnly End)
{
    public bool Includes(TimeOnly time)
    {
        // Handles midnight-spanning ranges (e.g. 22:00–06:00)
        return Start <= End
            ? time >= Start && time <= End
            : time >= Start || time <= End;
    }

    public bool Excludes(TimeOnly time) => !Includes(time);

}
