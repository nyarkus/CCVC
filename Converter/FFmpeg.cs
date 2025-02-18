using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using CCVC.Encoder;

namespace Converter
{
    internal class FFmpeg : FFmpegManager
    {
        public override void CheckFFmpeg()
        {
            if (File.Exists(_ffmpegPath))
                return;

            Console.WriteLine("FFMpeg not found.");
            Environment.Exit(-1);
        }
    }
}
