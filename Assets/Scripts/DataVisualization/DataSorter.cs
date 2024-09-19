using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataSorter
{
    /// <summary>
    /// Method <c>sortBy</c> sort the data by a nominal variable.
    /// It returns a dictionary with the name of the sorting value as key and the list of data line numbers matching this value
    /// </summary>
    public static Dictionary<string, List<DataPoint>> sortByNominal(List<DataPoint> data, int columnID)
    {
        Dictionary<string, List<DataPoint>> categories = new Dictionary<string, List<DataPoint>>();
        for (int i = 0; i < data.Count; i++)
        {
            DataPoint point = data[i];
            string[] line = point.getData();
            string cName = line[columnID];
            if (categories.ContainsKey(cName))
            {
                categories[cName].Add(point);
            }
            else
            {
                List<DataPoint> linesNums = new List<DataPoint>();
                linesNums.Add(point);
                categories.Add(cName, linesNums);
            }
        }
        return categories;
    }

    public static void echanger(string[] t, int i, int j)
    {
        string tmp = t[i];
        t[i] = t[j];
        t[j] = tmp;
    }



    public static SortedData sortByOrderedRange(List<DataPoint> data, int columnID, float[] ranges)
    {
        Dictionary<string, List<DataPoint>> categorizedData = sortByRange(data, columnID, ranges);
        string[] categories = new string[categorizedData.Count];
        int i = 0;
        string tab = "";
        foreach(string key in categorizedData.Keys)
        {
            categories[i] = key;
            tab += key + " ,";
            i++;
        }
        SortedData result = new SortedData();
        result.categories = categorizedData;
        result.categoriesOrder = categories;
        return result;
    }

    // Sort the data into categories defines by a given tab of ranges (each value is the maximum of a category)
    public static Dictionary<string, List<DataPoint>> sortByRange(List<DataPoint> data, int columnID, float[] ranges)
    {
        Dictionary<string, List<DataPoint>> categories = new Dictionary<string, List<DataPoint>>();
        List<DataPoint> lastCat = new List<DataPoint>();
        float maxValue = ranges[ranges.Length - 1];

        foreach (float i in ranges)
        {
            List<DataPoint> linesNums = new List<DataPoint>();
            categories.Add(i.ToString(), linesNums);
        }
        List<DataPoint> unknownList = new List<DataPoint>();
        for (int i = 0; i < data.Count; i++)
        {
            DataPoint point = data[i];
            string[] line = point.getData();
            string value = line[columnID];
            int j = 0;
            if (value == "XX")
            {
                unknownList.Add(point);
            }
            else
            {
                while (j < ranges.Length && float.Parse(value) > ranges[j]) j += 1;
                if (j < ranges.Length)
                {
                    //Debug.Log("Add " + value + " to " + ranges[j].ToString());
                    categories[ranges[j].ToString()].Add(point);
                }
                else
                {
                    float fValue = float.Parse(value);
                    if (fValue > maxValue) maxValue = fValue;
                    lastCat.Add(point);
                }
            }

        }
        if (lastCat.Count > 0 && !categories.ContainsKey(maxValue.ToString()))
        {
            categories.Add(maxValue.ToString(), lastCat);
        }
        if(unknownList.Count > 0)
        {
            categories.Add("unknown", unknownList);
        }
        return categories;
    }


    // Sort data by month/year
    public static Dictionary<string, List<DataPoint>> sortByTimeRange(List<DataPoint> data, int columnID, TimeRange timeType)
    {
        // Accepted Formats: mm/yy OR mm/yyyy OR dd/mm//yy OR dd/mm//yyyy
        string format = data[0].getData()[columnID];
        string[] dividedDate = format.Split('/');

        int catID = 0;
        if (dividedDate.Length > 2)
        {
            if (timeType == TimeRange.Month) catID = 1;
            else catID = 2;
        }
        else
        {
            if (timeType == TimeRange.Year) catID = 1;
        }
        Dictionary<string, List<DataPoint>> categories = new Dictionary<string, List<DataPoint>>();

        foreach (DataPoint point in data)
        { // For each data point
            string date = point.getData()[columnID];
            string[] dates = date.Split('/');
            if (timeType == TimeRange.Year)
            { // If we sort by year
                if (categories.ContainsKey(dates[catID])) categories[dates[catID]].Add(point);
                else
                {
                    List<DataPoint> newList = new List<DataPoint>();
                    newList.Add(point);
                    categories.Add(dates[catID], newList);
                }
            }
            else
            { // If we sort by month => need to consider mm/yy as two data points that are the same month but two different years need to be in different categories
                string mmyy = dates[catID] + "/" + dates[catID + 1];
                if (categories.ContainsKey(mmyy)) categories[mmyy].Add(point);
                else
                {
                    List<DataPoint> newList = new List<DataPoint>();
                    newList.Add(point);
                    categories.Add(mmyy, newList);
                }
            }
        }
        return categories;
    }
}
