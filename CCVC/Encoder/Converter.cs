using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCVC.Encoder
{
    public class Converter
    {
        public static CVideo ConvertFromVideo(string filename)
        {
            List<string> list = new();
            var ffmpeg = new FFmpegManager();

            double fps = ffmpeg.GetFPS(filename);

            {
                var framesDict = new ConcurrentDictionary<int, string>();

                int frameIndex = 0;
                ffmpeg.ExtractFramesToMemory(filename, fps, frameStream =>
                {
                    int currentIndex = Interlocked.Increment(ref frameIndex) - 1;

                    var frame = FrameConverter.Convert(frameStream);
                    framesDict[currentIndex] = frame;

                });

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

                var lowQualityFormat = new WaveFormat(8000, 8, wavReader.WaveFormat.Channels);

                using var conversionStream = new WaveFormatConversionStream(lowQualityFormat, wavReader);

                using var outputWavStream = new MemoryStream();
                using (var wavWriter = new WaveFileWriter(outputWavStream, conversionStream.WaveFormat))
                {
                    conversionStream.CopyTo(wavWriter);
                }

                audio = outputWavStream;
            }

            var video = new CVideo(list.ToArray(), fps, audio.ToArray());

            GC.Collect();
            return video;
        }
    }
}
