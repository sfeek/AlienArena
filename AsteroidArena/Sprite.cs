/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 *
 * Author(s):
 *  - Shane Feek <shane.feek@gmail.com>
 */

using System;
using System.Collections.Generic;
using System.Drawing;

namespace AsteroidArena
{
    class Sprite
    {
        // Public variables
        public Bitmap image { get; private set; }
        public static Bitmap collisionMask { get; private set; }
        public int xPos { get; private set; }
        public int yPos { get; private set; }
        public int xRPos { get; private set; }
        public int yRPos { get; private set; }
        public int zPos { get; private set; }
        public int height { get; private set; }
        public int width { get; private set; }
        public int frames { get; private set; }
        public int currentFrame { get; private set; }
        public int animationSpeed { get; private set; }
        public int movementSpeed { get; private set; }
        public int movementDirection { get; private set; }
        public int startFrame { get; private set; }
        public int stopFrame { get; private set; }
        public int movementDistance { get; private set; }
        public bool visible { get; private set; }
        public string id { get; private set; }
        public int collisionRadius { get; private set; }
        public int rotationAngle { get; private set; }
        public bool animateOnce { get; private set; }
        public bool movementEnabled { get; private set; }
        public bool checksForCollisions { get; private set; }
        public bool canCauseSpriteCollisions { get; private set; }
        public bool checksForBorders { get; private set; }
        public bool enforceBorders { get; private set; }
        public int areaClearRadius { get; private set; }
        public int x1Border { get; private set; }
        public int x2Border { get; private set; }
        public int y1Border { get; private set; }
        public int y2Border { get; private set; }
        public bool checksForMaskCollisions { get; private set; }
        public bool enforceMaskCollisions { get; private set; }
        public int currentMaskColor { get; private set; }
        public static int collisionMaskOffsetX { get; private set; }
        public static int collisionMaskOffsetY { get; private set; }
        public int animationDirection { get; private set; }
        public static bool spritesEnabled { get; private set; }

        // Events
        public delegate void AnimationCompleteEvent(string id);
        public event AnimationCompleteEvent OnAnimationComplete;

        public delegate void SpriteCollisionEvent(string id, List<string> collision);
        public event SpriteCollisionEvent OnSpriteCollision;

        public delegate void SpriteOutsideBorderEvent(string id, int border);
        public event SpriteOutsideBorderEvent OnSpriteOutsideBorder;

        public delegate void SpriteMaskCollisionEvent(string id, int color);
        public event SpriteMaskCollisionEvent OnSpriteMaskCollision;

        // Internal variables
        private int movementcount;
        private int animationcount;
        private double radianAngle;
        private int maskBackgroundColor = Color.Black.ToArgb();
        List<string> collisionList = new List<string>();

