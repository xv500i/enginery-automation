using ExcelDataReader;
using IfcLibrary.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace IfcLibrary.Excel
{
    public class ExcelReader : IExcelReader
    {
        private const string AddPropertySetHeaderValue = "Property set Nuevo ";
        private const string UpdatePropertySetHeaderValue = "Property set a cambiar";

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

        private List<AddPropertySetWithRelativePropertyAndValue> ParseAddPropertySetByRelativeValue(List<List<string>> cells)
        {
            var addNewPropertySetNameHeader = FindHeaderFor(AddPropertySetHeaderValue, cells);
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

            var headers = new List<CellIndex>
            {
                addNewPropertySetNameHeader,
                addNewPropertyNameHeader,
                addRelativePropertySetNameHeader,
                addRelativePropertyNameHeader,
            };
            return ParseRows(cells, headers, (dictionary) =>
                new AddPropertySetWithRelativePropertyAndValue
                {
                    CopyFromPropertyName = dictionary[addRelativePropertyNameHeader],
                    NewPropertyName = dictionary[addNewPropertyNameHeader],
                    NewPropertySetName = dictionary[addNewPropertySetNameHeader],
                    CopyFromPropertySetName = dictionary[addRelativePropertySetNameHeader],
                }
            );
        }

        private List<AddPropertySetWithPropertyAndValue> ParseAddPropertySetByValue(List<List<string>> cells)
        {
            var addNewPropertySetNameHeader = FindHeaderFor(AddPropertySetHeaderValue, cells);
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

            var headers = new List<CellIndex>
            {
                addNewPropertySetNameHeader,
                addNewPropertyNameHeader,
                addNewValueHeader,
            };
            return ParseRows(cells, headers, (dictionary) =>
                new AddPropertySetWithPropertyAndValue
                {
                    NewValue = dictionary[addNewValueHeader],
                    NewPropertyName = dictionary[addNewPropertyNameHeader],
                    NewPropertySetName = dictionary[addNewPropertySetNameHeader],
                }
            );
        }

        private List<UpdatePropertySetByValue> ParseUpdatePropertySetByValue(List<List<string>> cells)
        {
            var updatePropertySetHeader = FindHeaderFor(UpdatePropertySetHeaderValue, cells);
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

            var headers = new List<CellIndex>
            {
                updatePropertySetHeader,
                updateNewPropertyNameHeader,
                updateNewPropertyValueHeader,
            };
            return ParseRows(cells, headers, (dictionary) =>
                new UpdatePropertySetByValue
                {
                    NewValue = dictionary[updateNewPropertyValueHeader],
                    PropertyName = dictionary[updateNewPropertyNameHeader],
                    PropertySetName = dictionary[updatePropertySetHeader],
                }
            );
        }

        private List<T> ParseRows<T>(List<List<string>> cells, List<CellIndex> headers, Func<Dictionary<CellIndex, string>, T> buildDomainObjectFunc)
        {
            var result = new List<T>();
            for (int row = headers.First().Row + 1; row < cells.Count; row++)
            {
                var rowValues = new Dictionary<CellIndex, string>();
                foreach (var header in headers)
                {
                    rowValues[header] = cells[row][header.Column];
                }

                if (rowValues.Values.All(x => !string.IsNullOrWhiteSpace(x)))
                {
                    result.Add(buildDomainObjectFunc(rowValues));
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
    }
}
