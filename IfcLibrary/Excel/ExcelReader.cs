using ExcelDataReader;
using IfcLibrary.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace IfcLibrary.Excel
{
    public class ExcelReader : IExcelReader
    {
        public List<List<string>> GetCells(string path)
        {
            var results = new List<List<string>>();
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();

                    var firstSheet = result.Tables[0];

                    foreach (DataRow row in firstSheet.Rows)
                    {
                        var rowAsList = new List<string>();
                        foreach (DataColumn column in firstSheet.Columns)
                        {
                            rowAsList.Add(row[column]?.ToString() ?? string.Empty);
                        }
                        results.Add(rowAsList);
                    }
                }
            }
            return results;
        }

        public AutomatedChanges GetAutomatedChanges(string path)
        {
            return null;
        }
    }
}
