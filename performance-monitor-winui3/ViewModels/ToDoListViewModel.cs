using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using performance_monitor_winui3.Dao;
using performance_monitor_winui3.Models;

namespace performance_monitor_winui3.ViewModels;

public partial class ToDoListViewModel : ObservableRecipient
{
    /* begin: ToDoList */
    // using ObservableCollection can auto redraw the windows
    private static ObservableCollection<ToDoListItem> _ToDoListItems;
    public ObservableCollection<ToDoListItem> ToDoListItems
    {
        get => _ToDoListItems;
        set => _ToDoListItems = value;
    }

    static ToDoListViewModel()
    {
        _ToDoListItems = new ObservableCollection<ToDoListItem>(ToDoListDao.GetAllToDoListItemData());
        _FinishListItems = new ObservableCollection<FinishListItem>(ToDoListDao.GetAllFinishListItemData());
    }

    public static void ToDoListAddItem(ToDoListItem item)
    {
        ToDoListDao.ToDoListItemsAdd(item);
        _ToDoListItems.Add(item);
    }

    public static void ToDoListDeleteItem(ToDoListItem item)
    {
        ToDoListDao.ToDoListItemsDelete(item);
        _ToDoListItems.Remove(item);
    }

    public static void ToDoListReplaceItem(ToDoListItem oldItem,ToDoListItem newItem)
    {
        try
        {
            newItem.Id = oldItem.Id;
            ToDoListDao.ToDoListItemsUpdate(newItem);
            _ToDoListItems[_ToDoListItems.IndexOf(oldItem)] = newItem;
        }
        catch { }
    }
    /* end: ToDoList */

    /* begin: FinishList */
    // using ObservableCollection can auto redraw the windows
    private static ObservableCollection<FinishListItem> _FinishListItems;
    public ObservableCollection<FinishListItem> FinishListItems
    {
        get => _FinishListItems;
        set => _FinishListItems = value;
    }

    public static void FinishListAddItem(ToDoListItem task)
    {
        var newItem = new FinishListItem(task);
        ToDoListDao.FinishListItemsAdd(newItem);
        _FinishListItems.Add(newItem);
    }

    public static void FinishListDeleteItem(FinishListItem item)
    {
        ToDoListDao.FinishListItemsDelete(item);
        _FinishListItems.Remove(item);
    }
    /* end: FinishList */
}
