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

                    // Only one sheet supported
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
            var result = new AutomatedChanges();

            try
            {
                var cells = GetCells(path);
                result.UpdatePropertySetByValues.AddRange(ParseUpdatePropertySetByValue(cells));
                result.AddPropertySetWithPropertyAndValues.AddRange(ParseAddPropertySetByValue(cells));
                result.AddPropertySetWithRelativePropertyAndValues.AddRange(ParseAddPropertySetByRelativeValue(cells));
            }
            catch (Exception e)
            {
                throw new Exception("Could not read excel file", e);
            }

            return result;
        }

        private IEnumerable<AddPropertySetWithRelativePropertyAndValue> ParseAddPropertySetByRelativeValue(List<List<string>> cells)
        {
            var addNewPropertySetNameHeader = FindHeaderFor("Property set Nuevo ", cells);
            var addNewPropertyNameHeader = new CellIndex
            {
                Row = addNewPropertySetNameHeader.Row,
                Column = addNewPropertySetNameHeader.Column + 1,
            };
            var addRelativePropertySetNameHeader = new CellIndex
            {
                Row = addNewPropertyNameHeader.Row,
                Column = addNewPropertyNameHeader.Column + 1,
            };
            var addRelativePropertyNameHeader = new CellIndex
            {
                Row = addRelativePropertySetNameHeader.Row,
                Column = addRelativePropertySetNameHeader.Column + 1,
            };

            var result = new List<AddPropertySetWithRelativePropertyAndValue>();
            for (int row = addNewPropertySetNameHeader.Row + 1; row < cells.Count; row++)
            {
                var copyFromPropertyName = cells[row][addRelativePropertyNameHeader.Column];
                var newPropertyName = cells[row][addNewPropertyNameHeader.Column];
                var newPropertySetName = cells[row][addNewPropertySetNameHeader.Column];
                var copyFromPropertySetName = cells[row][addRelativePropertySetNameHeader.Column];

                if (!string.IsNullOrEmpty(copyFromPropertyName)
                    && !string.IsNullOrEmpty(newPropertyName)
                    && !string.IsNullOrEmpty(newPropertySetName))
                {
                    result.Add(new AddPropertySetWithRelativePropertyAndValue
                    {
                        CopyFromPropertyName = copyFromPropertyName,
                        NewPropertyName = newPropertyName,
                        NewPropertySetName = newPropertySetName,
                        CopyFromPropertySetName = copyFromPropertySetName,
                    });
                }
            }

            return result;
        }

        private IEnumerable<AddPropertySetWithPropertyAndValue> ParseAddPropertySetByValue(List<List<string>> cells)
        {
            var addNewPropertySetNameHeader = FindHeaderFor("Property set Nuevo ", cells);
            var addNewPropertyNameHeader = new CellIndex
            {
                Row = addNewPropertySetNameHeader.Row,
                Column = addNewPropertySetNameHeader.Column + 1,
            };
            var addNewValueHeader = new CellIndex
            {
                Row = addNewPropertyNameHeader.Row,
                Column = addNewPropertyNameHeader.Column + 3,
            };

            var result = new List<AddPropertySetWithPropertyAndValue>();
            for (int row = addNewPropertySetNameHeader.Row + 1; row < cells.Count; row++)
            {
                var newValue = cells[row][addNewValueHeader.Column];
                var newPropertyName = cells[row][addNewPropertyNameHeader.Column];
                var newPropertySetName = cells[row][addNewPropertySetNameHeader.Column];

                if (!string.IsNullOrEmpty(newValue)
                    && !string.IsNullOrEmpty(newPropertyName)
                    && !string.IsNullOrEmpty(newPropertySetName))
                {
                    result.Add(new AddPropertySetWithPropertyAndValue
                    {
                        NewValue = newValue,
                        NewPropertyName = newPropertyName,
                        NewPropertySetName = newPropertySetName,
                    });
                }
            }

            return result;
        }

        private List<UpdatePropertySetByValue> ParseUpdatePropertySetByValue(List<List<string>> cells)
        {
            var updatePropertySetHeader = FindHeaderFor("Property set a cambiar", cells);
            var updateNewPropertyNameHeader = new CellIndex
            {
                Row = updatePropertySetHeader.Row,
                Column = updatePropertySetHeader.Column + 1,
            };
            var updateNewPropertyValueHeader = new CellIndex
            {
                Row = updateNewPropertyNameHeader.Row,
                Column = updateNewPropertyNameHeader.Column + 1,
            };

            var result = new List<UpdatePropertySetByValue>();
            for (int row = updatePropertySetHeader.Row + 1; row < cells.Count; row++)
            {
                var newValue = cells[row][updateNewPropertyValueHeader.Column];
                var propertyName = cells[row][updateNewPropertyNameHeader.Column];
                var propertySetName = cells[row][updatePropertySetHeader.Column];

                if (!string.IsNullOrEmpty(newValue)
                    && !string.IsNullOrEmpty(propertyName)
                    && !string.IsNullOrEmpty(propertySetName))
                {
                    result.Add(new UpdatePropertySetByValue
                    {
                        NewValue = newValue,
                        PropertyName = propertyName,
                        PropertySetName = propertySetName,
                    });
                }
            }

            return result;
        }

        private CellIndex FindHeaderFor(string headerName, List<List<string>> cells)
        {
            for (int row = 0; row < cells.Count; row++)
            {
                for (int column = 0; column < cells[row].Count; column++)
                {
                    var currentCell = cells[row][column];
                    if (currentCell == headerName)
                    {
                        return new CellIndex
                        {
                            Column = column,
                            Row = row,
                        };
                    }
                }
            }

            return null;
        }

        private List<CellIndex> FindTables(List<List<string>> cells)
        {
            var result = new List<CellIndex>();
            for (int row = 0; row < cells.Count; row++)
            {
                for (int column = 0; column < cells[row].Count; column++)
                {
                    var currentCell = cells[row][column];
                    var isStartOfTable = IsCellStartOfTable(cells, currentCell, row, column);
                    if (isStartOfTable)
                    {
                        result.Add(new CellIndex
                        {
                            Column = column,
                            Row = row,
                        });
                    }
                }
            }

            return result;
        }

        private static bool IsCellStartOfTable(List<List<string>> cells, string currentCell, int row, int column)
        {
            if (string.IsNullOrWhiteSpace(currentCell))
            {
                return false;
            }

            return true;
        }
    }
}
