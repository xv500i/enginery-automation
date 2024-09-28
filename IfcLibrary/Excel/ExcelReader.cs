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
        public IfcManipulations GetChanges(string path)
        {
            try
            {
                var sheets = GetCells(path);
                var entityChanges = ParseEntityChangeInfos(sheets.First(x => x.Name == SheetNames.EntityChangesDataTableName).Cells);
                var propertySetCleanups = ParsePropertySetCleanups(sheets.First(x => x.Name == SheetNames.PropertySetCleanupDataTableName).Cells);
                return new IfcManipulations
                {
                    EntityChanges = entityChanges,
                    PropertySetCleanups = propertySetCleanups,
                };
            }
            catch (Exception e)
            {
                throw new Exception("Could not read excel file", e);
            }
        }

        private List<Sheet> GetCells(string path)
        {
            var sheets = new List<Sheet>();
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var dataSet = reader.AsDataSet();

                    var entityChangesSheet = ReadSheet(dataSet, SheetNames.EntityChangesDataTableName);
                    sheets.Add(entityChangesSheet);

                    var propertySetCleanupSheet = ReadSheet(dataSet, SheetNames.PropertySetCleanupDataTableName);
                    sheets.Add(propertySetCleanupSheet);
                }
            }
            return sheets;
        }

        private static Sheet ReadSheet(DataSet dataSet, string name)
        {
            var cells = new List<List<string>>();
            var dataTable = dataSet.Tables[name];
            foreach (DataRow row in dataTable.Rows)
            {
                var rowAsList = new List<string>();
                foreach (DataColumn column in dataTable.Columns)
                {
                    rowAsList.Add(row[column]?.ToString() ?? string.Empty);
                }
                cells.Add(rowAsList);
            }
            var sheet = new Sheet
            {
                Name = name,
                Cells = cells,
            };
            return sheet;
        }

        private List<PropertySetCleanup> ParsePropertySetCleanups(List<List<string>> cells)
        {
            var propertySetHeader = FindHeaderFor(HeaderCellValues.PropertySetHeaderCleanup, cells);
            var propertyHeader = FindHeaderFor(HeaderCellValues.PropertyHeaderCleanup, cells);

            var propertySetCleanups = new Dictionary<string, List<string>>();

            for (int i = propertySetHeader.Row + 1; i < cells.Count; i++)
            {
                var propertySetName = cells[i][propertySetHeader.Column];
                var propertyName = cells[i][propertyHeader.Column];

                if (!string.IsNullOrWhiteSpace(propertyName) && !string.IsNullOrWhiteSpace(propertySetName))
                {
                    if (!propertySetCleanups.ContainsKey(propertySetName))
                    {
                        propertySetCleanups[propertySetName] = new List<string>();
                    }
                    propertySetCleanups[propertySetName].Add(propertyName);
                }
            }

            var result = new List<PropertySetCleanup>();
            foreach (var item in propertySetCleanups)
            {
                result.Add(new PropertySetCleanup
                {
                    PropertySetName = item.Key,
                    PropertyNamesToKeep = item.Value,
                });
            }
            return result;
        }

        private List<EntityChange> ParseEntityChangeInfos(List<List<string>> cells)
        {
            var propertySetHeader = FindHeaderFor(HeaderCellValues.PropertySetHeader, cells);
            var nextColumn = propertySetHeader.Column + 1;
            var propertyInformationHeaders = new List<PropertyInformationHeader>();
            while(nextColumn < cells[propertySetHeader.Row].Count && !string.IsNullOrWhiteSpace(cells[propertySetHeader.Row][nextColumn]))
            {
                propertyInformationHeaders.Add(new PropertyInformationHeader
                {
                    Column = nextColumn,
                    PropertySetName = cells[propertySetHeader.Row][nextColumn],
                    PropertyName = cells[propertySetHeader.Row+1][nextColumn],
                });
                nextColumn++;
            }

            var guidHeader = FindHeaderFor(HeaderCellValues.Guid, cells);

            var excelInfoRows = new List<EntityChange>();
            for(var currentRow = guidHeader.Row + 3; currentRow < cells.Count; currentRow++)
            {
                var identifier = cells[currentRow][guidHeader.Column];
                if (!string.IsNullOrWhiteSpace(identifier))
                {
                    var excelInfoRow = new EntityChange
                    {
                        Identifier = identifier,
                        PropertyChanges = ParsePropertyChangeInfos(cells, propertyInformationHeaders, currentRow),
                    };

                    excelInfoRows.Add(excelInfoRow);
                }
            }

            return excelInfoRows;
        }

        private List<PropertyChange> ParsePropertyChangeInfos(List<List<string>> cells, List<PropertyInformationHeader> propertyInformationHeaders, int currentRow)
        {
            var changes = new List<PropertyChange>();

            foreach(var propertyInformationHeader in propertyInformationHeaders)
            {
                changes.Add(new PropertyChange
                {
                    Value = cells[currentRow][propertyInformationHeader.Column],
                    PropertyName = propertyInformationHeader.PropertyName,
                    PropertySetName = propertyInformationHeader.PropertySetName,
                });
            }

            return changes;
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
