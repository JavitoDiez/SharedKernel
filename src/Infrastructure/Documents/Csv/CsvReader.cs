﻿using SharedKernel.Application.Documents;
using System.Data;

namespace SharedKernel.Infrastructure.Documents.Csv;

/// <summary> </summary>
public class CsvReader : DocumentReader, ICsvReader
{
    /// <summary> </summary>
    public override string Extension => "csv";

    /// <summary> </summary>
    public override IEnumerable<IRowData> ReadStream(Stream stream)
    {
        var streamReader = new StreamReader(stream);
        var headers = streamReader.ReadLine()!.Split(Configuration.Separator).Select(x => x.Trim()).ToList();

        var lineNumber = 1;
        while (!streamReader.EndOfStream)
        {
            var rows = streamReader.ReadLine()!.Split(Configuration.Separator).ToList();

            lineNumber++;
            yield return new CsvRow(lineNumber, rows, headers, Configuration.CultureInfo);
        }
    }

    /// <summary> </summary>
    public override DataTable Read(Stream stream)
    {
        var streamReader = new StreamReader(stream);
        var dataTable = new DataTable();
        var headers = streamReader.ReadLine()!.Split(Configuration.Separator);

        if (Configuration.IncludeLineNumbers)
            dataTable.Columns.Add(Configuration.ColumnLineNumberName, typeof(int));

        foreach (var header in headers)
        {
            dataTable.Columns.Add(header);
        }

        var lineNumber = 1;
        while (!streamReader.EndOfStream)
        {
            var rows = streamReader.ReadLine()!.Split(Configuration.Separator);
            var dr = dataTable.NewRow();
            if (Configuration.IncludeLineNumbers)
                dr[Configuration.ColumnLineNumberName] = lineNumber;

            lineNumber++;
            for (var i = 1; i < headers.Length; i++)
            {
                dr[i] = rows[i];
            }
            dataTable.Rows.Add(dr);
        }

        return dataTable;
    }
}