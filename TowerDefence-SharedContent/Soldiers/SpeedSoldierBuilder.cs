﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TowerDefence_SharedContent.Soldiers
{
    public class SpeedSoldierBuilder : SoldierBuilder
    {
        public SpeedSoldierBuilder(PlayerType playerType, SoldierType soldierType, int level)
        {
            MyConsole.WriteLineWithCount("Builder: build speed soldier");
            soldier = new Soldier(playerType, soldierType, level);
        }
        public override void BuildHitpoints()
        {
            soldier.Hitpoints = new int[] { 5, 10, 15 };
            soldier.CurrentHitpoints = soldier.Hitpoints[soldier.Level];
        }

        public override void BuildSpeed()
        {
            soldier.Speeds = new double[] { 2, 3, 4 };
        }

        public override void BuildSprite(PlayerType playerType)
        {
            soldier.Sprite = SpritePaths.GetSoldier(playerType, soldier.SoldierType);
        }
    }
}
