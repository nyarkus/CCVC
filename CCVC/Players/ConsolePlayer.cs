using CCVC.Decoder;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Threading;

namespace CCVC.Players
{
    public class ConsolePlayer
    {
        public static int GetMaxWidth()
        {
            int result;
            ConsoleHelper.SetCurrentFont("Consolas", 2);
            result = Console.LargestWindowWidth;
            ConsoleHelper.SetCurrentFont("Consolas", 16);
            return result;
        }
        public static int GetMaxHeight()
        {
            int result;
            ConsoleHelper.SetCurrentFont("Consolas", 2);
            result = Console.LargestWindowHeight;
            ConsoleHelper.SetCurrentFont("Consolas", 16);
            return result;
        }

        public static void Play(CVideo video)
        {
            Console.Clear();
            Console.CursorVisible = false;

            ConsoleHelper.FontInfo info = new();
            ConsoleHelper.GetCurrentConsoleFontEx(1, true, ref info);
            ConsoleHelper.SetCurrentFont("Consolas", 2);

            var oldWidth = Console.WindowWidth;
            var oldHeight = Console.WindowHeight;

            Console.SetWindowPosition(0, 0);
            Console.WindowWidth = video.Width + 1;
            Console.WindowHeight = video.Height;


            var decoder = new FrameDecoder(
                video.Width,
                video.Height,
                video.FPS,
                new CVideoFrameReader(video)
            );
            _ = Task.Run(decoder.RecalculateBuffer);
            while (decoder.LastDecodedFrame < decoder.BufferSize)
                Thread.Sleep(1);

            using var waveStream = new WaveFileReader(video.Sound);
            using var waveOut = new WaveOutEvent();
            waveOut.Init(waveStream);

            var swGlobal = Stopwatch.StartNew();
            waveOut.Play();

            double accumulatedTime = 0;
            double frameTime = 1.0 / video.FPS;
            int targetFrame = 0;

            while (targetFrame < video.GetFramesCount())
            {
                double audioTime = waveOut.GetPosition() / (double)waveStream.WaveFormat.AverageBytesPerSecond;
                targetFrame = (int)(audioTime * video.FPS);

                if (targetFrame > decoder.LastDecodedFrame)
                {
                    decoder.Seek(targetFrame);
                }

                string frame = decoder.ReadFrame(targetFrame);
                if (!string.IsNullOrEmpty(frame))
                {
                    Console.SetCursorPosition(0, 0);
                    Console.Write(frame);
                }
                else
                {
                    Debug.WriteLine($"Frame {targetFrame} not found in buffer");
                }

                accumulatedTime += frameTime;
                double sleepTime = accumulatedTime - swGlobal.Elapsed.TotalSeconds;
                if (sleepTime > 0)
                {
                    Thread.Sleep((int)(sleepTime * 1000));
                }
            }

            decoder = null;

            ConsoleHelper.SetCurrentFont("Consolas", 16);
            Console.CursorVisible = true;
            Console.WindowWidth = oldWidth;
            Console.WindowHeight = oldHeight;
        }
    }
}