using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

// source: https://www.youtube.com/watch?v=xwnL4meq-j8

public static class CSVReader
{

    public static CSVData readCVSFile(string path)
    {
        StreamReader strReader = new StreamReader(path, Encoding.GetEncoding("iso-8859-1"));//System.Text.Encoding.UTF8);
        CSVData myData = new CSVData();
        bool endOfFile = false;
        string headers = strReader.ReadLine();
        var headersNames = headers.Split(';');

        myData.initializeData(headersNames);
        while (!endOfFile)
        {
            string data_String = strReader.ReadLine();
            if (data_String == null)
            {
                endOfFile = true;
                break;
            }
            var data_values = data_String.Split(';');
            myData.addLine(data_values);
        }
        return myData;
    }

}
