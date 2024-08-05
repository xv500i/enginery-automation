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
        private const string DataTableName = "Afegir propietats";
        private const string FirstHeaderFirstColumnName = "Número";
        private const int FirstHeaderPropertySetColumnIndex = 258;

        private List<List<string>> GetCells(string path)
        {
            var results = new List<List<string>>();
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();

                    var firstSheet = result.Tables[DataTableName];

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

        public List<EntityChangeInfo> GetChanges(string path)
        {
            try
            {
                var cells = GetCells(path);
                return ParseEntityChangeInfos(cells);
            }
            catch (Exception e)
            {
                throw new Exception("Could not read excel file", e);
            }
        }

        private List<EntityChangeInfo> ParseEntityChangeInfos(List<List<string>> cells)
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

            var nextColumn = FirstHeaderPropertySetColumnIndex;
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

            var excelInfoRows = new List<EntityChangeInfo>();
            for(var currentRow = entityHeader.Row + 2; currentRow < cells.Count; currentRow++)
            {
                var entity = cells[currentRow][entityHeader.Column];
                var identifier = cells[currentRow][nameHeader.Column];
                if (!string.IsNullOrWhiteSpace(entity) 
                    && !string.IsNullOrWhiteSpace(identifier))
                {
                    var excelInfoRow = new EntityChangeInfo
                    {
                        Entity = entity,
                        Identifier = identifier,
                        PropertyChangeInfos = ParsePropertyChangeInfos(cells, propertyInformationHeaders, currentRow),
                    };

                    excelInfoRows.Add(excelInfoRow);
                }
            }

            return excelInfoRows;
        }

        private List<PropertyChangeInfo> ParsePropertyChangeInfos(List<List<string>> cells, List<PropertyInformationHeader> propertyInformationHeaders, int currentRow)
        {
            var changes = new List<PropertyChangeInfo>();

            foreach(var propertyInformationHeader in propertyInformationHeaders)
            {
                changes.Add(new PropertyChangeInfo
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
