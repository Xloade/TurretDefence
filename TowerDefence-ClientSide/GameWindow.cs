﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using TowerDefence_SharedContent.Towers;
using TowerDefence_SharedContent;
using TowerDefence_SharedContent.Soldiers;
using TowerDefence_ClientSide.Prototype;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TowerDefence_ClientSide.Composite;

namespace TowerDefence_ClientSide
{
    public class GameWindow : Window, ICursorChange, IPlayerStats, IHaveShapePlatoon
    {
        private const string BUTTON_BUY_SOLDIER = "Buy soldier";
        private const string BUTTON_BUY_TOWER = "Buy tower";
        private const string BUTTON_RESTART_GAME = "Restart game";
        private const string BUTTON_DELETE_TOWER = "Delete tower";
        private const string BUTTON_UPGRADE_SOLDIER = "Upgrade soldier";
        private const string BUTTON_QUICK_BUY = "Quick buy two";
        private const string SERVER_URL = "https://localhost:5001/GameHub";
        Map currentMap;

        public ShapePlatoon Shapes { get; set; } = new ShapePlatoon(PlatoonType.Root);
        private MapUpdater mapUpdater;

        HubConnection connection;
        private PlayerType playerType;
        private PlayerStatsShowStatus PlayerStatsShowStatus = PlayerStatsShowStatus.All;
        private CursorState cursorState = CursorState.Default;
        private GameCursor gameCursor;
        private Command cursorCommand;
        private string towerToBuy = "";
        private System.Windows.Forms.Timer renderTimer = new System.Windows.Forms.Timer();
        private SelectionDrawing selectionDrawing = new SelectionDrawing();
        string IPlayerStats.LifePointsText{ set { LifePointsText.Text = value; } }

        string IPlayerStats.TowerCurrencyText { set { TowerCurrencyText.Text = value; } }
        string IPlayerStats.SoldierCurrencyText { set { SoldierCurrencyText.Text = value; } }

        PlayerStatsShowStatus IPlayerStats.PlayerStatsShowStatus { get { return PlayerStatsShowStatus; }}
        public GameWindow(PlayerType playerType, String mapType) : base(mapType, playerType.ToString(),
            1000, 700, BUTTON_BUY_SOLDIER, BUTTON_BUY_TOWER, BUTTON_RESTART_GAME, BUTTON_DELETE_TOWER, BUTTON_UPGRADE_SOLDIER, BUTTON_QUICK_BUY)
        {
            mapUpdater = new MapUpdater(this,playerType);
            gameCursor = new GameCursor(this, playerType);
            cursorCommand = new CursorCommand(gameCursor);
            this.playerType = playerType;
            startSignalR(mapType);
            MapParser.CreateInstance();
            renderTimer.Tick += RenderTimer_Tick;
            renderTimer.Interval = 10;
            renderTimer.Start();
        }

        private void RenderTimer_Tick(object sender, EventArgs e)
        {
            renderTimer.Stop();
            if (currentMap != null)
            {
                mapUpdater.UpdateMap(currentMap, out bgImage, this, selectionDrawing.Selection);
                Refresh();
            }
            renderTimer.Start();
        }

        
        private void startSignalR(String mapType)
        {
            connection = new HubConnectionBuilder().WithUrl(SERVER_URL).Build();
            connection.On<string>("ReceiveMessage", ReceiveMessage);
            connection.StartAsync();
            connection.SendAsync("createMap", mapType);
            connection.SendAsync("addPlayer", playerType);
        }

        private void ReceiveMessage(string updatedMapJson)
        {
            MapParser mapParser = MapParser.getInstance();
            currentMap = mapParser.Parse(updatedMapJson);
        }

        protected override void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics gr = e.Graphics;
            gr.DrawImage(bgImage, 0, 0, DrawArea.Width, DrawArea.Height);
            gr.SmoothingMode = SmoothingMode.HighSpeed;

            foreach (Shape shape in Shapes)
            {
                shape.DecoratedDraw(gr);
            }
            /*Parallel.ForEach(shapes, shape =>
            {
                shape.DecoratedDraw(gr);
            });*/
            selectionDrawing.Draw(gr);

        }

        protected override void btn_Click(object sender, EventArgs e)
        {
            switch (((Button)sender).Name)
            {
                case BUTTON_BUY_SOLDIER:
                    OpenSoldierSelection();
                    break;
                case BUTTON_BUY_TOWER:
                    OpenTowerSelection();
                    break;
                case BUTTON_DELETE_TOWER:
                    connection.SendAsync("deleteTower", playerType);
                    break;
                case BUTTON_UPGRADE_SOLDIER:
                    connection.SendAsync("upgradeSoldier", playerType);
                    break;
                case BUTTON_RESTART_GAME:
                    connection.SendAsync("restartGame");
                    break;
                case BUTTON_QUICK_BUY:
                    connection.SendAsync("buyTwoSoldier", playerType, SoldierType.HitpointsSoldier);
                    break;
                default:
                    break;
            }
        }

        private void OpenTowerSelection()
        {
            towerSelectionBox.Visible = true;
            towerSelectionBox.DroppedDown = true;
        }

