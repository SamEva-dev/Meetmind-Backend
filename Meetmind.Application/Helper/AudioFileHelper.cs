
namespace Meetmind.Application.Helper;

public static class AudioFileHelper
{
    public static string GenerateAudioPath(string meetingTitle, Guid meetingId)
    {
        var basePath = Path.Combine(AppContext.BaseDirectory, "Resources", "audio");
        if (!Directory.Exists(basePath))
            Directory.CreateDirectory(basePath);

        // Nettoyage du titre pour le nom de fichier
        var sanitizedTitle = string.Concat(meetingTitle.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        return Path.Combine(basePath, $"{sanitizedTitle}-{meetingId}-{timestamp}.wav");
    }

}
