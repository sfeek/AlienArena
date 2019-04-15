/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 *
 * Author(s):
 *  - Shane Feek <shane.feek@gmail.com>
 */

using System.Collections.Generic;
using System.Linq;

namespace AsteroidArena
{
    class SpriteController
    {
        // Public variables
        public Dictionary<string,Sprite> sprites = new Dictionary<string, Sprite>(); // Keep a dictionary of the current sprites for quick access
        public List<string> renderList { get; private set; } // Keep presorted list of sprites for rendering in correct Z Order
                
        public SpriteController()
        {
            renderList = new List<string>(); 
        }

        public void AddSprite(Sprite sp, string id)
        {
            sp.SetID(id);
            sprites.Add(id, sp);

            renderList.Clear();
            foreach (KeyValuePair<string,Sprite> vp in sprites.OrderBy(key => key.Value.zPos))
            {
                renderList.Add(vp.Key);
            }
        }

        public Sprite GetSprite(string id)
        {
            return sprites[id];
        }

        public void RemoveSprite(string id)
        {
            sprites.Remove(id);

            renderList.Clear();
            foreach (KeyValuePair<string, Sprite> vp in sprites.OrderBy(key => key.Value.zPos))
            {
                renderList.Add(vp.Key);
            }
        }
    }
}
