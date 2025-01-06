using Google.FlatBuffers;
using NAudio.Wave;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;

namespace CCVC;

public class CVideo
{
    public const int CurrentVersion = 1;

    private string[] _frames = new string[0];
    private double _fps;
    private byte[] _sound;
    private int _version;

    public IReadOnlyList<string> Frames { get { return _frames.ToList(); } }
    public double FPS { get { return _fps; } }
    public MemoryStream Sound { get { return new MemoryStream(_sound); } }
    public int Version { get { return _version; } }

    public void Save(Stream stream)
    {
        FlatBufferBuilder builder = new(1024);
        var sound = Schemes.CV.ConsoleVideo.CreateSoundVector(builder, _sound);

        List<StringOffset> strings = new();
        foreach (var frame in _frames)
            strings.Add(builder.CreateString(frame));
        var frames = Schemes.CV.ConsoleVideo.CreateFramesVector(builder, strings.ToArray());

        var video = Schemes.CV.ConsoleVideo.CreateConsoleVideo(builder, FPS, sound, frames, CurrentVersion);

        builder.Finish(video.Value);
        var buffer = builder.SizedByteArray();

        var gzip = new GZipStream(stream, CompressionLevel.SmallestSize);

        gzip.Write(buffer, 0, buffer.Length);
        gzip.Close();
        stream.Close();
        GC.Collect();
    }
    public void Save(string filename)
        => Save(File.Create(filename));

    
    public static CVideo Load(string filename)
    {
        byte[] bytes;
        using (MemoryStream stream = new())
        {
            using (GZipStream gzip = new(File.OpenRead(filename), CompressionMode.Decompress))
            {
                byte[] buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = gzip.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, bytesRead);
                }
            }

            bytes = stream.ToArray();
        }


        var video = Schemes.CV.ConsoleVideo.GetRootAsConsoleVideo(new(bytes));

        var version = video.Version;
        if(version > CurrentVersion)
        {
            throw new Exception("The file version is higher than the supported one. Loading is not possible");
        }

        List<string> frames = new();

        for (int i = 0; i < video.FramesLength; i++)
            frames.Add(video.Frames(i));

        List<byte> sound = new();
        
        for(int i = 0; i < video.SoundLength; i++)
            sound.Add(video.Sound(i));

        GC.Collect();
        return new CVideo(frames.ToArray(), video.Fps, sound.ToArray(), version);
    }

    public CVideo(string[] frames, double fps, byte[] sound, int version = CurrentVersion)
    {
        _frames = frames;
        _fps = fps;
        _sound = sound;
    }
}
