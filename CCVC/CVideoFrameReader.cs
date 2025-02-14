namespace CCVC;

public class CVideoFrameReader
{
    private CVideo _video;
    private int _lastFrame;
    public double FPS { get { lock (_video) { return _video.FPS; } } }
    public byte ColorCount { get { return _video.ColorCount; } }

    private int _cachedLength = -1;
    public int Length
    {   get 
        { 
            lock (_video) 
            { 
                if(_cachedLength != -1)
                    return _cachedLength;
                
                _cachedLength = _video.GetLength();
                return _cachedLength;
            } 
        } 
    }

    public int Position { get { return _lastFrame; } set { _lastFrame = value; } }
    public byte[] Read()
    {
        if (_lastFrame >= Length || _lastFrame < 0)
            return Array.Empty<byte>();

        try
        {
            var frame = _video.GetFrame(_lastFrame);
            _lastFrame++;
            return frame;
        }
        catch
        {
            return Array.Empty<byte>();
        }
    }

    public CVideoFrameReader(CVideo video)
    {
        this._video = video;
    }
}
