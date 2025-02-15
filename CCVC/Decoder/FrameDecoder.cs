using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace CCVC.Decoder
{
    public class FrameDecoder
    {
        private string _chars;

        private CVideoFrameReader _stream;

        private ConcurrentDictionary<int, Frame> _bufferedFrames = new();
        private int _framesInBuffer
        {
            get
            {
                lock (this)
                {
                    return _fib;
                }
            }
            set
            {
                lock (this)
                {
                    _fib = Math.Max(value, 0);
                }
            }
        }
        private int _fib = 0;
        private int _lastFrame = 0;
        public int BufferSize { get; set; } = 240;

        private ReaderWriterLockSlim _bufferLock = new ReaderWriterLockSlim();
        private Stopwatch _frameTimer = new();
        private double _frameInterval;

        public int LastDecodedFrame
        {
            get
            {
                _bufferLock.EnterReadLock();
                try
                {
                    return _bufferedFrames.IsEmpty ? -1 : _bufferedFrames.Last().Key;
                }
                finally
                {
                    _bufferLock.ExitReadLock();
                }
            }
        }

        public void Seek(int targetFrame)
        {
            _bufferLock.EnterWriteLock();
            try
            {
                _stream.Position = Math.Clamp(targetFrame, 0, _stream.Length - 1) + (int)_stream.FPS;

                var outdated = _bufferedFrames.Keys
                    .Where(k => k < targetFrame - 10 || k > targetFrame + 10)
                    .ToList();

                foreach (var key in outdated)
                {
                    if (_bufferedFrames.TryRemove(key, out _))
                    {
                        _framesInBuffer--;
                    }
                }
            }
            finally
            {
                _bufferLock.ExitWriteLock();
            }
        }

        public void RecalculateBuffer()
        {
            while (_stream.Position < _stream.Length)
            {
                Debug.WriteLine($"RecalculateBuffer executed on {DateTimeOffset.UtcNow}");
                try
                {
                    int neededFrames = BufferSize - _framesInBuffer;
                    if (neededFrames <= 0) continue;

                    List<byte[]> preloaded = new(neededFrames);
                    lock (_stream)
                    {
                        for (int i = 0; i < neededFrames && _stream.Position < _stream.Length; i++)
                        {
                            byte[] frameData = _stream.Read();
                            if (frameData.Length == 0) break;
                            preloaded.Add(frameData);
                        }
                    }


                    var tempBuffer = new ConcurrentDictionary<int, Frame>();
                    var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
                    Parallel.For(0, preloaded.Count, options, i =>
                    {
                        try
                        {
                            string content = FrameConverter.Convert(preloaded[i], _chars, _stream.ColorCount, _stream.Width, _stream.Height);
                            int index = _stream.Position - preloaded.Count + i;
                            tempBuffer.TryAdd(index, new Frame(content, index));
                        }
                        catch { }
                    });

                    _bufferLock.EnterWriteLock();
                    try
                    {
                        foreach (var pair in tempBuffer)
                        {
                            if (_bufferedFrames.TryAdd(pair.Key, pair.Value))
                            {
                                _framesInBuffer++;
                            }
                        }
                    }
                    finally
                    {
                        _bufferLock.ExitWriteLock();
                    }
                }
                finally
                {
                    //_bufferSemaphore.Release();
                    Debug.WriteLine($"RecalculateBuffer completed on {DateTimeOffset.UtcNow}");
                }
            }
            
        }


        public string ReadFrame(int frame)
        {
            _lastFrame = frame;

            if (!_frameTimer.IsRunning)
            {
                _frameTimer.Start();
            }
            else
            {
                double elapsed = _frameTimer.Elapsed.TotalMilliseconds;
                double delay = _frameInterval - elapsed;

                if (delay > 0)
                {
                    Thread.Sleep((int)Math.Max(delay, 1));
                }
                _frameTimer.Restart();
            }

            _bufferLock.EnterReadLock();

            Console.Title = $"Buffer: {BufferSize};Buffered: {_framesInBuffer};Last frame: {_lastFrame};Stream pos: {_stream.Position}";
            try
            {
                if (_bufferedFrames.TryGetValue(_lastFrame, out Frame frameData))
                {
                    _bufferedFrames.TryRemove(_lastFrame, out _);
                    _framesInBuffer--;
                    return frameData.Content;
                }
            }
            finally
            {
                _bufferLock.ExitReadLock();
            }

            return new string('\n', _stream.Height);
        }

        public FrameDecoder(int width, int height, double fps, CVideoFrameReader stream, string chars = " .:-=+*#%@")
        {
            _stream = stream;
            _frameInterval = 1000.0 / fps;
            _chars = chars;

            ThreadPool.SetMinThreads(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2);
        }
    }

    internal struct Frame
    {
        public string Content { get; }
        public int Index { get; }

        public Frame(string content, int index)
        {
            Content = content;
            Index = index;
        }
    }
}