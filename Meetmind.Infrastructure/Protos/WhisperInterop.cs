using System.Runtime.InteropServices;
using System.Text;

namespace Meetmind.Infrastructure.Protos;

public static class WhisperInterop
{
    [DllImport("whisper.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int transcribe(string audioPath, StringBuilder output, int outputLen);
}
