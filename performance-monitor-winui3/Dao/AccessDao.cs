using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using performance_monitor_winui3.Models;

namespace performance_monitor_winui3.Dao;

public static class AccessDao
{
    private static readonly string DataPath = "AccessPageItemsData.db";

    /* Begin: AccessItems */
    private static readonly string AccessItemsTable = "AccessItems";
    public static List<AccessItem> GetAllAccessItemData()
    {
        using var db = new LiteDatabase(DataPath);
        var collection = db.GetCollection<AccessItem>(AccessItemsTable);
        var result = collection.FindAll();
        return result.ToList<AccessItem>();
    }

    public static void AccessItemsAdd(AccessItem item)
    {
        using var db = new LiteDatabase(DataPath);
        var collection = db.GetCollection<AccessItem>(AccessItemsTable);
        collection.Insert(item);
    }

    public static void AccessItemsDelete(AccessItem item)
    {
        using var db = new LiteDatabase(DataPath);
        var collection = db.GetCollection<AccessItem>(AccessItemsTable);
        collection.Delete(item.Id);
    }

    public static void AccessItemsUpdate(AccessItem item)
    {
        using var db = new LiteDatabase(DataPath);
        var collection = db.GetCollection<AccessItem>(AccessItemsTable);
        collection.Update(item);
    }
    /* End: AccessItems */
}
