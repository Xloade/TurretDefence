﻿using System;
using System.Drawing;
using TowerDefence_ClientSide.shapes;
using TowerDefence_SharedContent;
using TowerDefence_ClientSide.Composite;

namespace TowerDefence_ClientSide
{
    public class Shape : IDraw, ICloneable, Composite.IShapeComposite, IShape, ISelected
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public PlatoonType PlatoonType { get; set; }
        public DrawInfo Info { get; set; }
        public float CenterX => Info.Coordinates.X;
        public float CenterY => Info.Coordinates.Y;
        public float Rotation => Info.Rotation;
        public Image SpriteImage;
        public IDraw DecoratedDrawInterface { get; set; }
        public bool Selected { get; set; }

        public Shape(DrawInfo drawInfo, float width, float height, Image spriteImage)
        {
            Width = width;
            Height = height;
            this.SpriteImage = (Image)spriteImage.Clone();
            Info = drawInfo;
            DecoratedDrawInterface = this;
        }
        public Shape()
        {

        }
        public void Draw(Graphics gr)
        {
            MyConsole.WriteLineWithCount("Drawing shape");
            int biggerSide = (int)(Math.Max(Width, Height) * 1.5);
            Bitmap bmp = new Bitmap(biggerSide, biggerSide);


            using (Graphics grImage = Graphics.FromImage(bmp))
            {
                grImage.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                grImage.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);
                //Rotate.        
                grImage.RotateTransform(Rotation);
                //Move image back.
                grImage.TranslateTransform(-Width / 2, -Height / 2);
                lock (this)
                {
                    //bullet prototipe doesnt do deep enough copy
                    lock (SpriteImage)
                    {
                        grImage.DrawImage(SpriteImage, 0, 0, Width, Height);
                    }
                }
            }
            lock (gr)
            {
                gr.DrawImage(bmp, CenterX - (bmp.Width / 2), CenterY - (bmp.Height / 2), bmp.Width, bmp.Height);
            }
        }
        // piešimas vykdomas išvestinėse klasėse

        public object Clone()
        {
            return (Shape)this.MemberwiseClone();
        }
        public void DecoratedDraw(Graphics gr)
        {
            DecoratedDrawInterface.Draw(gr);
        }
        public void GroupDraw(Graphics gr)
        {
            DecoratedDraw(gr);
        }

        public Shape GetNextShape(long last)
        {
            return this;
        }

        public void DeleteShape(Shape shape)
        {
            // Method intentionally left empty.
        }
        public void UpdatePlatoon(PlatoonType platoonType)
        {
            PlatoonType = platoonType;
        }

        public void UpdateSelection(PlatoonType platoonType)
        {
            Selected = platoonType == PlatoonType.Selected;
        }

        public void SaveSelection(MouseSelection mouseSelection) { }
    }
}