        private void OpenSoldierSelection()
        {
            soldierSelectionBox.Visible = true;
            soldierSelectionBox.DroppedDown = true;
        }

        protected override void Mouse_Click(object sender, MouseEventArgs e)
        {
            if (cursorState == CursorState.Modified && e.Button == MouseButtons.Left && CanBuyTower())
            {
                BuyTower(towerToBuy, PointToClient(Cursor.Position));
                cursorCommand.Undo();
                //connection.SendAsync("SendMessage", playerType.ToString(), "explotion", new string[] { e.X.ToString(), e.Y.ToString() });
            } else if(cursorState == CursorState.Modified && e.Button == MouseButtons.Right)
            {
                cursorCommand.Undo();
            }
        }

        private bool CanBuyTower()
        {
            switch(playerType)
            {
                case PlayerType.PLAYER1:
                    return PointToClient(Cursor.Position).X < 550;
                case PlayerType.PLAYER2:
                    return PointToClient(Cursor.Position).X > 550;
                default:
                    return false;
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            graphicalTimer.Stop();
            connection.StopAsync();
            base.OnFormClosing(e);
        }

        protected override void tower_selection_click(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedItem != null)
            {
                comboBox.Visible = false;
                cursorCommand.Do(GetTowerType(comboBox.SelectedItem.ToString()));
                towerToBuy = comboBox.SelectedItem.ToString();
            }
        }

        private TowerType GetTowerType(string name)
        {
            switch (name)
            {
                case "Minigun":
                    return TowerType.Minigun;
                case "Laser":
                    return TowerType.Laser;
                case "Rocket":
                    return TowerType.Rocket;
                default:
                    return TowerType.Empty;
            }
        }

        protected override void soldier_selection_click(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedItem != null)
            {
                BuySoldier(comboBox.SelectedItem.ToString());
                comboBox.Visible = false;
            }
        }

        private void BuyTower(string name, Point coordinates)
        {
            switch (name)
            {
                case "Minigun":
                    connection.SendAsync("buyTower", playerType, TowerType.Minigun, coordinates);
                    break;
                case "Laser":
                    connection.SendAsync("buyTower", playerType, TowerType.Laser, coordinates);
                    break;
                case "Rocket":
                    connection.SendAsync("buyTower", playerType, TowerType.Rocket, coordinates);
                    break;
                default:
                    break;
            }
        }

        private void BuySoldier(string name)
        {
            switch (name)
            {
                case "Hitpoints":
                    connection.SendAsync("buySoldier", playerType, SoldierType.HitpointsSoldier);
                    break;
                case "Speed":
                    connection.SendAsync("buySoldier", playerType, SoldierType.SpeedSoldier);
                    break;
                default:
                    break;
            }
        }

        protected override void status_selection_click(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedItem != null)
            {
                switch (comboBox.SelectedItem.ToString())
                {
                    case "All":
                        PlayerStatsShowStatus = PlayerStatsShowStatus.All;
                        LifePointsText.Visible = true;
                        TowerCurrencyText.Visible = true;
                        SoldierCurrencyText.Visible = true;
                        MyConsole.WriteLineWithCount("Adapter: show all");
                        break;
                    case "Lifepoints":                        
                        PlayerStatsShowStatus = PlayerStatsShowStatus.Lifepoints;
                        LifePointsText.Visible = true;
                        TowerCurrencyText.Visible = false;
                        SoldierCurrencyText.Visible = false;
                        MyConsole.WriteLineWithCount("Adapter: show lifepoints");
                        break;
                    case "Tower Currency":
                        PlayerStatsShowStatus = PlayerStatsShowStatus.TowerCurrency;
                        LifePointsText.Visible = false;
                        TowerCurrencyText.Visible = true;
                        SoldierCurrencyText.Visible = false;
                        MyConsole.WriteLineWithCount("Adapter: show tower currency");
                        break;
                    case "Soldier Currency":                        
                        PlayerStatsShowStatus = PlayerStatsShowStatus.SoldierCurrency;
                        LifePointsText.Visible = false;
                        TowerCurrencyText.Visible = false;
                        SoldierCurrencyText.Visible = true;
                        MyConsole.WriteLineWithCount("Adapter: show soldier currency");
                        break;
                }
            }
        }

        public void OnCursorChanged(Cursor cursor, CursorState cursorState)
        {
            Cursor = cursor;
            this.cursorState = cursorState;
        }

        protected override void Mouse_Down(object sender, MouseEventArgs e)
        {
            selectionDrawing.Selection.StartPoint = PointToClient(Cursor.Position);
            selectionDrawing.Selection.Selected = true;
        }

        protected override void Mouse_Up(object sender, MouseEventArgs e)
        {
            selectionDrawing.Selection.Selected = false;
            mapUpdater.SaveSelection(selectionDrawing.Selection);
        }

        protected override void Mouse_Move(object sender, MouseEventArgs e)
        {
            selectionDrawing.Selection.EndPoint = PointToClient(Cursor.Position);
        }
    }
}
