using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using performance_monitor_winui3.Dao;
using performance_monitor_winui3.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace performance_monitor_winui3.ViewModels;


public partial class AccessViewModel : ObservableRecipient
{
    public static string AccessHandlerName = "cmd.exe";

    /* Begin: ErrorMessages */
    private static ObservableCollection<AccessRunError> _ErrorMessages;
    public ObservableCollection<AccessRunError> ErrorMessages
    {
        get => _ErrorMessages;
        set => _ErrorMessages = value;
    }

    public static void ErrorMessagesAdd(AccessRunError error)
    {
        _ErrorMessages.Add(error);
    }

    public static void ErrorMessagesDelete(AccessRunError error)
    {
        _ErrorMessages.Remove(error);
    }
    /* End: ErrorMessages */

    /* Begin: AccessItems */
    private static ObservableCollection<AccessItem> _AccessItems;
    public ObservableCollection<AccessItem> AccessItems
    {
        get => _AccessItems;
        set => _AccessItems = value;
    }

    public static void AccessItemsAdd(AccessItem accessItem)
    {
        AccessDao.AccessItemsAdd(accessItem);
        _AccessItems.Add(accessItem);
    }

    public static void AccessItemsDelete(AccessItem accessItem)
    {
        AccessDao.AccessItemsDelete(accessItem);
        _AccessItems.Remove(accessItem);
    }

    public static void AccessItemsReplace(AccessItem oldItem,AccessItem newItem)
    {
        try
        {
            newItem.Id = oldItem.Id;
            AccessDao.AccessItemsUpdate(newItem);
            _AccessItems[_AccessItems.IndexOf(oldItem)] = newItem;
        }
        catch { }
    }
    /* End: AccessItems */


    static AccessViewModel()
    {
        _ErrorMessages = new ObservableCollection<AccessRunError>();
        try
        {
            _AccessItems = new ObservableCollection<AccessItem>(AccessDao.GetAllAccessItemData());
        }
        catch(Exception ex)
        {
            _AccessItems = new ObservableCollection<AccessItem>();
            ErrorMessagesAdd(new AccessRunError("DataBase Error!", ex.Message));
        }
    }

    public AccessViewModel() { }

    public static async void Run(AccessItem accessItem)
    {
        var error = await Task.Run(() => Handle(accessItem.Command, accessItem.ShowWindow, accessItem.ExitAfterFinish));
        if (error!="")
        {
            ErrorMessagesAdd(new AccessRunError(accessItem.TaskName, error));
        }
    }

    private static string Handle(string command, bool showWindow, bool exitAfterFinish)
    {
        try
        {
            Process p = new Process();
            p.StartInfo.FileName = AccessHandlerName;

            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = !showWindow;

            p.Start();

            p.StandardInput.WriteLine(command);
            if(exitAfterFinish)
            {
                p.StandardInput.WriteLine("exit");
            }

            p.WaitForExit();
            var error = p.StandardError.ReadToEnd();
            p.Close();

            if(error != "") 
            {
                return error.Trim();
            }
        }catch(Exception ex)
        {
            return ex.Message.Trim();
        }
        return "";
    }
}
