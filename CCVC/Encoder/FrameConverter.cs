using ComputeSharp;
using SkiaSharp;
using System.Text;

namespace CCVC.Encoder;

public class FrameConverter
{
    private static object locker = new();
    public static byte[] Convert(Stream image, byte countOfColors, int width, int height)
    {
        var bitmap = SKBitmap.Decode(image).Resize(new SKSizeI(width, height), new SKSamplingOptions());
        image = new MemoryStream();
        bitmap.Encode(image, SKEncodedImageFormat.Png, 100);
        bitmap.Dispose();

        using var texture = GraphicsDevice.GetDefault().LoadReadWriteTexture2D<Bgra32, float4>(image);
        using var buffer = GraphicsDevice.GetDefault().AllocateReadWriteTexture2D<int>(width, height);
        using var properties = GraphicsDevice.GetDefault().AllocateReadWriteBuffer<int>([countOfColors]);
        lock(locker)
        {
            GraphicsDevice.GetDefault().For<EncodeShader>(texture.Width, texture.Height, new EncodeShader(texture, buffer, properties));
        }
        
        var result = buffer.ToArray();


        List<byte> frame = new();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var value = result[y, x];
                frame.Add((byte)value);
            }
        }

        return frame.ToArray();
    }
}


[ThreadGroupSize(DefaultThreadGroupSizes.XY)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct EncodeShader(IReadWriteNormalizedTexture2D<float4> texture, ReadWriteTexture2D<int> buffer, ReadWriteBuffer<int> properties) : IComputeShader
{
    public void Execute()
    {
        float3 rgb = texture[ThreadIds.XY].RGB;
        float grayValue = rgb[0] * 0.3f + rgb[1] * 0.59f + rgb[2] * 0.11f;

        int index = (int)(grayValue * properties[0]);

        index = Hlsl.Clamp(index, 0, properties[0]);

        buffer[ThreadIds.XY] = index;
    }
}
