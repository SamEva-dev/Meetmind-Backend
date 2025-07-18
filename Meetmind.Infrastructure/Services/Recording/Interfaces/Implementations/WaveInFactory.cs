using NAudio.Wave;

namespace Meetmind.Infrastructure.Services.Recording.Interfaces.Implementations;

internal sealed class WaveInFactory : IWaveInFactory
{
    public WaveInEvent Create(int sampleRate = 16000, int channels = 1) => new() { WaveFormat = new WaveFormat(sampleRate, channels) };
}
