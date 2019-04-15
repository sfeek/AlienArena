using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace AsteroidArena
{
    public partial class asteroidarena : Form
    {
        // **********************************************************************************************************************************************************************************
        // Render Engine Global Variables - DO NOT REMOVE
        // **********************************************************************************************************************************************************************************
        BackGroundController bg; // Background Image
        Sprite sp;               // Sprite object
        SpriteController spc;    // Sprite Controller
        Sound sd;                // Sound object
        SoundController sndc;    // Sound Controller

        bool running = true; // Keep main eventloop running

        Stopwatch sw; // High resolution timer
        long tm; // Tick timer

        Random random = new Random(); // Generate Random Numbers

        RectangleF scoreboardLocation; // Location for Scoreboard text
        Font scoreboardFont; // Font for Scoreboard text
        System.Drawing.Brush scoreboardBrushes; // Brush Color for scoreboard text

        int fps=0; // FPS counter
        int frames = 0; // Frame Counter
        bool[] k = new bool[100]; // Keyboard Key Status Array
        bool waiting = true; // Waiting for keypress
        bool frameLock = true; // Frame locked or wide open?

        // **********************************************************************************************************************************************************************************
        // Game Global Variables
        // **********************************************************************************************************************************************************************************
        int shipangle = 0;
        int shipspeed = 10;
        bool ship1fired = false;
        bool ship2fired = false;
        int player1_points = 0;
        int player2_points = 0;
        string scoreboard;


        // **********************************************************************************************************************************************************************************
        // Form and Controller Initializations
        // **********************************************************************************************************************************************************************************
        public asteroidarena()
        {
            InitializeComponent(); // Initialize form

            spc = new SpriteController(); // Initialize Sprite Controller
            sndc = new SoundController(); // Initialize Sound Controller

            // Initialize Background controller and set the background image
            bg = new BackGroundController(@"Graphics\nebulastartscreen.png", pictureMain.Width, pictureMain.Height);
            bg.SetWindow(0, 0, pictureMain.Width, pictureMain.Height);

            this.pictureMain.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureMain_Down); // Handle mouse clicks

            ScoreBoardDefinitions(); // Intialize Scoreboard
            SpriteDefinitions(); // Intialize Sprites
            SoundDefinitions(); // Intialize Sound

            new Task(EventLoop).Start(); // Start Game Event Loop
        }

        // **********************************************************************************************************************************************************************************
        // Sprite Definitions
        // **********************************************************************************************************************************************************************************
        public void SpriteDefinitions()
        {
            // Demo for Collision Masks
            //Sprite.SetCollisionMask(@"Graphics\collisionmask.png");

            // Define ships, explosions, asteroids and laser sprites
            sp = new Sprite(@"Graphics\laserbeam.png", 1); // Laser 1
            sp.SetBorder(0, 0, pictureMain.Width, pictureMain.Height);
            sp.SetVisible(false);
            sp.SetZPos(10); // Laser renders behind the ship
            sp.SetAnimationDirection(0);
            sp.SetCurrentAnimationFrame(0);
            sp.SetMovementSpeed(1);
            sp.SetMovementEnabled(true);
            sp.SetCollisionRadius(15);
            sp.OnSpriteOutsideBorder += new Sprite.SpriteOutsideBorderEvent(SpriteOutsideBorder);
            sp.OnSpriteCollision += new Sprite.SpriteCollisionEvent(Laser1Collision);
            spc.AddSprite(sp, "laser1");

            sp = new Sprite(@"Graphics\laserbeam.png", 1); // Laser 2
            sp.SetBorder(0, 0, pictureMain.Width, pictureMain.Height);
            sp.SetVisible(false);
            sp.SetZPos(10); // Laser renders behind the ship
            sp.SetAnimationDirection(0);
            sp.SetCurrentAnimationFrame(0);
            sp.SetMovementSpeed(1);
            sp.SetMovementEnabled(true);
            sp.SetCollisionRadius(15);
            sp.OnSpriteOutsideBorder += new Sprite.SpriteOutsideBorderEvent(SpriteOutsideBorder);
            sp.OnSpriteCollision += new Sprite.SpriteCollisionEvent(Laser2Collision);
            spc.AddSprite(sp, "laser2");

            sp = new Sprite(@"Graphics\explosion.png", 12); // Explosion 1
            sp.SetBorder(0, 0, pictureMain.Width, pictureMain.Height);
            sp.SetZPos(40);
            sp.SetPosition(0, 0);
            sp.SetAnimationDirection(0);
            sp.SetAnimationSpeed(5);
            sp.SetMovementEnabled(false);
            sp.SetVisible(false);
            sp.SetChecksforCollisions(false);
            sp.SetChecksforBorders(false);
            sp.SetCanCauseCollisions(false);
            sp.OnAnimationComplete += new Sprite.AnimationCompleteEvent(ExplosionAnimationComplete);
            spc.AddSprite(sp, "explosion1");

            sp = new Sprite(@"Graphics\explosion.png", 12); // Explosion 2
            sp.SetBorder(0, 0, pictureMain.Width, pictureMain.Height);
            sp.SetZPos(40);
            sp.SetPosition(0, 0);
            sp.SetAnimationDirection(0);
            sp.SetAnimationSpeed(5);
            sp.SetMovementEnabled(false);
            sp.SetVisible(false);
            sp.SetChecksforCollisions(false);
            sp.SetChecksforBorders(false);
            sp.SetCanCauseCollisions(false);
            sp.OnAnimationComplete += new Sprite.AnimationCompleteEvent(ExplosionAnimationComplete);
            spc.AddSprite(sp, "explosion2");

            for (int x = 0; x < 2; x++)
            {
                sp = new Sprite(@"Graphics\grey_asteroid_large.png", 16); // Large Asteroids
                sp.SetBorder(0, 0, pictureMain.Width, pictureMain.Height);
                sp.SetZPos(30);
                sp.SetMovementDistance(4);
                sp.SetPosition(random.Next(80, pictureMain.Width - 80), random.Next(80, pictureMain.Height - 80));
                sp.SetAnimationSpeed(random.Next(10, 20));
                sp.SetMovementDirection(random.Next(0, 360));
                sp.SetMovementSpeed(random.Next(8, 25));
                sp.SetCollisionRadius(75);
                sp.SetMovementEnabled(true);
                sp.SetVisible(true);
                sp.SetChecksforCollisions(false);
                sp.SetAnimationDirection(-1);
                sp.OnSpriteOutsideBorder += new Sprite.SpriteOutsideBorderEvent(SpriteOutsideBorder);
                spc.AddSprite(sp, "large_asteroid" + x.ToString());
            }

            for (int x = 0; x < 3; x++)
            {
                sp = new Sprite(@"Graphics\copper_asteroid_medium.png", 16); // Medium Asteroids
                sp.SetBorder(0, 0, pictureMain.Width, pictureMain.Height);
                sp.SetZPos(30);
                sp.SetMovementDistance(3);
                sp.SetPosition(random.Next(35, pictureMain.Width - 35), random.Next(35, pictureMain.Height - 35));
                sp.SetAnimationSpeed(random.Next(5, 20));
                sp.SetMovementDirection(random.Next(0, 360));
                sp.SetMovementSpeed(random.Next(6, 15));
                sp.SetCollisionRadius(30);
                sp.SetMovementEnabled(true);
                sp.SetAnimationDirection(1);
                sp.SetVisible(true);
                sp.SetChecksforCollisions(false);
                sp.OnSpriteOutsideBorder += new Sprite.SpriteOutsideBorderEvent(SpriteOutsideBorder);
                spc.AddSprite(sp, "medium_asteroid" + x.ToString());
            }

            for (int x = 0; x < 5; x++)
            {
                sp = new Sprite(@"Graphics\brown_asteroid_small.png", 16); // Small Asteroids
                sp.SetBorder(0, 0, pictureMain.Width, pictureMain.Height);
                sp.SetZPos(30);
                sp.SetMovementDistance(2);
                sp.SetPosition(random.Next(25, pictureMain.Width - 25), random.Next(25, pictureMain.Height - 25));
                sp.SetAnimationSpeed(random.Next(5, 20));
                sp.SetMovementDirection(random.Next(0, 360));
                sp.SetMovementSpeed(random.Next(6, 15));
                sp.SetCollisionRadius(20);
                sp.SetMovementEnabled(true);
                sp.SetAnimationDirection(1);
                sp.SetVisible(true);
                sp.SetChecksforCollisions(false);
                sp.OnSpriteOutsideBorder += new Sprite.SpriteOutsideBorderEvent(SpriteOutsideBorder);
                spc.AddSprite(sp, "small_asteroid" + x.ToString());
            }

            sp = new Sprite(@"Graphics\spaceship1.png", 2); // Ship 1
            sp.SetBorder(0, 0, pictureMain.Width, pictureMain.Height);
            sp.SetAreaClearRadius(100);
            while (true) // Keep picking locations until we are away from the asteroids and each other
            {
                sp.SetPosition(random.Next(30, pictureMain.Width - 30), random.Next(30, pictureMain.Height - 30));
                if (sp.CheckIfAreaClearSprite(spc.sprites).Count == 0) break;
            }
            sp.SetVisible(true);
            sp.SetZPos(20);
            sp.SetMovementSpeed(shipspeed);
            sp.SetAnimationSpeed(50);
            sp.SetMovementDistance(5);
            sp.SetMovementDirection(180);
            sp.SetCollisionRadius(20);
            sp.SetRotationAngle(180);
            sp.SetCurrentAnimationFrame(0);
            sp.SetAnimationDirection(0);
            sp.SetMovementEnabled(true);
            sp.SetAnimateOnce(true);
            //sp.SetChecksforMaskCollisions(true); // Demo for mask collisions
            //sp.SetEnforceMaskCollisions(true);
            //sp.OnSpriteMaskCollision += new Sprite.SpriteMaskCollisionEvent(ShipMaskCollision);
            sp.OnSpriteCollision += new Sprite.SpriteCollisionEvent(Ship1Collision);
            sp.OnSpriteOutsideBorder += new Sprite.SpriteOutsideBorderEvent(SpriteOutsideBorder);
            sp.OnAnimationComplete += new Sprite.AnimationCompleteEvent(TurnOffThruster);
            spc.AddSprite(sp, "ship1");

            sp = new Sprite(@"Graphics\spaceship2.png", 2); // Ship 2
            sp.SetBorder(0, 0, pictureMain.Width, pictureMain.Height);
            sp.SetAreaClearRadius(100);
            while (true) // Keep picking locations until we are away from the asteroids and each other
            {
                sp.SetPosition(random.Next(30, pictureMain.Width - 30), random.Next(30, pictureMain.Height - 30));
                if (sp.CheckIfAreaClearSprite(spc.sprites).Count == 0) break;
            }
            sp.SetVisible(true);
            sp.SetZPos(20);
            sp.SetMovementSpeed(shipspeed);
            sp.SetAnimationSpeed(50);
            sp.SetMovementDistance(5);
            sp.SetMovementDirection(0);
            sp.SetCollisionRadius(20);
            sp.SetRotationAngle(0);
            sp.SetCurrentAnimationFrame(0);
            sp.SetAnimationDirection(0);
            sp.SetMovementEnabled(true);
            sp.SetAnimateOnce(true);
            //sp.SetChecksforMaskCollisions(true); // Demo for mask collisions
            //sp.SetEnforceMaskCollisions(true);
            //sp.OnSpriteMaskCollision += new Sprite.SpriteMaskCollisionEvent(ShipMaskCollision);
            sp.OnSpriteCollision += new Sprite.SpriteCollisionEvent(Ship2Collision);
            sp.OnSpriteOutsideBorder += new Sprite.SpriteOutsideBorderEvent(SpriteOutsideBorder);
            sp.OnAnimationComplete += new Sprite.AnimationCompleteEvent(TurnOffThruster);
            spc.AddSprite(sp, "ship2");
        }

        // **********************************************************************************************************************************************************************************
        // Sound Definitions
        // **********************************************************************************************************************************************************************************
        public void SoundDefinitions()
        {
            // Add Sound Effects
            sd = new Sound(@"\Sounds\8bit_explosion.wav");
            sndc.AddSound(sd, "shipexplosion1");
            sd = new Sound(@"\Sounds\8bit_explosion.wav");
            sndc.AddSound(sd, "shipexplosion2");

            sd = new Sound(@"\Sounds\shiplaser1.wav");
            sd.SetVolume(5);
            sndc.AddSound(sd, "shiplaser1");
            sd = new Sound(@"\Sounds\shiplaser2.wav");
            sd.SetVolume(5);
            sndc.AddSound(sd, "shiplaser2");

            sd = new Sound(@"\Sounds\thruster.wav");
            sd.SetVolume(3);
            sndc.AddSound(sd, "shipthruster1");
            sd = new Sound(@"\Sounds\thruster.wav");
            sd.SetVolume(3);
            sndc.AddSound(sd, "shipthruster2");

            // Add background music
            sd = new Sound(@"\Sounds\asteroidriff.wav");
            sd.SetRepeat(true); // Loop music
            sd.PlayNow(); // Start it right now
            sd.SetVolume(3); // Play background music quieter than sound effects
            sndc.AddSound(sd, "music");
        }

        // **********************************************************************************************************************************************************************************
        // Scoreboard Definitions
        // **********************************************************************************************************************************************************************************
        public void ScoreBoardDefinitions()
        {
            // Initialize the opening screen text
            scoreboardLocation = new RectangleF(pictureMain.Width / 2 - 120, pictureMain.Height / 2 - 200, 300, 400); // Location for Instructions text
            scoreboardFont = new Font("Courier New", 11); // Font for Scoreboard text
            scoreboardBrushes = System.Drawing.Brushes.Chartreuse; // Brush Color for scoreboard text
            scoreboard = "Keys Player 1\n\tA = Rotate Left\n\tD = Rotate Right\n\tW = Thrust\n\tS = Brake\n\tQ or E = Fire\n\nKeys Player 2\n\tJ = Rotate Left\n\tL = Rotate Right\n\tI = Thrust\n\tK = Brake\n\tU or O = Fire\n\n\n   Space Bar to Start!";
        }

        // **********************************************************************************************************************************************************************************
        // Handle Sprite Events
        // **********************************************************************************************************************************************************************************
        private void TurnOffThruster(string id)
        {
            spc.sprites[id].SetCurrentAnimationFrame(0);
            spc.sprites[id].SetAnimationDirection(0);
        }

        private void SpriteOutsideBorder(string id, int border) // Check for objects leaving screen border, bounce if necessary or reset if necessary
        {
            if (id == "laser1") { spc.sprites["laser1"].SetVisible(false); ship1fired = false; return; } // Reset laser1 if it left boarder
            if (id == "laser2") { spc.sprites["laser2"].SetVisible(false); ship2fired = false; return; } // Reset laser2 if it left boarder

            int angle = Sprite.CalculateBorderBounceAngle(border, spc.sprites[id].movementDirection); // Calculate direction of bounce based on which border and what angle

            spc.sprites[id].SetMovementDirection(angle); // Change sprite direction
            if (id == "ship1" || id == "ship2") spc.sprites[id].SetRotationAngle(angle); // If this is our ship also change it's rotation
        }

        private void Ship1Collision(string id, List<string> scl)
        {
            // What did we hit?
            foreach (String s in scl)
            {
                if (s.Contains("laser")) return; // Ignore Lasers
                if (s.Contains("asteroid")) { player1_points--; if (player1_points < 0) player1_points = 0; break; } // Take away points for asteroids
            }

            if (scl.Contains("ship2")) ShipExplosion("ship2"); // If it was the other ship, make sure they blow up too!

            // Handle the explosion
            ShipExplosion("ship1");
        }

        private void Ship2Collision(string id, List<string> scl)
        {
            // What did we hit?
            foreach (String s in scl)
            {
                if (s.Contains("laser")) return; // Ignore Lasers
                if (s.Contains("asteroid")) { player2_points--; if (player2_points < 0) player2_points = 0; break; } // Take away points for asteroids
            }

            if (scl.Contains("ship1")) ShipExplosion("ship1"); // If it was the other ship, make sure they blow up too!

            // Handle the explosion
            ShipExplosion("ship2");
        }

        private void ShipExplosion(string id)
        {
            string explosion;

            if (id == "ship1")
            {
                explosion = "explosion1";
                sndc.sounds["shipexplosion1"].PlayNow(); // Play Explosion sound
            }

            else
            {
                explosion = "explosion2";
                sndc.sounds["shipexplosion2"].PlayNow(); // Play Explosion sound
            }

            spc.sprites[id].SetVisible(false); // Turn off ship

            spc.sprites[explosion].SetPosition(spc.sprites[id].xPos, spc.sprites[id].yPos); // Set explosion to same place as ship
            spc.sprites[explosion].SetAnimateOnce(true); // Explode once
            spc.sprites[explosion].SetCurrentAnimationFrame(0); // Start at the beginning of the explosion
            spc.sprites[explosion].SetAnimationDirection(1); // Allow animation to play
            spc.sprites[explosion].SetVisible(true); // Turn on explosion
        }

        private void Laser1Collision(string id, List<string> scl)
        {
            if (scl.Contains("ship1")) return; // Don't shoot ourselves

            if (scl.Contains("ship2")) // Hit other player, give us a point
            {
                spc.sprites["laser1"].SetVisible(false); ship1fired = false; // Reset Laser
                player1_points++; // Our point
                ShipExplosion("ship2"); // Blow up their ship
                return;
            }

            foreach (String s in scl)
            {
                if (s.Contains("asteroid")) { spc.sprites["laser1"].SetVisible(false); ship1fired = false; } // Asteroids absorb shots
            }
        }

        private void Laser2Collision(string id, List<string> scl)
        {
            if (scl.Contains("ship2")) return; // Don't shoot ourselves

            if (scl.Contains("ship1")) // Hit other player, give us a point
            {
                spc.sprites["laser2"].SetVisible(false); ship2fired = false; // Reset Laser
                player2_points++; // Our point
                ShipExplosion("ship1"); // Blow up their ship
                return;
            }

            foreach (String s in scl)
            {
                if (s.Contains("asteroid")) { spc.sprites["laser2"].SetVisible(false); ship2fired = false; } // Asteroids absorb shots
            }
        }

        private void ExplosionAnimationComplete(string id)
        {
            string ship;

            if (id == "explosion1")
                ship = "ship1";
            else
                ship = "ship2";

            // Turn off the explosion
            spc.sprites[id].SetVisible(false);
            spc.sprites[id].SetAnimationDirection(0); 

            // Reset the ship
            int angle = random.Next(0, 360);
            while (true) // Keep picking locations until we are away from the asteroids
            {
                spc.sprites[ship].SetPosition(random.Next(30, pictureMain.Width - 30), random.Next(30, pictureMain.Height - 30));
                if (spc.sprites[ship].CheckIfAreaClearSprite(spc.sprites).Count == 0) break;
            }
            spc.sprites[ship].SetAnimationDirection(0); 
            spc.sprites[ship].SetMovementSpeed(10);
            spc.sprites[ship].SetMovementDirection(angle);
            spc.sprites[ship].SetRotationAngle(angle);
            spc.sprites[ship].SetCurrentAnimationFrame(0);
            spc.sprites[ship].SetVisible(true);
        }

        // Demo of Background Mask Collisions
        /*
        private void ShipMaskCollision(string id, int color)
        {
            int border = 0;
            if (color == Color.Green.ToArgb()) border = 1; // Horizontal surface
            if (color == Color.Red.ToArgb()) border = 2; // Verticle surface

            if (border == 0)
                return; // Neither color

            int angle = Sprite.CalculateBorderBounceAngle(border, sc.sprites[id].movementDirection); // Calculate direction of bounce based on which border and what angle

            sc.sprites[id].SetMovementDirection(angle); // Change sprite direction
            sc.sprites[id].SetRotationAngle(angle); // If this is our ship also change it's rotation
        }
        */

        // **********************************************************************************************************************************************************************************
        // Handle Keyboard Events
        // **********************************************************************************************************************************************************************************
        private void CheckKeyboard() // Handle form Keypresses
        {
            // Check for spacebar
            if (k[12]) waiting = false;

            // Ship 1 Controls
            if (spc.sprites["ship1"].visible) // Only control if ship is visible
            {
                if (k[0]) // W 
                {
                    shipspeed -= 1;
                    if (shipspeed < 3) shipspeed = 3;
                    spc.sprites["ship1"].SetMovementSpeed(shipspeed);
                    spc.sprites["ship1"].SetCurrentAnimationFrame(1); // Thruster ship animation
                    spc.sprites["ship1"].SetAnimationDirection(1);
                    if (!sndc.sounds["shipthruster1"].soundPlaying) // Play thruster sound
                        sndc.sounds["shipthruster1"].PlayNow(); 
                }

                if (k[1]) // S
                {
                    shipspeed += 1;
                    if (shipspeed > 20) shipspeed = 20;
                    spc.sprites["ship1"].SetMovementSpeed(shipspeed);
                    spc.sprites["ship1"].SetCurrentAnimationFrame(0); // Regular ship animation
                    spc.sprites["ship1"].SetAnimationDirection(0);
                }

                if (k[3]) // D
                {
                    shipangle = spc.sprites["ship1"].rotationAngle;
                    shipangle += 10;
                    spc.sprites["ship1"].SetRotationAngle(shipangle);
                    spc.sprites["ship1"].SetMovementDirection(shipangle);
                }

                if (k[2]) // A
                {
                    shipangle = spc.sprites["ship1"].rotationAngle;
                    shipangle -= 10;
                    spc.sprites["ship1"].SetRotationAngle(shipangle);
                    spc.sprites["ship1"].SetMovementDirection(shipangle);
                }

                if (k[8] || k[10]) // Q Ship 1 Fire
                {
                    if (ship1fired) return; // Only fire one at a time!
                    ship1fired = true;

                    spc.sprites["laser1"].SetPosition(spc.sprites["ship1"].xPos, spc.sprites["ship1"].yPos);
                    spc.sprites["laser1"].SetMovementDirection(spc.sprites["ship1"].movementDirection);
                    spc.sprites["laser1"].SetRotationAngle(spc.sprites["ship1"].rotationAngle);
                    spc.sprites["laser1"].SetVisible(true);

                    sndc.sounds["shiplaser1"].PlayNow(); // Play Laser sound
                }
            }

            // Ship 2 Controls
            if (spc.sprites["ship2"].visible) // Only control if ship is visible
            {
                if (k[4]) // I 
                {
                    shipspeed -= 1;
                    if (shipspeed < 3) shipspeed = 3;
                    spc.sprites["ship2"].SetMovementSpeed(shipspeed);
                    spc.sprites["ship2"].SetCurrentAnimationFrame(1); // Thruster ship animation
                    spc.sprites["ship2"].SetAnimationDirection(1);
                    if (!sndc.sounds["shipthruster2"].soundPlaying) // Play thruster sound once
                        sndc.sounds["shipthruster2"].PlayNow(); 
                }

                if (k[5]) // K
                {
                    shipspeed += 1;
                    if (shipspeed > 20) shipspeed = 20;
                    spc.sprites["ship2"].SetMovementSpeed(shipspeed);
                    spc.sprites["ship2"].SetCurrentAnimationFrame(0); // Regular ship animation
                    spc.sprites["ship2"].SetAnimationDirection(0);
                }

                if (k[7]) // L
                {
                    shipangle = spc.sprites["ship2"].rotationAngle;
                    shipangle += 10;
                    spc.sprites["ship2"].SetRotationAngle(shipangle);
                    spc.sprites["ship2"].SetMovementDirection(shipangle);
                }

                if (k[6]) // J
                {
                    shipangle = spc.sprites["ship2"].rotationAngle;
                    shipangle -= 10;
                    spc.sprites["ship2"].SetRotationAngle(shipangle);
                    spc.sprites["ship2"].SetMovementDirection(shipangle);
                }

                if (k[9] || k[11]) // O Ship 2 Fire
                {
                    if (ship2fired) return; // Only fire one at a time!
                    ship2fired = true;
                    spc.sprites["laser2"].SetPosition(spc.sprites["ship2"].xPos, spc.sprites["ship2"].yPos);
                    spc.sprites["laser2"].SetMovementDirection(spc.sprites["ship2"].movementDirection);
                    spc.sprites["laser2"].SetRotationAngle(spc.sprites["ship2"].rotationAngle);
                    spc.sprites["laser2"].SetVisible(true);

                    sndc.sounds["shiplaser2"].PlayNow(); // Play Laser sound
                }
            }
        }

        // Key UP and DOWN overrides to allow for multiple keys at once and faster response time. Key up and down codes MUST match!
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.KeyCode)
            {
                case Keys.W:
                    k[0] = true;
                    break;
                case Keys.S:
                    k[1] = true;
                    break;
                case Keys.A:
                    k[2] = true;
                    break;
                case Keys.D:
                    k[3] = true;
                    break;
                case Keys.I:
                    k[4] = true;
                    break;
                case Keys.K:
                    k[5] = true;
                    break;
                case Keys.J:
                    k[6] = true;
                    break;
                case Keys.L:
                    k[7] = true;
                    break;
                case Keys.Q:
                    k[8] = true;
                    break;
                case Keys.O:
                    k[9] = true;
                    break;
                case Keys.E:
                    k[10] = true;
                    break;
                case Keys.U:
                    k[11] = true;
                    break;
                case Keys.Space:
                    k[12] = true;
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            switch (e.KeyCode)
            {
                case Keys.W:
                    k[0] = false;
                    break;
                case Keys.S:
                    k[1] = false;
                    break;
                case Keys.A:
                    k[2] = false;
                    break;
                case Keys.D:
                    k[3] = false;
                    break;
                case Keys.I:
                    k[4] = false;
                    break;
                case Keys.K:
                    k[5] = false;
                    break;
                case Keys.J:
                    k[6] = false;
                    break;
                case Keys.L:
                    k[7] = false;
                    break;
                case Keys.Q:
                    k[8] = false;
                    break;
                case Keys.O:
                    k[9] = false;
                    break;
                case Keys.E:
                    k[10] = false;
                    break;
                case Keys.U:
                    k[11] = false;
                    break;
                case Keys.Space:
                    k[12] = false;
                    break;
            }
        }

        // **********************************************************************************************************************************************************************************
        // Game Event Loop
        // **********************************************************************************************************************************************************************************
        private void EventLoop()
        {
            long screenrefresh_timer = 0;
            long keyboardrefresh_timer = 0;
            long fps_timer = 0;
            long seconds_left; // How long the match lasts
            long ms = Stopwatch.Frequency / 1000;  // Precalculate timers for better speed
            long scntm;
            long keytm = ms * 50; // Check Keyboard every 50ms
            long fpstm = ms * 1000; // Show Scoreboard every second

            if (frameLock) // Framelock?
                scntm = ms * 1000 / 100; // Framelock 100fps
            else
                scntm = 0;

            sw = Stopwatch.StartNew(); // Start high resolution timer

            // Show Startup and countdown screens
            Sprite.SetSpritesEnabled(false); // Show only background to start

            pictureMain.Invalidate(); // Show the opening screen

            while (waiting) // Wait for spacebar press
            {
                tm = sw.ElapsedTicks; // Get the current tick
                if (keyboardrefresh_timer < tm) // Keyboard Check
                {
                    CheckKeyboard();
                    keyboardrefresh_timer = tm + keytm;
                }
            }

            scoreboardLocation = new RectangleF(pictureMain.Width / 2 - 50, pictureMain.Height / 2 - 100, 250, 250); // Location for countdown text
            scoreboardFont = new Font("Lucida Sans", 100); // Font for Scoreboard text
            scoreboardBrushes = System.Drawing.Brushes.Red; // Brush Color for scoreboard text
            waiting = true;
            seconds_left = 5; // 5 second count down

            while (waiting) // Wait for count down
            {
                tm = sw.ElapsedTicks; // Get the current tick

                if (fps_timer < tm) // Update scoreboard
                {
                    scoreboard = seconds_left.ToString(); // Scoreboard text
                    pictureMain.Invalidate(); // Render text

                    if (seconds_left < 1) waiting = false; // Check countdown finished
                    seconds_left--;

                    fps_timer = tm + fpstm;
                }
            }

            seconds_left = 120; // 2 minute round

            Sprite.SetSpritesEnabled(true); // Show the sprites to start the game

            bg.SetBackground("Graphics\\nebula.png", pictureMain.Width, pictureMain.Height); // Change to game screen background
            bg.SetWindow(0, 0, pictureMain.Width, pictureMain.Height);

            scoreboardLocation = new RectangleF(pictureMain.Width / 2 - 150, 20, 300, 50); // Location for scoreboard text
            scoreboardFont = new Font("Tahoma", 14); // Font for Scoreboard text
            scoreboardBrushes = System.Drawing.Brushes.Chartreuse; // Brush Color for scoreboard text
            scoreboard = "P1: " + player1_points + "  P2: " + player2_points + "   SL:" + seconds_left.ToString() + "   " + fps.ToString() + "fps"; // Scoreboard text

            // Main Game Loop
            while (running)
            {
                tm = sw.ElapsedTicks; // Get the current tick

                if (screenrefresh_timer < tm) // Screen Refresh
                {
                    pictureMain.Invalidate();
                    screenrefresh_timer = tm + scntm;
                }

                if (keyboardrefresh_timer < tm) // Keyboard Check
                {
                    CheckKeyboard();
                    keyboardrefresh_timer = tm + keytm;
                }

                if (fps_timer < tm) // Update scoreboard and FPS
                {
                    fps = frames;
                    scoreboard = "P1: " + player1_points + "  P2: " + player2_points + "   SL:" + seconds_left.ToString() + "   " + fps.ToString() + "fps"; // Scoreboard text
                    
                    if (seconds_left < 0) running = false; // Check if Game Over
                    seconds_left--;

                    frames = 0;
                    fps_timer = tm + fpstm;
                }
            }

            // End the game!
            scoreboard = "P1: " + player1_points + "  P2: " + player2_points + " GAME OVER!"; // Scoreboard text
            pictureMain.Invalidate();

            sw.Stop(); // Stop the timer

            System.Threading.Thread.Sleep(5000); // Allow Game Over screen for 5 sec.

            Application.Restart(); // Restart the game
            Environment.Exit(0); // Exit old application
        }

        // **********************************************************************************************************************************************************************************
        // Handle Mouse Events
        // **********************************************************************************************************************************************************************************
        private void pictureMain_Down(object sender, MouseEventArgs e)
        {
            // Demo of Mouse Event handling. If uncommented ships will blow up when clicked by mouse
            //if (sc.sprites["ship1"].CheckIfMouseOver(e.Location.X, e.Location.Y)) ShipExplosion("ship1"); 
            //if (sc.sprites["ship2"].CheckIfMouseOver(e.Location.X, e.Location.Y)) ShipExplosion("ship2");
        }

        // **********************************************************************************************************************************************************************************
        // Call Render Engine - DO NOT CHANGE!
        // **********************************************************************************************************************************************************************************
        private void pictureMain_Paint(object sender, PaintEventArgs e)
        {
            Renderer.Render(e, bg, spc, sp, sndc, sd, scoreboard, scoreboardFont, scoreboardBrushes, scoreboardLocation); // Render graphics and sound
            frames++; // Frame Counter
        }
    }
}
