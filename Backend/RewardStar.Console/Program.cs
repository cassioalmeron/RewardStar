//// See https://aka.ms/new-console-template for more information

//using Microsoft.EntityFrameworkCore;
//using RewardStart.Core;

var folder = Environment.SpecialFolder.LocalApplicationData;
var path = Environment.GetFolderPath(folder);
var DBPath = Path.Combine(path, "RewardStar.db");

Console.WriteLine(DBPath);
Console.ReadKey();

//using var context = new RewardStartDbContext();




////await context.Database.migr