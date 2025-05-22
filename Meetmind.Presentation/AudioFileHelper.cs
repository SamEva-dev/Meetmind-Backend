namespace Meetmind.Presentation;

public static class AudioFileHelper
{
    public static string AudioFolder =>
            Path.Combine(AppContext.BaseDirectory, "Resources", "audio");
    public static string GenerateAudioPath(string meetingTitle, Guid meetingId)
    {

        // Nettoyage du titre pour le nom de fichier
        var sanitizedTitle = string.Concat(meetingTitle.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        return Path.Combine(AudioFolder, $"{sanitizedTitle}-{meetingId}-{timestamp}.wav");
    }

    public static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

}
