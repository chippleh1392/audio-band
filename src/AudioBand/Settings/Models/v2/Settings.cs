﻿using System.Collections.Generic;

namespace AudioBand.Settings.Models.v2
{
    internal class Settings : ISettings
    {
        public string Version { get; set; } = "2";
        public string AudioSource { get; set; }
        public AudioBandSettings AudioBandSettings { get; set; }
        public PreviousButtonSettings PreviousButtonSettings { get; set; }
        public PlayPauseButtonSettings PlayPauseButtonSettings{ get; set; }
        public NextButtonSettings NextButtonSettings { get; set; }
        public ProgressBarSettings ProgressBarSettings { get; set; }
        public AlbumArtSettings AlbumArtSettings { get; set; }
        public AlbumArtPopupSettings AlbumArtPopupSettings { get; set; }
        public List<CustomLabelSettings> CustomLabelSettings { get; set; }
        public List<AudioSourceSettings> AudioSourceSettings { get; set; }
    }
}