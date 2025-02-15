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

            using GZipStream gzip = new(new MemoryStream(Resources.FfmpegArchive), CompressionMode.Decompress);
            using FileStream file = File.Create(_ffmpegPath);
            gzip.CopyTo(file);
            file.Close();
        }
    }
}
