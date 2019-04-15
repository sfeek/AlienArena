/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 *
 * Author(s):
 *  - Shane Feek <shane.feek@gmail.com>
 */

using System.Drawing;

namespace AsteroidArena
{
    class BackGroundController
    {
        // Public variables
        public Bitmap canvas { get; set; }
        public Bitmap masterbackground { get; set; }
        public int canvaswidth { get; private set;}
        public int canvasheight { get; private set; }
        public int totalwidth { get; private set; }
        public int totalheight { get; private set; }
        public int windowwidth { get; private set; }
        public int windowheight { get; private set; }
        public int windowstartx { get; private set; }
        public int windowstarty { get; private set; }


        public BackGroundController(string fname, int xSize, int ySize)
        {
            SetBackground(fname, xSize, ySize);
        }

        public void SetBackground(string fname, int xSize, int ySize)
        {
            Bitmap image = new Bitmap(fname);
            masterbackground = image.Clone(new Rectangle(0, 0, xSize, ySize), System.Drawing.Imaging.PixelFormat.Format32bppPArgb); // Precompute ARGB Multiplication

            canvas = new Bitmap(xSize, ySize);

            totalwidth = image.Width;
            totalheight = image.Height;

            canvaswidth = xSize;
            canvasheight = ySize;

            image.Dispose();
        }

        public void SetWindow(int x, int y, int width, int height)
        {
            windowstartx = x;
            windowstarty = y;
            windowwidth = width;
            windowheight = height;

            Sprite.SetCollisionMaskOffset(x,y); // Make sure collision mask lines up with background window
        }
    }
}
