﻿using System;
using BrickBreaker.Properties;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using System.Xml;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Reflection.Emit;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BrickBreaker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            #region Creating Minecraft Font

            int fontLength = Properties.Resources.MinecraftFont.Length;
            byte[] fontdata = Properties.Resources.MinecraftFont;
            System.IntPtr data = Marshal.AllocCoTaskMem(fontLength);
            Marshal.Copy(fontdata, 0, data, fontLength);

            //Add this font to the private font collection
            pfc.AddMemoryFont(data, fontLength);
            #endregion
        }

        public static void SetLevelFonts(UserControl uc)
        {
            foreach (System.Windows.Forms.Button b in uc.Controls.OfType<System.Windows.Forms.Button>())
            {
                b.UseCompatibleTextRendering = true;
                b.Font = new Font(pfc.Families[0], b.Font.Size);
            }
            foreach (System.Windows.Forms.Label l in uc.Controls.OfType<System.Windows.Forms.Label>())
            {
                l.UseCompatibleTextRendering = true;
                l.Font = new Font(pfc.Families[0], l.Font.Size);
            }

        }



        //Create new private font collection
        public static PrivateFontCollection pfc = new PrivateFontCollection();


        #region Block ID & Powerup ID Data
        public static string[][] blockData = new string[][]
        {
        new string [] {"Hp", "Weak To", "Png", "Chance Of Powerup (0 - 10)", "ID of Powerup"},

        new string [] {"1", "Shovel", "grass_block", "1", "2"},      //Grass Block      |   //Seeds
        new string [] {"3", "Axe", "oak_log", "0.1", "1"},           //Oak Wood Log     |
        new string [] {"1", "Hoe", "oak_leaves", "2", "1"},          //Oak Leaves       |   //Apple
        new string [] {"3", "Axe", "oak_planks", "0.1", "1"},        //Oak Planks       |
        new string [] {"2", "Pick", "stone", "1", "3"},              //Stone            |   //Stone Pick
        
        new string [] {"2", "Pick", "iron_ore", "1", "1"},           //Iron Ore         |
        new string [] {"3", "Pick", "gold_ore", "1", "1"},           //Gold Ore         |
        new string [] {"2", "Pick", "diamond_ore", "1", "1"},        //Diamond Ore      |
        new string [] {"5", "Pick", "obsidian", "0.1", "1"},         //Obsidian         |
        new string [] {"2", "Pick", "netherrack", "0.1", "1"},       //Netherack        |
        
        new string [] {"3", "Pick", "quartz_ore", "1", "1"},         //Quartz Ore       |
        new string [] {"4", "Pick", "netherite", "1", "1"},          //Netherite        |
        new string [] {"10", "Sword", "endframe_side", "0.4", "1"},  //End Portal Block |
        new string [] {"4", "Pick", "stonebrick", "0.2", "1"},       //Stone Bricks     |
        new string [] {"4", "Pick", "end_stone", "0.1", "1"},        //Endstone         |
        
        new string [] {"4", "Pick", "end_bricks", "0.1", "1"},       //Endstone Bricks  |
        new string [] {"2", "Shovel", "sand", "0.1", "1"},           //Sand             |
        new string [] {"2", "Shovel", "gravel", "0.1", "1"},         //Gravel           |
        new string [] {"4", "Pick", "coal_ore", "1", "1"},           //Coal Ore         |
        new string [] {"2", "Sword", "water", "0.1", "1"},           //Water            |
        
        new string [] {"2", "Sword", "lava", "0.1", "1"},            //Lava             |
        new string [] {"1", "Sword", "portal", "0", "1"},            //Nether Portal    |
        new string [] {"2", "Sword", "bedrock", "0.1", "1"},         //Bedrock          |
        new string [] {"4", "Sword", "dragon_egg", "1", "1"},        //Dragon Egg       |
        new string [] {"3", "Pick", "cobblestone", "0.1", "1"},      //Cobblestone      |

        };

        public static string[][] powerupData = new string[][]
     {
        new string[]{"Fallspeed", "Activetime", "Png", "Radius" },
        new string[]{"3", "400", "apple", "10"},
        new string[]{"3", "400", "seeds", "10"},
        new string[]{"3", "400", "stone_pickaxe", "10"},
     };
        #endregion
        public static int globalTimer;
        public static int tickDeltaTime = 10;

        public static int currentLevel = 1;


        #region helperFunctions

        public static bool isNegative(float num) { return (Math.Abs(num) != num); }

        public static int timeSincePoint(int checkedTime)
        {
            return checkedTime < globalTimer ? (globalTimer - checkedTime) : -1;  // returns -1 if checkedtime is in the future
        }

        public static float clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static void ChangeScreen(object sender, UserControl next)
        {

            Form f; // will either be the sender or parent of sender 

            if (sender is Form)
            {
                f = (Form)sender;
            }
            else
            {
                UserControl current = (UserControl)sender;
                f = current.FindForm();
                f.Controls.Remove(current);
            }

            next.Location = new Point((f.ClientSize.Width - next.Width) / 2, (f.ClientSize.Height - next.Height) / 2);
            f.Controls.Add(next);

            next.Focus();
        }

        public static bool IsWithinRange(float num, float lowerBound, float upperBound) { return num >= lowerBound && num <= upperBound; }

        public static float GreaterOf(float num1, float num2) { return num1 > num2 ? num1 : num2; }

        #endregion



        #region gameLogic

        public static int CheckCollision(Ball ball, Paddle rectObject, int collisionTimeStamp)
        { //returns 0 (no collision) or 1-4 (collides from the rectangle's top, right side, bottom and left side respectively)

            if (timeSincePoint(collisionTimeStamp) <= 8) { return 0; }

            Point ballCenter = new Point(ball.x + ball.radius, ball.y + ball.radius);
            Point rectCenter = new Point(rectObject.x + (rectObject.width / 2), rectObject.y + (rectObject.height / 2));

            bool CollidesX = Math.Abs(ballCenter.X - rectCenter.X) <= (ball.radius + rectObject.width / 2);
            bool CollidesY = Math.Abs(ballCenter.Y - rectCenter.Y) <= (ball.radius + rectObject.height / 2);

            if (!CollidesX || !CollidesY) { return 0; } //return false values if there is no chance of collision

            //prioritize collisions with the top / bottom of an object unless rectTop < ballY < rectBottom

            if (CollidesX && IsWithinRange(ballCenter.Y, rectObject.y, rectObject.y + rectObject.height))
            {
                return (ball.x > rectObject.x) ? 2 : 4;
            }
            if (CollidesY)
            {
                return (ball.y > rectObject.y) ? 3 : 1;
            }

            return 0; //return 0 if no collision was detected (shouln't really be used if things are working properly but it yells at me if I delete)
        }

        public static int CheckCollision(Ball ball, Block rectObject, int collisionTimeStamp)
        { //returns 0 (no collision) or 1-4 (collides from the rectangle's top, right side, bottom and left side respectively)

            if (timeSincePoint(collisionTimeStamp) <= 4) { return 0; }

            Point ballCenter = new Point(ball.x + ball.radius, ball.y + ball.radius);
            Point rectCenter = new Point(rectObject.x + (rectObject.width / 2), rectObject.y + (rectObject.height / 2));

            bool CollidesX = Math.Abs(ballCenter.X - rectCenter.X) <= (ball.radius + rectObject.width / 2);
            bool CollidesY = Math.Abs(ballCenter.Y - rectCenter.Y) <= (ball.radius + rectObject.height / 2);

            if (!CollidesX || !CollidesY) { return 0; } //return false values if there is no chance of collision

            //prioritize collisions with the top / bottom of an object unless rectTop < ballY < rectBottom

            if (CollidesX && IsWithinRange(ballCenter.Y, rectObject.y, rectObject.y + rectObject.height))
            {
                return (ball.x > rectObject.x) ? 2 : 4;
            }

            if (CollidesY)
            {
                return (ball.y > rectObject.y) ? 3 : 1;
            }

            return 0; //return 0 if no collision was detected
        }

        #endregion

        #region XMLPacking

        string x, y, width, height, id;
        List<Block> blocks = new List<Block>();


        public void LevelReader()
        {
            XmlReader reader = XmlReader.Create("Resources/GenXML.xml");

            while (reader.Read())
            { //exPLODE (thanks hark)
                if (reader.NodeType == XmlNodeType.Text)
                {

                    x = reader.ReadString();

                    reader.ReadToNextSibling("y");
                    y = reader.ReadString();

                    reader.ReadToNextSibling("width");
                    width = reader.ReadString();

                    reader.ReadToNextSibling("height");
                    height = reader.ReadString();

                    reader.ReadToNextSibling("id");
                    id = reader.ReadString();

                }
            }
        }

        #endregion



        private void Form1_Load(object sender, EventArgs e)
        {
            // Start the program centred on the Menu Screen
            MenuScreen ms = new MenuScreen();
            this.Controls.Add(ms);

            ms.Location = new Point((this.Width - ms.Width) / 2, (this.Height - ms.Height) / 2);
        }
    }
}
