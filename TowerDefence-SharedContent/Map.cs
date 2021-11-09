﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using TowerDefence_SharedContent.Soldiers;
using TowerDefence_SharedContent.Towers;

namespace TowerDefence_SharedContent
{
    public class Map : IMapObserver
    {
        public string backgroundImageDir;

        public List<Player> players;

        public Map()
        {
            players = new List<Player>();
        }

        public void AddPlayer(PlayerType playerType)
        {
            lock (this)
            {
                players.RemoveAll((player)=> player.PlayerType == playerType);
                players.Add(new Player(playerType));
            }
        }


        public Player GetPlayer(PlayerType type)
        {
            lock (this)
            {
                return players.Find(player => player.PlayerType == type);
            }
        }

        public Player GetPlayerEnemy(PlayerType type)
        {
            lock (this)
            {
            return players.Find(player => player.PlayerType != type);
            }
        }

        public string ToJson()
        {
            JObject mapJson;
            lock (this)
            {
                mapJson = (JObject)JToken.FromObject(this);
            }
            return mapJson.ToString();
        }

        public void AddSoldier(Soldier soldier, PlayerType playerType)
        {
            lock (this)
            {
                foreach (Player player in players)
                {
                    if (player.PlayerType == playerType)
                    {
                        player.soldiers.Add(soldier);
                    }
                }
            }
        }

        public void AddTower(Tower tower, PlayerType playerType)
        {
            lock (this)
            {
                foreach (Player player in players)
                {
                    if (player.PlayerType == playerType)
                    {
                        player.towers.Add(tower);
                    }
                }
            }
        }

        public void UpdateSoldierMovement()
        {
            lock (this)
            {
                foreach (Player player in players)
                {
                    player.UpdateSoldierMovement();
                }
            }
        }

        public void UpdateTowerActivity()
        {
            lock (this)
            {
                if (players.Count > 1)
                {
                    foreach (Player player in players)
                    {
                        player.UpdateTowerActivity(GetPlayerEnemy(player.PlayerType).soldiers);
                    }
                }
            }         
        }
    }
}
