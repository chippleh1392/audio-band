﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using AudioBand.AudioSource;
using iTunesLib;
using Timer = System.Timers.Timer;

namespace iTunesAudioSource
{
    public class AudioSource : IAudioSource
    {
        private Timer _checkiTunesTimer;
        private string _currentTrack;
        private bool _isPlaying;
        private int _volume;
        private bool _shuffle;
        private ITPlaylistRepeatMode _repeat;
        private ITunesControls _itunesControls = new ITunesControls();

        public AudioSource()
        {
            _checkiTunesTimer = new Timer(100)
            {
                Enabled = false,
                AutoReset = false
            };

            _checkiTunesTimer.Elapsed += CheckItunes;
        }

        public event EventHandler<TrackInfoChangedEventArgs> TrackInfoChanged;

        public event EventHandler<bool> IsPlayingChanged;

        public event EventHandler<TimeSpan> TrackProgressChanged;

#pragma warning disable 00067 // Event is not used
        public event EventHandler<SettingChangedEventArgs> SettingChanged;
#pragma warning restore 00067 // Event is not used

        public event EventHandler<float> VolumeChanged;

        public event EventHandler<bool> ShuffleChanged;

        public event EventHandler<RepeatMode> RepeatModeChanged;

        public string Name => "iTunes";

        public IAudioSourceLogger Logger { get; set; }

        public Task ActivateAsync()
        {
            _itunesControls.Start();
            _checkiTunesTimer.Start();
            return Task.CompletedTask;
        }

        public Task DeactivateAsync()
        {
            _checkiTunesTimer.Stop();
            _itunesControls.Stop();

            _isPlaying = false;
            _currentTrack = null;

            return Task.CompletedTask;
        }

        public Task NextTrackAsync()
        {
            _itunesControls.Next();
            return Task.CompletedTask;
        }

        public Task PauseTrackAsync()
        {
            _itunesControls.Pause();
            return Task.CompletedTask;
        }

        public Task PlayTrackAsync()
        {
            _itunesControls.Play();
            return Task.CompletedTask;
        }

        public Task PreviousTrackAsync()
        {
            _itunesControls.Previous();
            return Task.CompletedTask;
        }

        public Task SetVolumeAsync(float newVolume)
        {
            _itunesControls.Volume = (int)(newVolume * 100);
            return Task.CompletedTask;
        }

        public Task SetPlaybackProgressAsync(TimeSpan newProgress)
        {
            _itunesControls.Progress = newProgress;
            return Task.CompletedTask;
        }

        public Task SetShuffleAsync(bool shuffleOn)
        {
            _itunesControls.Shuffle = shuffleOn;
            return Task.CompletedTask;
        }

        public Task SetRepeatModeAsync(RepeatMode newRepeatMode)
        {
            _itunesControls.RepeatMode = ToITRepeat(newRepeatMode);
            return Task.CompletedTask;
        }

        private RepeatMode ToRepeatMode(ITPlaylistRepeatMode mode)
        {
            switch (mode)
            {
                case ITPlaylistRepeatMode.ITPlaylistRepeatModeAll:
                    return RepeatMode.RepeatContext;
                case ITPlaylistRepeatMode.ITPlaylistRepeatModeOff:
                    return RepeatMode.Off;
                case ITPlaylistRepeatMode.ITPlaylistRepeatModeOne:
                    return RepeatMode.RepeatTrack;
                default:
                    Logger.Warn($"No case for {mode}");
                    return RepeatMode.Off;
            }
        }

        private ITPlaylistRepeatMode ToITRepeat(RepeatMode mode)
        {
            switch (mode)
            {
                case RepeatMode.Off:
                    return ITPlaylistRepeatMode.ITPlaylistRepeatModeOff;
                case RepeatMode.RepeatContext:
                    return ITPlaylistRepeatMode.ITPlaylistRepeatModeAll;
                case RepeatMode.RepeatTrack:
                    return ITPlaylistRepeatMode.ITPlaylistRepeatModeOne;
                default:
                    Logger.Warn($"No case for {mode}");
                    return ITPlaylistRepeatMode.ITPlaylistRepeatModeOff;
            }
        }

        private void NotifyTrackChange(Track track)
        {
            var trackInfo = new TrackInfoChangedEventArgs
            {
                Artist = track.Artist,
                Album = track.Album,
                AlbumArt = track.Artwork,
                TrackLength = track.Length,
                TrackName = track.Name,
            };

            TrackInfoChanged?.Invoke(this, trackInfo);
        }

        private bool IsNewTrack(Track track)
        {
            var trackname = track.Artist + track.Name;
            if (trackname == _currentTrack)
            {
                return false;
            }

            _currentTrack = trackname;
            return true;
        }

        private void NotifyPlayerState()
        {
            var playing = _itunesControls.IsPlaying;
            if (_isPlaying == playing)
            {
                return;
            }

            _isPlaying = playing;
            IsPlayingChanged?.Invoke(this, _isPlaying);
        }

        private void CheckItunes(object sender, ElapsedEventArgs eventArgs)
        {
            try
            {
                var track = _itunesControls.CurrentTrack;
                if (track == null)
                {
                    return;
                }

                NotifyPlayerState();
                NotifyVolume();
                NotifyShuffle();
                if (IsNewTrack(track))
                {
                    NotifyTrackChange(track);
                }

                TrackProgressChanged?.Invoke(this, _itunesControls.Progress);
            }
            catch (Exception e)
            {
                Logger.Debug(e);
            }
            finally
            {
                _checkiTunesTimer.Enabled = true;
            }
        }

        private void NotifyVolume()
        {
            int volume = _itunesControls.Volume;

            if (volume == _volume)
            {
                return;
            }

            _volume = volume;
            VolumeChanged?.Invoke(this, _volume / 100f);
        }

        private void NotifyShuffle()
        {
            bool shuffle = _itunesControls.Shuffle;

            if (shuffle == _shuffle)
            {
                return;
            }

            _shuffle = shuffle;
            ShuffleChanged?.Invoke(this, _shuffle);
        }

        private void NotifyRepeatMode()
        {
            ITPlaylistRepeatMode repeat = _itunesControls.RepeatMode;

            if (repeat == _repeat)
            {
                return;
            }

            _repeat = repeat;
            RepeatModeChanged?.Invoke(this, ToRepeatMode(_repeat));
        }
    }
}
