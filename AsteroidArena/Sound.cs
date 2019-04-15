/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 *
 * Author(s):
 *  - Shane Feek <shane.feek@gmail.com>
 */

using System.Windows.Media;
using System;
using System.IO;
using System.Windows.Forms;

namespace AsteroidArena
{
    class Sound
    {
        // Public variables
        string appPath = Path.GetDirectoryName(Application.ExecutablePath);
        public string id { get; private set; }
        public bool play { get; private set; }
        public bool soundPlaying { get; private set; }
        public bool repeat { get; private set; }
        public static bool soundsEnabled { get; private set; }
        public bool soundEnabled { get; private set; }
        public int volume { get; private set; }

        MediaPlayer snd;

        public Sound(string fname)
        {
            snd = new MediaPlayer();
            
            snd.Open(new Uri(appPath + @"\" + fname));

            snd.MediaEnded += new EventHandler(SoundDone);

            play = false;

            repeat = false;

            soundsEnabled = true;
            soundEnabled = true;
            soundPlaying = false;

            volume = 10; // Set for loudest by default
            SetVolume(volume);
        }

        public void SetID(string ID)
        {
            id = ID;
        }

        public void CloseSound()
        {
            snd.Stop();
            snd.Close();
        }

        public void Play()
        {
            if (play)
            {
                snd.Position = TimeSpan.Zero;
                snd.Play();
                soundPlaying = true;
                play = false;
            }
        }

        public void PlayNow()
        {
            play = true;
        }

        public void SetRepeat(bool r)
        {
            repeat = r;
        }

        public void SetSoundsEnabled(bool se)
        {
            soundsEnabled = se;
        }

        public void SetVolume(int vol)
        {
            if (vol < 0) vol = 0;
            if (vol > 10) vol = 10;
            volume = vol;

            snd.Volume = (float)(vol / 10.0);
        }

        public void SetSoundEnabled(bool se)
        {
            soundEnabled = se;
        }

        public void SoundDone(object sender, EventArgs e)
        {
            soundPlaying = false;
            if (repeat) play = true;
        }
    }
}
