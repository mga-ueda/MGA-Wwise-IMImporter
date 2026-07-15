using NAudio.Wave;

namespace MgaWwiseImporter.Wave;

/// <summary>
/// Wave ファイルの再生。位置は Position で取得する。
/// </summary>
internal sealed class WaveAudioPlayer : IDisposable
{
    private AudioFileReader? _reader;
    private WaveOutEvent? _output;
    private string? _path;
    private bool _isPlaying;
    private bool _disposed;

    public event EventHandler? PlaybackEnded;

    public bool IsPlaying => _isPlaying;

    public bool HasSource => !string.IsNullOrEmpty(_path);

    public TimeSpan Position => _reader?.CurrentTime ?? TimeSpan.Zero;

    public TimeSpan Duration => _reader?.TotalTime ?? TimeSpan.Zero;

    /// <summary>0〜1。長さ不明時は 0。</summary>
    public double Progress
    {
        get
        {
            var duration = Duration;
            if (duration <= TimeSpan.Zero)
            {
                return 0;
            }

            return Math.Clamp(Position.TotalSeconds / duration.TotalSeconds, 0d, 1d);
        }
    }

    public void Load(string path)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        StopAndRelease();
        _path = path;
        _reader = new AudioFileReader(path);
        _output = new WaveOutEvent();
        _output.Init(_reader);
        _output.PlaybackStopped += OnPlaybackStopped;
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        StopAndRelease();
        _path = null;
    }

    public void Play()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_output is null || _reader is null)
        {
            return;
        }

        if (_reader.Position >= _reader.Length)
        {
            _reader.Position = 0;
        }

        _output.Play();
        _isPlaying = true;
    }

    public void Pause()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_output is null || !_isPlaying)
        {
            return;
        }

        _output.Pause();
        _isPlaying = false;
    }

    public void Stop()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_output is null || _reader is null)
        {
            _isPlaying = false;
            return;
        }

        _output.Stop();
        _reader.Position = 0;
        _isPlaying = false;
    }

    /// <summary>再生中なら一時停止、停止中なら再生。</summary>
    public void Toggle()
    {
        if (_isPlaying)
        {
            Pause();
        }
        else
        {
            Play();
        }
    }

    /// <summary>位置を 0〜1 でシークする。</summary>
    public void Seek(double progress)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_reader is null)
        {
            return;
        }

        var duration = _reader.TotalTime;
        if (duration <= TimeSpan.Zero)
        {
            return;
        }

        var clamped = Math.Clamp(progress, 0d, 1d);
        // 終端ぴったりだと即 MediaEnded 扱いになることがあるためわずかに手前へ
        var ticks = (long)(duration.Ticks * clamped);
        if (clamped >= 1d && duration.Ticks > 0)
        {
            ticks = Math.Max(0, duration.Ticks - 1);
        }

        _reader.CurrentTime = TimeSpan.FromTicks(ticks);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        StopAndRelease();
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (_reader is null)
        {
            return;
        }

        // 末尾到達時のみ終了扱い（Stop 呼び出しでも発火するため位置で判定）
        if (_reader.Position >= _reader.Length || _reader.CurrentTime >= _reader.TotalTime)
        {
            _reader.Position = 0;
            _isPlaying = false;
            PlaybackEnded?.Invoke(this, EventArgs.Empty);
        }
    }

    private void StopAndRelease()
    {
        _isPlaying = false;

        if (_output is not null)
        {
            _output.PlaybackStopped -= OnPlaybackStopped;
            _output.Stop();
            _output.Dispose();
            _output = null;
        }

        if (_reader is not null)
        {
            _reader.Dispose();
            _reader = null;
        }
    }
}
