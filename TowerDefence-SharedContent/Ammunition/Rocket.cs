﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TowerDefence_SharedContent.Towers;

namespace TowerDefence_SharedContent
{
    public class Rocket : Ammunition, IMove
    {
        public override Point Coordinates { get; set; }
        public override int Speed { get; set; }
        public override AmmunitionType AmmunitionType { get; set; }
        public int Width { get; set; }

        public Rocket(Point towerCoordinates, AmmunitionType ammunitionType, int power) : base(towerCoordinates, ammunitionType, power)
        {
            Coordinates = towerCoordinates;
            Speed = 5;
            AmmunitionType = ammunitionType;
            Width = 300;
        }     

        public override bool CanDestroy(Point soldierCoordinates, PlayerType playerType)
        {
            return soldierCoordinates.X <= this.Coordinates.X + this.Width / 2 && soldierCoordinates.X >= this.Coordinates.X - this.Width / 2;
        }

        public override void MoveForward(PlayerType playerType)
        {
            // rocket doesn't move
        }

    }
}
