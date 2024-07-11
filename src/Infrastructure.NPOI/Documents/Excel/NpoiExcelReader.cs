﻿using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SharedKernel.Application.Documents;
using SharedKernel.Infrastructure.Documents;
using System.Data;

namespace SharedKernel.Infrastructure.NPOI.Documents.Excel;

/// <summary>  </summary>
public class NpoiExcelReader : DocumentReader, IExcelReader
{
    /// <summary>  </summary>
    public override string Extension => "xlsx";

    /// <summary>  </summary>
    public override IEnumerable<IRowData> ReadStream(Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);

        using var workbook = new XSSFWorkbook(stream);

        var sheet = workbook.GetSheetAt(Configuration.SheetIndex);

        var formulateEvaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();

        SetColumnNames(sheet);
        for (var rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
        {
            var row = sheet.GetRow(rowIndex);
            if (row == null)
                continue;

            yield return new NpoiExcelRow(rowIndex + 1, row, ColumnNames, Configuration.CultureInfo, formulateEvaluator);
        }
    }

    /// <summary>  </summary>
    public DataSet ReadTabs(Stream stream)
    {
        var dataSet = new DataSet();

        using IWorkbook workbook = new XSSFWorkbook(stream);

        for (var i = 0; i < workbook.NumberOfSheets; i++)
        {
            var sheet = workbook.GetSheetAt(i);
            var dataTable = ReadSheet(sheet);
            dataSet.Tables.Add(dataTable);
        }

        return dataSet;
    }

    /// <summary>  </summary>
    public override DataTable Read(Stream stream)
    {
        using IWorkbook workbook = new XSSFWorkbook(stream);
        var sheet = workbook.GetSheetAt(Configuration.SheetIndex);
        return ReadSheet(sheet);
    }

    private DataTable ReadSheet(ISheet sheet)
    {
        var dataTable = new DataTable(sheet.SheetName);

        // write the header row
        SetColumnNames(sheet);

        if (Configuration.IncludeLineNumbers)
            dataTable.Columns.Add(Configuration.ColumnLineNumberName);

        foreach (var column in ColumnNames)
        {
            dataTable.Columns.Add(column);
        }

        // write the rest
        for (var i = 1; i < sheet.PhysicalNumberOfRows; i++)
        {
            var sheetRow = sheet.GetRow(i);
            var dtRow = dataTable.NewRow();

            var valores = new List<string>();
            // ReSharper disable once RedundantSuppressNullableWarningExpression
            if (Configuration.IncludeLineNumbers)
                valores.Add((sheetRow?.RowNum + 1).ToString()!);

            // ReSharper disable once RedundantSuppressNullableWarningExpression
            dataTable.Columns
                .Cast<DataColumn>()
                .ToList()
                .ForEach(c => valores.Add(sheetRow!.GetCell(c.Ordinal, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString()!));

            if (sheetRow == default || !valores.Skip(1).Any() || valores.Skip(1).All(string.IsNullOrWhiteSpace))
                continue;

            dtRow.ItemArray = valores.Cast<object>().Take(dataTable.Columns.Count).ToArray();
            dataTable.Rows.Add(dtRow);
        }

        return dataTable;
    }

    private void SetColumnNames(ISheet sheet)
    {
        var headerRow = sheet.GetRow(0);
        var duplicates = 0;
        var columns = new List<string>();
        foreach (var headerCell in headerRow.Select(h => h.ToString()).Where(h => !string.IsNullOrWhiteSpace(h)))
        {
            var headerCellTrim = headerCell?.Trim()!;
            if (columns.Contains(headerCellTrim))
            {
                duplicates++;
                columns.Add($"{headerCellTrim}_{duplicates}");
            }
            else
            {
                columns.Add(headerCellTrim);
            }
        }

        ColumnNames = columns;
    }
}
