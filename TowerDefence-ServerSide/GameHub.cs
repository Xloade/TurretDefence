﻿using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
namespace TowerDefence_ServerSide
{
    public class GameHub : Hub
    {
        public async Task SendMessage(string user, string function, string[] args)
        {
            string argString ="";
            foreach(string arg in args)
            {
                argString += arg + ", ";
            }
            if(argString != "")
            {
                argString = argString.Substring(0, argString.Length - 2);
            }
            Console.WriteLine($"{user}: {function}({argString})");
            await Clients.All.SendAsync("ReceiveMessage", user, function, args);
        }
    }
}
