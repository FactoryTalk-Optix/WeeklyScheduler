#region Using directives
using System;
using FTOptix.Core;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.OPCUAServer;
using FTOptix.CoreBase;
using FTOptix.SQLiteStore;
using FTOptix.UI;
using FTOptix.Store;
using FTOptix.NetLogic;
using FTOptix.HMIProject;
using FTOptix.NativeUI;
using FTOptix.DataLogger;
#endregion

public class SchedulerPanelLogic : BaseNetLogic
{
    private Store SchedulerDB;
    public override void Start()
    {
        SchedulerDB = InformationModel.Get<Store>((NodeId)LogicObject.GetVariable("SchedulerDB").Value);
        LoadWeek();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    private void LoadWeek()
    {
        object[,] resultSet;
        string[] header;
        SchedulerDB.Query("SELECT Day0, Day1, Day2, Day3, Day4, Day5, Day6 FROM Scheduler", out header, out resultSet);
        if (resultSet.GetLength(0) != 0)
        {
            ColumnLayout dayVL = Owner.Get<ColumnLayout>("DayVerticalLayout");
            foreach (var item in dayVL.Children)
            {
                switch ((int)item.GetVariable("DayOfWeek").Value)
                {
                    case 0:
                        LoadDay((RowLayout)item, resultSet[0, 0].ToString());
                        break;
                    case 1:
                        LoadDay((RowLayout)item, resultSet[0, 1].ToString());
                        break;
                    case 2:
                        LoadDay((RowLayout)item, resultSet[0, 2].ToString());
                        break;
                    case 3:
                        LoadDay((RowLayout)item, resultSet[0, 3].ToString());
                        break;
                    case 4:
                        LoadDay((RowLayout)item, resultSet[0, 4].ToString());
                        break;
                    case 5:
                        LoadDay((RowLayout)item, resultSet[0, 5].ToString());
                        break;
                    case 6:
                        LoadDay((RowLayout)item, resultSet[0, 6].ToString());
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void LoadDay(RowLayout dayRow, string day)
    {
        RowLayout quarter = dayRow.Get<RowLayout>("QuarterHorizontalLayout");
        int counter = 0;
        foreach (var item in quarter.Children)
        {
            item.GetVariable("Active").Value = Int16.Parse(day[counter].ToString());
            counter++;
        }
    }

    [ExportMethod]
    public void UpdateWeek()
    {
        string Day0 = "";
        string Day1 = "";
        string Day2 = "";
        string Day3 = "";
        string Day4 = "";
        string Day5 = "";
        string Day6 = "";
        ColumnLayout dayVL = Owner.Get<ColumnLayout>("DayVerticalLayout");
        foreach (var item in dayVL.Children)
        {
            switch ((int)item.GetVariable("DayOfWeek").Value)
            {
                case 0:
                    Day0 = DayBuilder((RowLayout)item);
                    break;
                case 1:
                    Day1 = DayBuilder((RowLayout)item);
                    break;
                case 2:
                    Day2 = DayBuilder((RowLayout)item);
                    break;
                case 3:
                    Day3 = DayBuilder((RowLayout)item);
                    break;
                case 4:
                    Day4 = DayBuilder((RowLayout)item);
                    break;
                case 5:
                    Day5 = DayBuilder((RowLayout)item);
                    break;
                case 6:
                    Day6 = DayBuilder((RowLayout)item);
                    break;
                default:
                    break;
            }
        }

        object[,] resultSet;
        string[] header;
        SchedulerDB.Query("SELECT Day0 FROM Scheduler", out header, out resultSet);
        if (resultSet.GetLength(0) != 0)
        {
            SchedulerDB.Query("UPDATE Scheduler SET Day0 = '" + Day0 + "' , Day1 = '" + Day1 + "' , Day2 = '" + Day2 + "' , Day3 = '" + Day3 + "' , Day4 = '" + Day4 + "' , Day5 = '" + Day5 + "' , Day6 = '" + Day6 + "'", out header, out resultSet);
        }
        else
        {
            var tbl = SchedulerDB.Tables.Get<Table>("Scheduler");
            object[,] rawValues = new object[1, 7];
            rawValues[0, 0] = Day0;
            rawValues[0, 1] = Day1;
            rawValues[0, 2] = Day2;
            rawValues[0, 3] = Day3;
            rawValues[0, 4] = Day4;
            rawValues[0, 5] = Day5;
            rawValues[0, 6] = Day6;
            string[] columns = new string[7] { "Day0", "Day1", "Day2", "Day3", "Day4", "Day5", "Day6" };
            tbl.Insert(columns, rawValues);

        }

        SchedulerDB.Get("SchedulerLogic").GetVariable("ForceCheck").Value = true;
    }

    private string DayBuilder(RowLayout dayRow)
    {
        string day = "";
        RowLayout quarter = dayRow.Get<RowLayout>("QuarterHorizontalLayout");
        foreach (var item in quarter.Children)
        {
            if (item.GetVariable("Active").Value)
                day = day + "1";
            else
                day = day + "0";
        }
        return day;
    }
}
