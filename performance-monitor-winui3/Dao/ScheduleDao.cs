using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using performance_monitor_winui3.Models;

namespace performance_monitor_winui3.Dao;

public static class  ScheduleDao
{
    private static readonly string DataPath = "ScheduleItemsData.db";

    /* Begin: ScheduleItems */
    private static readonly string ScheduleItemsTable = "ScheduleItems";
    public static List<ScheduleItem> GetAllScheduleItemData()
    {
        using var db = new LiteDatabase(DataPath);
        var collection = db.GetCollection<ScheduleItem>(ScheduleItemsTable);
        var result = collection.FindAll().OrderBy(x => x.BeginTime);
        return result.ToList<ScheduleItem>();
    }

    public static List<ScheduleItem> GetScheduleItemDataFromDayOfWeek(Models.DayOfWeek dayOfWeek)
    {
        using var db = new LiteDatabase(DataPath);
        var collection = db.GetCollection<ScheduleItem>(ScheduleItemsTable);
        var result = collection.Find(x => x.Day == dayOfWeek || x.Day == Models.DayOfWeek.All).OrderBy(x => x.BeginTime);
        return result.ToList<ScheduleItem>();
    }

    public static void ScheduleItemsAdd(ScheduleItem item)
    {
        using var db = new LiteDatabase(DataPath);
        var collection = db.GetCollection<ScheduleItem>(ScheduleItemsTable);
        collection.Insert(item);
    }

    public static void ScheduleItemsDelete(ScheduleItem item)
    {
        using var db = new LiteDatabase(DataPath);
        var collection = db.GetCollection<ScheduleItem>(ScheduleItemsTable);
        collection.Delete(item.Id);
    }

    public static void ScheduleItemsUpdate(ScheduleItem item)
    {
        using var db = new LiteDatabase(DataPath);
        var collection = db.GetCollection<ScheduleItem>(ScheduleItemsTable);
        collection.Update(item);
    }
    /* End: ScheduleItems */
}
