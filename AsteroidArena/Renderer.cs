/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 *
 * Author(s):
 *  - Shane Feek <shane.feek@gmail.com>
 */

using System;
using System.Drawing;
using System.Windows.Forms;

namespace AsteroidArena
{
    class Renderer
    {
        public static void Render(PaintEventArgs e, BackGroundController bg, SpriteController spc, Sprite sp, SoundController sndc, Sound sd, string scoreboard, Font scoreboardFont, System.Drawing.Brush scoreboardBrushes, RectangleF scoreboardLocation)
        {
            using (Graphics graphics = Graphics.FromImage(bg.canvas)) // Grab the blank canvas
            {
                e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy; // No blending to write background image super fast
                e.Graphics.DrawImage(bg.masterbackground, 0, 0, new Rectangle(bg.windowstartx, bg.windowstarty, bg.windowwidth, bg.windowheight), GraphicsUnit.Pixel); // Grab the window of the main background image

                e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver; // Blending to write scoreboard and sprites on top

                if (Sprite.spritesEnabled) // Check if sprites enabled
                {
                    foreach (String st in spc.renderList) // Cycle through sprites by name in Z Order
                    {
                        sp = spc.sprites[st]; // Get Sprite by name

                        if (sp.visible) // Check if sprite is visible, if not skip it
                        {
                            if (sp.rotationAngle == 0) // Check if rotated
                            {
                                e.Graphics.DrawImage(sp.image, new Rectangle(sp.xRPos, sp.yRPos, sp.width, sp.height), sp.currentFrame * sp.width, 0, sp.width, sp.height, GraphicsUnit.Pixel); // Not rotated, draw fast
                            }
                            else
                            {
                                Bitmap bitmap = sp.image.Clone(new Rectangle(sp.currentFrame * sp.width, 0, sp.width, sp.height), sp.image.PixelFormat);

                                using (Graphics g = Graphics.FromImage(bitmap))
                                {
                                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy; // No blending to write image super fast
                                    g.TranslateTransform(sp.width / 2, sp.height / 2);
                                    g.RotateTransform(sp.rotationAngle);
                                    g.TranslateTransform(-sp.width / 2, -sp.height / 2);
                                    g.DrawImageUnscaled(bitmap, new Point(0, 0)); // No scaling to write image super fast
                                }

                                e.Graphics.DrawImage(bitmap, new Point(sp.xRPos, sp.yRPos));
                            }

                            if (sp.checksForCollisions) sp.CheckIfCollidingSprite(spc.sprites); // Check for sprite to sprite collisions
                            sp.NextFrame(); // Animate and move the sprite
                        }
                    }
                }

                e.Graphics.DrawString(scoreboard, scoreboardFont, scoreboardBrushes, scoreboardLocation); // Draw Scoreboard Text

                if (Sound.soundsEnabled) // Play sounds if necessary
                {
                    foreach (Sound snd in sndc.sounds.Values)
                    {
                        if (snd.soundEnabled) snd.Play();
                    }
                }
            }
        }
    }
}
