using ComputeSharp;
using System.Text;

namespace CCVC.Decoder;

public class FrameConverter
{
    public static string Convert(byte[] input, string chars, byte colors, int width, int height)
    {
        int[,] EncodedFrame = new int[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                EncodedFrame[y, x] = input[index];
            }
        }


        using var texture = GraphicsDevice.GetDefault().AllocateReadOnlyTexture2D<int>(EncodedFrame);
        using var buffer = GraphicsDevice.GetDefault().AllocateReadWriteTexture2D<int>(width, height);
        using var properties = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<int>([chars.Length, colors]);
        //lock(locker)
        //{
        GraphicsDevice.GetDefault().For<DecodeShader>(texture.Width, texture.Height, new DecodeShader(texture, buffer, properties));
        //}
        
        var result = buffer.ToArray();


        StringBuilder DecodedFrame = new();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var index = result[y, x];
                DecodedFrame.Append(chars[index]);
            }
            DecodedFrame.AppendLine();
        }

        return DecodedFrame.ToString();
    }
}


[ThreadGroupSize(DefaultThreadGroupSizes.XY)]
[GeneratedComputeShaderDescriptor]
public readonly partial struct DecodeShader(ReadOnlyTexture2D<int> input, ReadWriteTexture2D<int> buffer, ReadOnlyBuffer<int> properties) : IComputeShader
{
    public void Execute()
    {
        float k = (float)properties[1] / properties[0];
        int value = (int)(input[ThreadIds.XY] / k + 0.5f);
        buffer[ThreadIds.XY] = Hlsl.Clamp(value, 0, properties[0] - 1);
    }
}
