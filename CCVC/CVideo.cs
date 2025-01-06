using Google.FlatBuffers;
using NAudio.Wave;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;

namespace CCVC;

internal class CVideo
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

    public async Task PlayInConsole()
    {
        Console.Clear();

        ConsoleHelper.FontInfo info = new();
        ConsoleHelper.GetCurrentConsoleFontEx(1, true, ref info);

        ConsoleHelper.SetCurrentFont("Consolas", 5);
        Console.WindowWidth = Console.WindowWidth * 3;
        Console.WindowHeight = Console.WindowHeight * 3;

        // Открываем аудио
        using var vorbisStream = new WaveFileReader(new MemoryStream(_sound));
        using var waveOut = new WaveOutEvent();
        waveOut.Init(vorbisStream);
        waveOut.Play();

        var stopwatch = Stopwatch.StartNew();
        var targetFrameTime = TimeSpan.FromSeconds(1.0 / FPS);

        for (int i = 0; i < _frames.Length; i++)
        {
            var elapsedTime = stopwatch.Elapsed;
            var expectedTime = TimeSpan.FromSeconds(i / FPS);

            if (elapsedTime < expectedTime)
            {
                // Ждём до следующего кадра
                var delay = expectedTime - elapsedTime;
                await Task.Delay(delay);
            }
            else
            {
                // Пропускаем кадры, если отстаём
                while (elapsedTime > expectedTime && i < _frames.Length - 1)
                {
                    i++;
                    expectedTime = TimeSpan.FromSeconds(i / (double)FPS);
                }
            }

            // Отрисовка кадра
            Console.Write(_frames[i]);
            Console.CursorLeft = 0;
            Console.CursorTop = 0;
        }

        // Восстанавливаем шрифт и размеры окна
        ConsoleHelper.SetCurrentFont("Consolas");
        Console.WindowWidth = Console.WindowWidth / 3;
        Console.WindowHeight = Console.WindowHeight / 3;
    }

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

    public static CVideo ConvertFromVideo(string filename)
    {
        List<string> list = new();
        var ffmpeg = new FFmpegFrameExtractor();

        double fps = ffmpeg.GetFPS(filename);

        {
            // Потокобезопасный словарь для сохранения результата с учётом индекса
            var framesDict = new ConcurrentDictionary<int, string>();

            // Счётчик индексов для сохранения порядка кадров
            int frameIndex = 0;
            ffmpeg.ExtractFramesToMemory(filename, fps, frameStream =>
            {
                // Копируем индекс для текущей задачи
                int currentIndex = Interlocked.Increment(ref frameIndex) - 1;

                // Обрабатываем кадр в отдельной задаче
                var frame = ASCIIArtGenerator.Convert(frameStream);
                framesDict[currentIndex] = frame;
                
            });

            // Упорядочиваем и сохраняем в List после завершения
            list = framesDict.OrderBy(kv => kv.Key).Select(kv => kv.Value).ToList();
        }

        var audio = ffmpeg.ExtractSoundToMemory(filename);

        {
            using (var reader = new Mp3FileReader(audio))
            {
                var mem = new MemoryStream();
                WaveFileWriter.WriteWavFileToStream(mem, reader);

                audio = new MemoryStream(mem.ToArray());
            }
            using var wavReader = new WaveFileReader(audio);

            // Создаём целевой формат: 8 кГц, 8 бит, 2 канала (моно/стерео в зависимости от исходника)
            var lowQualityFormat = new WaveFormat(8000, 8, wavReader.WaveFormat.Channels);

            // Конвертируем WAV в целевой формат
            using var conversionStream = new WaveFormatConversionStream(lowQualityFormat, wavReader);

            // Сохраняем преобразованный WAV в память
            using var outputWavStream = new MemoryStream();
            using (var wavWriter = new WaveFileWriter(outputWavStream, conversionStream.WaveFormat))
            {
                conversionStream.CopyTo(wavWriter);
            }

            // Возвращаем данные преобразованного WAV
            audio = outputWavStream;
        }

        var video = new CVideo(list.ToArray(), fps, audio.ToArray());

        GC.Collect();
        return video;
    }
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
                    stream.Write(buffer, 0, bytesRead); // Записываем только прочитанные байты
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
