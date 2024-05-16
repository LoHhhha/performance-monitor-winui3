using LiteDB;
using performance_monitor_winui3.Models;

namespace performance_monitor_winui3.Dao;

// keeping ToDoListItem/FinishListItem
// https://zhuanlan.zhihu.com/p/677026839
public static class ToDoListDao
{
    private static readonly string DataPath = "ToDoPageItemsData.db";

    /* Begin: ToDoListItems */
    private static readonly string ToDoListItemsTable = "ToDoListItems";
    public static List<ToDoListItem> GetAllToDoListItemData()
    {
        using var db = new LiteDatabase(DataPath);
        var collection = db.GetCollection<ToDoListItem>(ToDoListItemsTable);
        var result = collection.FindAll();
        return result.ToList<ToDoListItem>();
    }

    public static void ToDoListItemsAdd(ToDoListItem item)
    {
        using var db = new LiteDatabase(DataPath);
        var collection = db.GetCollection<ToDoListItem>(ToDoListItemsTable);
        collection.Insert(item);
    }

    public static void ToDoListItemsDelete(ToDoListItem item)
    {
        using var db = new LiteDatabase(DataPath);
        var collection = db.GetCollection<ToDoListItem>(ToDoListItemsTable);
        collection.Delete(item.Id);
    }

    public static void ToDoListItemsUpdate(ToDoListItem item)
    {
        using var db = new LiteDatabase(DataPath);
        var collection = db.GetCollection<ToDoListItem>(ToDoListItemsTable);
        collection.Update(item);
    }
    /* End: ToDoListItems */

    /* Begin: FinishListItems */
    private static readonly string FinishListItemsTable = "FinishListItems";
    public static List<FinishListItem> GetAllFinishListItemData()
    {
        using var db = new LiteDatabase(DataPath);
        var collection = db.GetCollection<FinishListItem>(FinishListItemsTable);
        var result = collection.FindAll();
        return result.ToList<FinishListItem>();
    }

    public static void FinishListItemsAdd(FinishListItem item)
    {
        using var db = new LiteDatabase(DataPath);
        var collection = db.GetCollection<FinishListItem>(FinishListItemsTable);
        collection.Insert(item);
    }

    public static void FinishListItemsDelete(FinishListItem item)
    {
        using var db = new LiteDatabase(DataPath);
        var collection = db.GetCollection<FinishListItem>(FinishListItemsTable);
        collection.Delete(item.Id);
    }

    public static void FinishListItemsUpdate(FinishListItem item)
    {
        using var db = new LiteDatabase(DataPath);
        var collection = db.GetCollection<FinishListItem>(FinishListItemsTable);
        collection.Update(item);
    }
    /* End: FinishListItems */
}