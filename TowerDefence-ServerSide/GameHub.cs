﻿using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using TowerDefence_SharedContent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TowerDefence_SharedContent.Towers;
using TowerDefence_SharedContent.Soldiers;
using System.Drawing;
using System.Linq;
using System.Threading;
using TowerDefence_ServerSide.Facade;

namespace TowerDefence_ServerSide
{
    public class GameHub : Hub
    {
        private readonly PatternFacade facade = PatternFacade.GetInstance();
        public void CreateMap(String mapType)
        {
            MapController.CreateInstance();
            Map map = facade.CreateMap(mapType);

            MapController mapController = MapController.GetInstance();
            mapController.Attach(map);
        }

        public void AddPlayer(PlayerType playerType)
        {
            MapController mapController = MapController.GetInstance();
            mapController.AddPlayer(playerType);
        }

        public void BuySoldier(PlayerType playerType, SoldierType soldierType)
        {
            MapController mapController = MapController.GetInstance();
            mapController.AddSoldier(facade.TrainSoldier(playerType, soldierType), playerType);
            MyConsole.WriteLineWithCount($"{playerType}: buySoldier");
        }

        public void BuyTower(PlayerType playerType, TowerType towerType, Point point)
        {
            MapController mapController = MapController.GetInstance();
            mapController.AddTower(facade.CreateTower(playerType, towerType, point), playerType);
            MyConsole.WriteLineWithCount($"{playerType}: buyTower");                   
        }
        public void RestartGame()
        {
            MapController mapController = MapController.GetInstance();
            mapController.Restart();
        }
        public void BuyTwoSoldier(PlayerType playerType, SoldierType soldierType)
        {
            Thread thread1 = new Thread(new ThreadStart(() =>
            {
                var controller = MapController.GetInstance();
                controller.AddSoldier(facade.TrainSoldier(playerType, soldierType), playerType);
            }));
            Thread thread2 = new Thread(new ThreadStart(() =>
            {
                var controller = MapController.GetInstance();
                controller.AddSoldier(facade.TrainSoldier(playerType, soldierType), playerType);
            }));
            thread1.Start();
            thread2.Start();
        }

        public void DeleteTower(PlayerType playerType)
        {
            MyConsole.WriteLineWithCount($"{playerType}: deleteTower");

        }

        public void Upgrade(PlayerType playerType, List<IdableObject> objects)
        {
            MapController mapController = MapController.GetInstance();
            mapController.UpgradeTowers(playerType, objects);
            mapController.UpgradeSoldiers(playerType, objects);
        }
    }
}
