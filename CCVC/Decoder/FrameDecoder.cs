﻿using System;
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
        private int _lastFrame = 0;
        public int FramesInBuffer { get; set; } = 120;

        private SemaphoreSlim _bufferSemaphore = new(1, 1);
        private Stopwatch _frameTimer = new();
        private double _frameInterval;

        public int LastDecodedFrame
        {
            get
            {
                _bufferSemaphore.Wait();
                try
                {
                    return _bufferedFrames.IsEmpty ? -1 : _bufferedFrames.Last().Key;
                }
                finally
                {
                    _bufferSemaphore.Release();
                }
            }
        }

        public void Seek(int targetFrame)
        {
            _bufferSemaphore.Wait();
            try
            {
                _bufferedFrames.Clear();
                _stream.Position = Math.Clamp(targetFrame, 0, _stream.Length - 1);

                _bufferSemaphore.Release();
                RecalculateBuffer();
            }
            finally
            {
                _bufferSemaphore.Release();
            }
        }

        private bool _recalculateCalled = false;
        private bool recalculateCalled
        {
            get
            {
                lock (this)
                {
                    return _recalculateCalled;
                }
            }
            set
            {
                lock (this)
                {
                    _recalculateCalled = value;
                }
            }
        }

        public void RecalculateBuffer()
        {
            if (_stream.Position >= _stream.Length || recalculateCalled) return;

            recalculateCalled = true;
            _bufferSemaphore.Wait();
            try
            {
                int neededFrames = FramesInBuffer - _bufferedFrames.Count;
                if (neededFrames <= 0) return;

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
                Parallel.For(0, preloaded.Count, i =>
                {
                    try
                    {
                        string content = FrameConverter.Convert(preloaded[i], _chars, _stream.ColorCount, _stream.Width, _stream.Height);
                        int index = _stream.Position - preloaded.Count + i;
                        tempBuffer.TryAdd(index, new Frame(content, index));
                    }
                    catch { }
                });

                foreach (var pair in tempBuffer)
                {
                    _bufferedFrames.TryAdd(pair.Key, pair.Value);
                }
            }
            finally
            {
                _bufferSemaphore.Release();
                recalculateCalled = false;
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

            _bufferSemaphore.Wait();
            Console.Title = $"Buffer size: {FramesInBuffer}; Frames buffered: {_bufferedFrames.Count}; Last frame: {_lastFrame}";
            try
            {
                if (_bufferedFrames.TryGetValue(_lastFrame, out Frame frameData))
                {
                    _bufferedFrames.TryRemove(_lastFrame, out _);
                    return frameData.Content;
                }

                _bufferSemaphore.Release();
                RecalculateBuffer();
                _bufferSemaphore.Wait();

                if (_bufferedFrames.TryGetValue(_lastFrame, out frameData))
                {
                    _bufferedFrames.TryRemove(_lastFrame, out _);
                    return frameData.Content;
                }
            }
            finally
            {
                if (_bufferSemaphore.CurrentCount == 0)
                {
                    _bufferSemaphore.Release();
                }
            }

            return new string('\n', _stream.Height);
        }

        public FrameDecoder(int width, int height, double fps, CVideoFrameReader stream, string chars = " .:-=+*#%@")
        {
            _stream = stream;
            _frameInterval = 1000.0 / fps;
            _chars = chars;
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