        public Sprite(string fname, int numFrames)
        {
            Bitmap srcimage = new Bitmap(fname);
            image = srcimage.Clone(new Rectangle(0, 0, srcimage.Width, srcimage.Height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb); // Precompute ARGB Multiplication
            srcimage.Dispose();

            frames = numFrames;
            height = image.Height;
            width = image.Width / frames;
            animationcount = 0;
            movementcount = 0;
            movementDirection = 0;
            zPos = 0;
            visible = false;
            collisionRadius = Convert.ToInt32(((height / 2) + (width / 2)) / 2 * 0.80); // 80% default collision radius
            startFrame = 0;
            currentFrame = startFrame;
            stopFrame = numFrames - 1;
            movementDistance = 10;
            animateOnce = false;
            movementEnabled = false;
            checksForCollisions = true;
            checksForBorders = true;
            canCauseSpriteCollisions = true;
            checksForMaskCollisions = false;
            enforceMaskCollisions = true;
            animationDirection = 1;
            enforceBorders = true;
            currentMaskColor = Color.Black.ToArgb();
            spritesEnabled = true;
        }

        public void NextFrame()
        {
            if (animationDirection != 0)
            {
                animationcount++;
                if (animationcount >= animationSpeed)
                {
                    if (animationDirection > 0)
                    {
                        if (currentFrame >= stopFrame)
                            if (animateOnce)
                            {
                                currentFrame = stopFrame;
                                OnAnimationComplete?.Invoke(id);
                            }
                            else
                                currentFrame = startFrame;
                        else
                        {
                            currentFrame++;
                        }
                    }

                    if (animationDirection < 0)
                    {
                        if (currentFrame <= startFrame)
                            if (animateOnce)
                            {
                                currentFrame = startFrame;
                                OnAnimationComplete?.Invoke(id);
                            }
                            else
                                currentFrame = stopFrame;
                        else
                        {
                            currentFrame--;
                        }
                    }

                    animationcount = 0;
                }
            }

            if (movementEnabled)
            {
                movementcount++;
                if (movementcount >= movementSpeed)
                {
                    SetPosition(Convert.ToInt32(xPos + movementDistance * Math.Cos(radianAngle)), Convert.ToInt32(yPos + movementDistance * Math.Sin(radianAngle)));
                    movementcount = 0;
                }
            }
        }

        public static void SetSpritesEnabled(bool se)
        {
            spritesEnabled = se;
        }

        public void SetAnimationStartStopFrames(int start, int stop)
        {
            if (start < 0 || start > stop) start = 0;
            if (stop < 0 || stop < start || stop > frames - 1) stop = frames - 1;

            startFrame = start;
            stopFrame = stop;
            currentFrame = startFrame;
        }

        public void SetCurrentAnimationFrame(int framenumber)
        {
            if (framenumber < startFrame) framenumber = startFrame;
            if (framenumber > stopFrame) framenumber = stopFrame;

            currentFrame = framenumber;
        }

        public void SetAnimationDirection(int d) // -1 = Backwards, 0 = Disable Animation, 1 = Forwards
        {
            animationDirection = d;
        }

        public void SetAnimateOnce(bool ao)
        {
            animateOnce = ao;
        }

        public static double ConvertDegreesToRadians(double Degrees)
        {
            return (Math.PI / 180.0) * Degrees;
        }

        public void CheckIfCollidingSprite(Dictionary<string, Sprite> splist)
        {
            collisionList.Clear();

            foreach (Sprite sp in splist.Values)
            {
                if (sp == this) continue; // Skip ourselves
                if (sp.visible == false) continue; // Skip disabled sprites
                if (sp.canCauseSpriteCollisions == false) continue; // Skip sprites that don't cause collisions

                double d = Math.Sqrt((sp.xPos - xPos) * (sp.xPos - xPos) + (sp.yPos - yPos) * (sp.yPos - yPos));

                if (d < (sp.collisionRadius + collisionRadius)) collisionList.Add(sp.id); // Add collisions to the list
            }

            if (collisionList.Count > 0) OnSpriteCollision?.Invoke(id, collisionList);
        }

        public List<string> CheckIfAreaClearSprite(Dictionary<string, Sprite> splist)
        {
            collisionList.Clear();

            foreach (Sprite sp in splist.Values)
            {
                if (sp == this) continue; // Skip ourselves
                if (sp.visible == false) continue; // Skip disabled sprites

                double d = Math.Sqrt((sp.xPos - xPos) * (sp.xPos - xPos) + (sp.yPos - yPos) * (sp.yPos - yPos));

                if (d < (sp.collisionRadius + areaClearRadius)) collisionList.Add(sp.id); // Add collisions to the list
            }

            return collisionList;
        }

        public bool CheckIfMouseOver(int mx, int my)
        {
            if (visible)
            {
                double d = Math.Sqrt((mx - xPos) * (mx - xPos) + (my - yPos) * (my - yPos));

                if (d < collisionRadius) return true;
            }

            return false;
        }

        public void SetBorder(int x1, int y1, int x2, int y2)
        {
            x1Border = x1;
            x2Border = x2;
            y1Border = y1;
            y2Border = y2;
        }

        public void SetPosition(int x, int y)
        {
            if (checksForBorders)
            {
                if (x >= x2Border - collisionRadius) { OnSpriteOutsideBorder?.Invoke(id, 1); if (enforceBorders) return; }
                if (y >= y2Border - collisionRadius) { OnSpriteOutsideBorder?.Invoke(id, 2); if (enforceBorders) return; }
                if (x <= x1Border + collisionRadius) { OnSpriteOutsideBorder?.Invoke(id, 3); if (enforceBorders) return; }
                if (y <= y1Border + collisionRadius) { OnSpriteOutsideBorder?.Invoke(id, 4); if (enforceBorders) return; }
            }

            if (checksForMaskCollisions)
            {
                currentMaskColor = collisionMask.GetPixel(x + collisionMaskOffsetX, y + collisionMaskOffsetY).ToArgb();

                if (currentMaskColor != maskBackgroundColor)
                {
                    OnSpriteMaskCollision?.Invoke(id, currentMaskColor);
                    if (enforceMaskCollisions) return; // Do not allow sprite to move if Collision Enforcement is true
                }
            }

            // Render position (Top corner)
            xRPos = x - (width / 2);
            yRPos = y - (height / 2);

            // Center Position
            xPos = x;
            yPos = y;
        }

        public bool CheckIfInsideBox(int x1, int y1, int x2, int y2)
        {
            if (xPos <= x1 + collisionRadius) return false;
            if (yPos <= y1 + collisionRadius) return false;
            if (xPos >= x2 - collisionRadius) return false;
            if (yPos >= y2 - collisionRadius) return false;

            return true;
        }

        public bool CheckIfOutsideBox(int x1, int y1, int x2, int y2)
        {
            if (xPos <= x1 + collisionRadius) return true;
            if (yPos <= y1 + collisionRadius) return true;
            if (xPos >= x2 - collisionRadius) return true;
            if (yPos >= y2 - collisionRadius) return true;

            return false;
        }

        public static int CalculateBorderBounceAngle(int border_num, int angle)
        {
            angle = NormalizeAngle(angle);

            if (border_num == 1 || border_num == 3)
                angle = 180 - angle; // Horizontal
            else
                angle = 360 - angle; // Verticle

            if (angle < 0) angle += 360;

            return angle;
        }

        public static int NormalizeAngle(int angle)
        {
            angle = angle % 360;
            if (angle < 0) angle += 360;

            return angle;
        }

        public void SetMovementDirection(int a)
        {
            a = NormalizeAngle(a);
            radianAngle = ConvertDegreesToRadians(a);
            movementDirection = a;
        }

        public void SetAnimationSpeed(int spd)
        {
            if (spd < 1) spd = 1;
            if (spd > 1000) spd = 1000;
            animationSpeed = spd; // Lower number, greater speed. Fraction of full speed
        }

        public void SetMovementSpeed(int spd)
        {
            if (spd < 1) spd = 1;
            if (spd > 1000) spd = 1000;
            movementSpeed = spd; // Lower number, greater speed. Fraction of full speed
        }

        public void SetMovementDistance(int d)
        {
            if (d < 2) d = 2;
            movementDistance = d;
        }

        public void SetRotationAngle(int a)
        {
            a = NormalizeAngle(a);
            rotationAngle = a;
        }

        public void SetCollisionRadius(int cr)
        {
            collisionRadius = cr;
        }

        public void SetAreaClearRadius(int cr)
        {
            areaClearRadius = cr;
        }

        public void SetZPos(int z)
        {
            zPos = z; // Lowest number rendered on the bottom, higher numbers on top
        }

        public void SetVisible(bool v)
        {
            visible = v;
        }

        public void SetMovementEnabled(bool me)
        {
            movementEnabled = me;
        }

        public void SetChecksforCollisions(bool cc)
        {
            checksForCollisions = cc;
        }

        public void SetCanCauseCollisions(bool cc)
        {
            canCauseSpriteCollisions = cc;
        }

        public void SetChecksforBorders(bool cb)
        {
            checksForBorders = cb;
        }

        public void SetEnforceBorders(bool eb)
        {
            enforceBorders = eb;
        }

        public void SetID(string ID)
        {
            id = ID;
        }

        public void SetEnforceMaskCollisions(bool emc)
        {
            enforceMaskCollisions = emc;
        }

        public void SetChecksforMaskCollisions(bool mc)
        {
            checksForMaskCollisions = mc;
        }

        public static void SetCollisionMaskOffset(int x, int y)
        {
            collisionMaskOffsetX = x;
            collisionMaskOffsetY = y;
        }

        public static void SetCollisionMask(string fname)
        {
            Bitmap bmp = new Bitmap(fname);
            collisionMask = bmp.Clone(new Rectangle(0, 0, bmp.Width,bmp.Height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb); // Precompute ARGB Multiplication
            bmp.Dispose();
        }
    }
}
