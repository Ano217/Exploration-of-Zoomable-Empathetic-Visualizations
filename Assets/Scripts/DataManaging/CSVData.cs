using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSVData 
{
    public Dictionary<string,int> headers;
    List<DataPoint> dataPoints;
    //List<string[]> datalines;

    public void initializeData(string[] headersList)
    {
        headers = new Dictionary<string, int>();
        for(int i=0; i<headersList.Length; i++)
        {
            headers.Add(headersList[i], i);
        }
        dataPoints = new List<DataPoint>();
    }

    public string[] getHeadersTab()
    {
        string[] tab = new string[headers.Count];
        var i = 0;
        foreach (string key in headers.Keys)
        {
            tab[i] = key;
            i++;
        }
        return tab;
    }

    public int getNbHeaders()
    {
        return headers.Count;
    }

    public int getSize()
    {
        return dataPoints.Count;
    }

    public DataPoint GetPoint(int index){ return (dataPoints[index]); }

    public void addLine(string[] newLine)
    {
        DataPoint d = new DataPoint();
        d.setData(newLine);
        dataPoints.Add(d);
    }

    public DataPoint getLine(int numLine)
    {
        return dataPoints[numLine];
    }

    public string getData(int numLine, string headerName)
    {
        if (headers.ContainsKey(headerName) && dataPoints.Count<numLine)
        {
            return dataPoints[numLine].getData()[headers[headerName]];
        }
        return null;
    }

    public List<DataPoint> getAllData()
    {
        return dataPoints;
    }

    public int getColumnID(string header)
    {
        if (headers.ContainsKey(header)) return headers[header];
        return -1;
    }


    /// <summary>
    /// Method <c>sortBy</c> sort the data by a nominal variable.
    /// It returns a dictionary with the name of the sorting value as key and the list of data line numbers matching this value
    /// </summary>
    public Dictionary<string, List<DataPoint>> sortByNominal(string column)
    {
        Dictionary<string, List<DataPoint>> categories = new Dictionary<string, List<DataPoint>>();
        int colID = -1;
        if (headers.ContainsKey(column))
        {
            colID = headers[column];
        }
        else
        {
            return categories;
        }
        for(int i=0; i<dataPoints.Count; i++)
        {
            DataPoint point = dataPoints[i];
            string[] line = point.getData();
            string cName = line[colID];
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


    public Dictionary<string, List<DataPoint>> sortByNominal(string column, List<DataPoint> unsortedData)
    {
        Dictionary<string, List<DataPoint>> categories = new Dictionary<string, List<DataPoint>>();
        int colID = -1;
        if (headers.ContainsKey(column))
        {
            colID = headers[column];
        }
        else
        {
            return categories;
        }
        for (int i = 0; i < unsortedData.Count; i++)
        {
            DataPoint point = unsortedData[i];
            string[] line = point.getData();
            string cName = line[colID];
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


    // Sort the data into categories defines by a given tab of ranges (each value is the maximum of a category)
    public Dictionary<string, List<DataPoint>> sortByRange(string column, float[] ranges)
    {
        Dictionary<string, List<DataPoint>> categories = new Dictionary<string, List<DataPoint>>();
        List<DataPoint> lastCat = new List<DataPoint>();
        float maxValue = ranges[ranges.Length-1];
        int colID = -1;
        if (headers.ContainsKey(column))
        {
            colID = headers[column];
        }
        else
        {
            return categories;
        }
        foreach (float i in ranges){
            List<DataPoint> linesNums = new List<DataPoint>();
            categories.Add(i.ToString(), linesNums);
        }

        for (int i = 0; i < dataPoints.Count; i++)
        {
            DataPoint point = dataPoints[i];
            string[] line = point.getData();
            string value = line[colID];
            int j = 0;
            if (value == "XX")
            {
                if (categories.ContainsKey("unknown"))
                {
                    categories["unknown"].Add(point);
                }
                else
                {
                    List<DataPoint> unknownList = new List<DataPoint>();
                    unknownList.Add(point);
                    categories.Add("unknown", unknownList);
                }
            }
            else
            {
                while (j < ranges.Length && float.Parse(value) > ranges[j]) j += 1;
                if (j < ranges.Length)
                {
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
        return categories;
    }

    public Dictionary<string, List<DataPoint>> sortByRange(string column, float[] ranges, List<DataPoint> unsortedData)
    {
        Dictionary<string, List<DataPoint>> categories = new Dictionary<string, List<DataPoint>>();
        List<DataPoint> lastCat = new List<DataPoint>();
        float maxValue = ranges[ranges.Length - 1];
        int colID = -1;
        if (headers.ContainsKey(column))
        {
            colID = headers[column];
        }
        else
        {
            return categories;
        }
        foreach (float i in ranges)
        {
            List<DataPoint> linesNums = new List<DataPoint>();
            if (!categories.ContainsKey(i.ToString())) categories.Add(i.ToString(), linesNums);
        }

        // Add Maximum Category
        string maxName = "maxCategory";
        List<DataPoint> upValueList = new List<DataPoint>();
        categories.Add(maxName, upValueList);

        // Add unknown category
        List<DataPoint> unknownList = new List<DataPoint>();
        categories.Add("unknown", unknownList);

        for (int i = 0; i < unsortedData.Count; i++)
        {
            DataPoint point = unsortedData[i];

            string[] line = point.getData();
            string value = line[colID];
            int j = 0;
            try
            {
                while (j < ranges.Length && float.Parse(value) > ranges[j]) j += 1;
                if (j < ranges.Length)
                {
                    categories[ranges[j].ToString()].Add(point);
                }
                else
                {
                    categories[maxName].Add(point);
                }
            }
            catch
            {
                if (categories.ContainsKey("unknown"))
                {
                    categories["unknown"].Add(point);
                }
            }
        }
        return categories;
    }


    /// <summary>
    ///  sort by time range 
    /// </summary>
    public Dictionary<string,List<DataPoint>> sortByTimeRange(string column, TimeRange timeType)
    {
        // Accepted Formats: mm/yy OR mm/yyyy OR dd/mm//yy OR dd/mm//yyyy
        int colID = headers[column];
        string format = dataPoints[0].getData()[colID];
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

        foreach(DataPoint point in dataPoints)
        { // For each data point
            string date = point.getData()[colID];
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

    public Dictionary<string, List<DataPoint>> sortByTimeRange(string column, TimeRange timeType, List<DataPoint> unsortedData)
    {
        // Accepted Formats: mm/yy OR mm/yyyy OR dd/mm//yy OR dd/mm//yyyy
        int colID = headers[column];
        string format = unsortedData[0].data[colID];
        string[] dividedDate = format.Split('/');

        int catID = 0;
        if (dividedDate.Length > 2)
        {
            if (timeType == TimeRange.Month)
            {
                catID = 1;
            }
            else
            {
                catID = 2;
            }
        }
        else
        {
            if (timeType == TimeRange.Year)
            {
                catID = 1;
            }
        }
        Dictionary<string, List<DataPoint>> categories = new Dictionary<string, List<DataPoint>>();

        foreach (DataPoint point in unsortedData)
        { // For each data point
            string date = point.data[colID];
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
