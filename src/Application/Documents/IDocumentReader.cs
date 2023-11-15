﻿using System.Data;

namespace SharedKernel.Application.Documents;

/// <summary>  </summary>
public interface IDocumentReader
{
    /// <summary>  </summary>
    string Extension { get; }

    /// <summary>  </summary>
    DocumentReaderConfiguration Configuration { get; }

    /// <summary>  </summary>
    IDocumentReader Configure(Action<DocumentReaderConfiguration> change);

    /// <summary>  </summary>
    IEnumerable<IRowData> ReadStream(Stream stream);

    /// <summary>  </summary>
    DataTable Read(Stream stream);
}