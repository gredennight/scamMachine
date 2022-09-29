// See https://aka.ms/new-console-template for more information
using Google.Apis.Drive.v3.Data;

Console.WriteLine("username: {0}",Environment.UserName);
GayRadarSystems App = new GayRadarSystems();
App.SetMaxFileSize(1000000);
await App.LessGo();
//await App.ForcedLessGo();