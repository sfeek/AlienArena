/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 *
 * Author(s):
 *  - Shane Feek <shane.feek@gmail.com>
 */

using System.Collections.Generic;

namespace AsteroidArena
{
    class SoundController
    {
        // Public variables
        public Dictionary<string, Sound> sounds = new Dictionary<string, Sound>(); // Keep a dictionary of the current sounds for quick access

        public SoundController()
        {
        }

        public void AddSound(Sound snd, string id)
        {
            snd.SetID(id);
            sounds.Add(id, snd);
        }

        public Sound GetSound(string id)
        {
            return sounds[id];
        }

        public void RemoveSound(string id)
        {
            sounds[id].CloseSound();
            sounds.Remove(id);
        }
    }
}
