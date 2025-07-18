using NAudio.Wave;

namespace Meetmind.Infrastructure.Services.Recording.Interfaces;

public interface IWaveInFactory
{
    WaveInEvent Create(int sampleRate = 16000, int channels = 1);
}