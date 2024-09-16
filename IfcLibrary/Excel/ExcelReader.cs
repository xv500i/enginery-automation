using ExcelDataReader;
using IfcLibrary.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace IfcLibrary.Excel
{
    public class ExcelReader : IExcelReader
    {
        private const string EntityChangesDataTableName = "Afegir propietats";
        private const string PropertySetCleanupDataTableName = "Borrar propietats";
        private const string FirstHeaderFirstColumnName = "Número";
        private const string PropertySetCleanupFirstHeaderFirstColumnName = "Property Sets";

        public IfcManipulations GetChanges(string path)
        {
            try
            {
                var sheets = GetCells(path);
                var entityChanges = ParseEntityChangeInfos(sheets.First(x => x.Name == EntityChangesDataTableName).Cells);
                var propertySetCleanups = ParsePropertySetCleanups(sheets.First(x => x.Name == PropertySetCleanupDataTableName).Cells);
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

                    var entityChangesSheet = ReadSheet(dataSet, EntityChangesDataTableName);
                    sheets.Add(entityChangesSheet);

                    var propertySetCleanupSheet = ReadSheet(dataSet, PropertySetCleanupDataTableName);
                    sheets.Add(propertySetCleanupSheet);
                }
            }
            return sheets;
        }

        private static Sheet ReadSheet(DataSet dataSet, string name)
        {
            var cells = new List<List<string>>();
            var dataTable = dataSet.Tables[EntityChangesDataTableName];
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
            var propertySetHeader = FindHeaderFor(PropertySetCleanupFirstHeaderFirstColumnName, cells);
            var currentColumn = propertySetHeader.Column + 1;
            var propertySetRow = propertySetHeader.Row;
            var propertyRow = propertySetRow + 1;

            var propertySetCleanups = new Dictionary<string, List<string>>();

            while (currentColumn < cells[propertySetRow].Count && !string.IsNullOrWhiteSpace(cells[propertySetRow][currentColumn]))
            {
                var propertySetName = cells[propertySetRow][currentColumn];
                var propertyName = cells[propertyRow][currentColumn];

                if (!propertySetCleanups.ContainsKey(propertySetName))
                {
                    propertySetCleanups[propertySetName] = new List<string>();
                }
                propertySetCleanups[propertySetName].Add(propertyName);

                currentColumn++;
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
            var numberHeader = FindHeaderFor(FirstHeaderFirstColumnName, cells);
            var entityHeader = new CellIndex
            {
                Row = numberHeader.Row,
                Column = numberHeader.Column + 1,
            };
            var nameHeader = new CellIndex
            {
                Row = entityHeader.Row,
                Column = entityHeader.Column + 1,
            };

            var propertySetHeader = FindHeaderFor(PropertySetCleanupFirstHeaderFirstColumnName, cells);
            var nextColumn = propertySetHeader.Column + 1;
            var propertyInformationHeaders = new List<PropertyInformationHeader>();
            while(nextColumn < cells[nameHeader.Row].Count && !string.IsNullOrWhiteSpace(cells[nameHeader.Row][nextColumn]))
            {
                propertyInformationHeaders.Add(new PropertyInformationHeader
                {
                    Column = nextColumn,
                    PropertySetName = cells[nameHeader.Row][nextColumn],
                    PropertyName = cells[nameHeader.Row+1][nextColumn],
                });
                nextColumn++;
            }

            var excelInfoRows = new List<EntityChange>();
            for(var currentRow = entityHeader.Row + 2; currentRow < cells.Count; currentRow++)
            {
                var entity = cells[currentRow][entityHeader.Column];
                var identifier = cells[currentRow][nameHeader.Column];
                if (!string.IsNullOrWhiteSpace(entity) 
                    && !string.IsNullOrWhiteSpace(identifier))
                {
                    var excelInfoRow = new EntityChange
                    {
                        Entity = entity,
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
