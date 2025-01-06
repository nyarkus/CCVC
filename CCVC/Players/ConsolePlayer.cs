using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCVC.Players
{
    public class ConsolePlayer
    {
        public static async Task Play(CVideo video)
        {
            Console.Clear();

            ConsoleHelper.FontInfo info = new();
            ConsoleHelper.GetCurrentConsoleFontEx(1, true, ref info);

            ConsoleHelper.SetCurrentFont("Consolas", 5);
            Console.WindowWidth = Console.WindowWidth * 3;
            Console.WindowHeight = Console.WindowHeight * 3;

            var frames = video.Frames;

            using var vorbisStream = new WaveFileReader(video.Sound);
            using var waveOut = new WaveOutEvent();
            waveOut.Init(vorbisStream);
            waveOut.Play();

            var stopwatch = Stopwatch.StartNew();
            var targetFrameTime = TimeSpan.FromSeconds(1.0 / video.FPS);

            for (int i = 0; i < frames.Count; i++)
            {
                var elapsedTime = stopwatch.Elapsed;
                var expectedTime = TimeSpan.FromSeconds(i / video.FPS);

                if (elapsedTime < expectedTime)
                {
                    var delay = expectedTime - elapsedTime;
                    await Task.Delay(delay);
                }
                else
                {
                    while (elapsedTime > expectedTime && i < frames.Count - 1)
                    {
                        i++;
                        expectedTime = TimeSpan.FromSeconds(i / video.FPS);
                    }
                }

                Console.Write(frames[i]);
                Console.CursorLeft = 0;
                Console.CursorTop = 0;
            }
            ConsoleHelper.SetCurrentFont("Consolas");
            Console.WindowWidth = Console.WindowWidth / 3;
            Console.WindowHeight = Console.WindowHeight / 3;
        }
    }
}
