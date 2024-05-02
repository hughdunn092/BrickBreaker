﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrickBreaker.Screens
{
    public partial class PauseScreen : UserControl
    {
        GameScreen parentScreen;
        public PauseScreen(GameScreen _parentScreen)
        {
            parentScreen = _parentScreen;
            InitializeComponent();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void levelButton_Click(object sender, EventArgs e)
        {
            // Goes to the level screen
            LevelScreen ls = new LevelScreen();
            Form form = this.FindForm();


            form.Controls.Add(ls);
            form.Controls.Remove(parentScreen);
            form.Controls.Remove(this);

            ls.Location = new Point((form.Width - ls.Width) / 2, (form.Height - ls.Height) / 2);
        }

        private void continueButton_Click(object sender, EventArgs e)
        {
            parentScreen.gameTimer.Enabled = true;
            parentScreen.Controls.Remove(this);
            parentScreen.isPaused = false;
            parentScreen.Focus();
        }
    }
}
