﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameMapEditor.Objects
{
    //[Serializable]
    class GameTile : IDrawable
    {
        private Bitmap texture; 

        public GameTile()
        {
            this.texture = null;
            this.Position = new Point();
        }

        public GameTile(Point position)
        {
            this.Position = position;
        }

        public void Draw(PaintEventArgs e)
        {
            if(this.texture != null)
                e.Graphics.DrawImage(this.texture, new Point(this.Position.X * GlobalData.TileSize.Width, this.Position.Y * GlobalData.TileSize.Height));
        }

        public void Draw(Point origin, PaintEventArgs e)
        {
            if (this.texture != null)
                e.Graphics.DrawImage(this.texture, new Point(
                    this.Position.X * GlobalData.TileSize.Width - origin.X,
                    this.Position.Y * GlobalData.TileSize.Height - origin.Y));
        }

        public Bitmap Texture
        {
            get { return this.texture; }
            set { this.texture = value; }
        }

        public Point Position
        {
            get;
            set;
        }

        // TODO : Only for debug
        public override string ToString()
        {
            if (texture != null)
                return "[1]";
            return string.Empty;
        }
    }
}