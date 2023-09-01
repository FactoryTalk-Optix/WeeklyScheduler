#region Using directives
using System;
using FTOptix.Core;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.OPCUAServer;
using FTOptix.HMIProject;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.CoreBase;
using FTOptix.NetLogic;
using FTOptix.Store;
using FTOptix.DataLogger;
#endregion

public class SchedulerLogic : BaseNetLogic
{
    private PeriodicTask periodicTask;
    private Store SchedulerDB;
    private IUAVariable forceCheck;
    private int lastMinute = 0;

    public override void Start()
    {
        lastMinute = DateTime.Now.Minute;
        forceCheck = LogicObject.GetVariable("ForceCheck");
        forceCheck.VariableChange += ForceCheck_VariableChange;
        SchedulerDB = (Store)Owner;// InformationModel.Get<Store>((NodeId)LogicObject.GetVariable("SchedulerDB").Value);
        periodicTask = new PeriodicTask(CheckChangedMinute, 1000, LogicObject);
        periodicTask.Start();
        CheckScheduler();
    }

    private void ForceCheck_VariableChange(object sender, VariableChangeEventArgs e)
    {
        if (forceCheck.Value)
        {
            CheckScheduler();
            forceCheck.Value = false;
        }
    }

    public override void Stop()
    {
        if (periodicTask != null)
        {
            periodicTask.Dispose();
            periodicTask = null;
        }
    }

    private void CheckChangedMinute()
    {
        if (DateTime.Now.Minute != lastMinute)
        {
            Log.Info("Checking Scheduler On-Off");
            CheckScheduler();
            lastMinute = DateTime.Now.Minute;
        }
    }

    private void CheckScheduler()
    {
        int minNow = DateTime.Now.Minute + 60 * DateTime.Now.Hour;
        bool OffOn;
        OffOn = CheckDay((int)DateTime.Now.DayOfWeek);
        LogicObject.GetVariable("Active").Value = OffOn;
    }
    
    private bool CheckDay(int day)
    {
        int minNow = DateTime.Now.Minute + 60 * DateTime.Now.Hour;
        int quarter = minNow / 15;
        object[,] resultSet;
        string[] header;
        string dayQuarter;
        SchedulerDB.Query("SELECT Day" + day.ToString() + " FROM Scheduler", out header, out resultSet);
        if (resultSet.GetLength(0) != 0)
        {
            dayQuarter = resultSet[0, 0].ToString();
            if (dayQuarter[quarter] == '0')
                return false;
            else
                return true;
        }
        else
            return false;
    }
}
