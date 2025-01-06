using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CCVC;

class FFmpegFrameExtractor
{
    private readonly string _ffmpegPath;

    public FFmpegFrameExtractor(string ffmpegPath = "ffmpeg.exe")
    {
        _ffmpegPath = ffmpegPath;
    }

    public MemoryStream ExtractSoundToMemory(string videoPath)
    {
        var memory = new MemoryStream();
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = $"-i \"{videoPath}\" -f mp3 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();

        Task.Run(() =>
        {
            using (var errorStream = process.StandardError)
            {
                string errorMessage;
                while ((errorMessage = errorStream.ReadLine()) != null)
                {
                    Console.WriteLine(errorMessage);
                }
            }
        });

        using (var outputStream = process.StandardOutput.BaseStream)
        {
            process.StandardOutput.BaseStream.CopyTo(memory);
        }

        process.WaitForExit();
        memory.Position = 0;
        return memory;
    }
    public double GetFPS(string videoPath)
    {
        var memory = new MemoryStream();
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = $"-i \"{videoPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();

        string output = process.StandardError.ReadToEnd();
        process.WaitForExit();

        string fpsPattern = @"(\d+(?:\.\d+)?)\s*fps";
        Match match = Regex.Match(output, fpsPattern);

        if (match.Success)
            return double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);

        throw new Exception("Failed to get the FPS of the video");
    }

    public void ExtractFramesToMemory(string videoPath, double fps, Action<MemoryStream> onFrameReceived)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = $"-i \"{videoPath}\" -vf fps={fps} -f image2pipe -vcodec bmp -",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();

        Task.Run(() =>
        {
            using (var errorStream = process.StandardError)
            {
                string errorMessage;
                while ((errorMessage = errorStream.ReadLine()) != null)
                {
                    // Обработка ошибок (например, логирование)
                    Console.WriteLine(errorMessage);
                }
            }
        });

        using (var outputStream = process.StandardOutput.BaseStream)
        {
            while (true)
            {
                try
                {
                    // Считываем один кадр BMP
                    var frameStream = ReadBmpFrameToMemory(outputStream);
                    if (frameStream == null) break;

                    // Вызываем callback для кадра
                    onFrameReceived(frameStream);
                }
                catch (EndOfStreamException)
                {
                    break;
                }
            }
        }

        process.WaitForExit();
    }
    private MemoryStream ReadBmpFrameToMemory(Stream stream)
    {
        var buffer = new byte[54]; // Заголовок BMP имеет фиксированный размер 54 байта
        if (stream.Read(buffer, 0, 54) != 54)
            return null;

        // Проверяем сигнатуру BMP (первые 2 байта: 42 4D, "BM")
        if (buffer[0] != 0x42 || buffer[1] != 0x4D)
            return null;

        // Извлекаем размер BMP-файла (байты 2–5 в заголовке)
        int fileSize = BitConverter.ToInt32(buffer, 2);

        // Читаем остальные данные BMP
        var ms = new MemoryStream();
        ms.Write(buffer, 0, 54);

        var remainingBytes = fileSize - 54;
        var readBuffer = new byte[4096];
        while (remainingBytes > 0)
        {
            int bytesRead = stream.Read(readBuffer, 0, Math.Min(readBuffer.Length, remainingBytes));
            if (bytesRead == 0)
                break;

            ms.Write(readBuffer, 0, bytesRead);
            remainingBytes -= bytesRead;
        }

        ms.Position = 0;
        return ms;
    }
}
