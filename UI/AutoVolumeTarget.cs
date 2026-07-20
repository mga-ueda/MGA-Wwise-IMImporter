namespace MgaWwiseIMImporter.UI;

/// <summary>
/// Loudness Normalize の Auto Volume で Music Playlist のどのプロパティへ補償するか。
/// </summary>
internal enum AutoVolumeTarget
{
    /// <summary>Make-Up Gain（既定）。</summary>
    MakeUpGain,

    /// <summary>Voice Volume。</summary>
    VoiceVolume,
}
