using ComputeSharp;
using SkiaSharp;
using System.Text;

namespace CCVC.Encoder;

public class FrameConverter
{
    const string asciiChars = " .:-=+*#%@";
    public static int Width { get; } = 256;
    public static int Height { get; } = 128;

    private static object locker = new();
    public static string Convert(Stream image)
    {
        var bitmap = SKBitmap.Decode(image).Resize(new SKSizeI(Width, Height), new SKSamplingOptions());
        image = new MemoryStream();
        bitmap.Encode(image, SKEncodedImageFormat.Png, 100);
        bitmap.Dispose();

        using var texture = GraphicsDevice.GetDefault().LoadReadWriteTexture2D<Bgra32, float4>(image);
        using var buffer = GraphicsDevice.GetDefault().AllocateReadWriteTexture2D<int>(Width, Height);
        lock(locker)
        {
            GraphicsDevice.GetDefault().For<EncodeShader>(texture.Width, texture.Height, new EncodeShader(texture, buffer));
        }
        
        var result = buffer.ToArray();
        

        var sb = new StringBuilder();

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var index = result[y, x];
                sb.Append(asciiChars[index]);
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}


[ThreadGroupSize(DefaultThreadGroupSizes.XY)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct EncodeShader(IReadWriteNormalizedTexture2D<float4> texture, ReadWriteTexture2D<int> buffer) : IComputeShader
{
    public void Execute()
    {
        float3 rgb = texture[ThreadIds.XY].RGB;
        float grayValue = rgb[0] * 0.3f + rgb[1] * 0.59f + rgb[2] * 0.11f;

        int index = (int)(grayValue * 10f);

        index = Hlsl.Clamp(index, 0, 9);

        buffer[ThreadIds.XY] = index;
    }
}
