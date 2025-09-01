//==============================================================================  
/// MIT License  
///  
/// Copyright (c) 2025 Rockwell Automation, Inc.  
///  
/// Permission is hereby granted, free of charge, to any person obtaining a copy  
/// of this software and associated documentation files (the "Software"), to deal  
/// in the Software without restriction, including without limitation the rights  
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell  
/// copies of the Software, and to permit persons to whom the Software is  
/// furnished to do so, subject to the following conditions:  
///  
/// The above copyright notice and this permission notice shall be included in all  
/// copies or substantial portions of the Software.  
///  
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR  
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,  
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE  
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER  
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,  
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE  
/// SOFTWARE.  
///  
//==============================================================================

#nullable enable
using System;
using System.Collections.Immutable;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// A custom kpi collector which records each KPI as a new row, using KPI Name and KPI Value columns.
/// Note: This collector does not work if a primary key has been used when setting up the KPI recorder. 
/// </summary>
public class CustomCsvKpiCollector : Demo3D.TestRunner.KpiRecorders.IKpiCollector
{
    private StringBuilder rowStringBuilder = new();

    /// <summary>
    /// Gets or sets a value indicating whether to append to the CSV file.
    /// </summary>
    public bool Append { get; set; }

    /// <summary>
    /// The name of the current test. This is writted in to the csv row.
    /// </summary>
    public string? TestName { get; set; }

    /// <summary>
    /// Gets or sets the path to write the CSV file to.
    /// </summary>
    public string? OutputPath { get; set; }

    public Task InitializeAsync()
    {
        //Nothing to do.
        return Task.CompletedTask;
    }

    public Task UpdateAsync(DataTable sourceTable, ImmutableArray<int> updatedRows)
    {
        //Note: This csv collector does not work when Primary Keys have been enabled when building the KPI recorder.
        var realTimeCol = sourceTable.Columns["Real Time"];
        var modelTimeCol = sourceTable.Columns["Model Time"];

        //Check the remaining columns for cells that have a value.
        foreach (var rowIndex in updatedRows)
        {
            WriteRow(sourceTable, realTimeCol, modelTimeCol, rowIndex);
        }
        return Task.CompletedTask;
    }

    private void WriteRow(DataTable sourceTable, DataColumn? realTimeCol, DataColumn? modelTimeCol, int rowIndex)
    {
        var row = sourceTable.Rows[rowIndex];
        foreach (DataColumn col in sourceTable.Columns)
        {
            if (col == realTimeCol || col == modelTimeCol)
            {
                continue;
            }
            var cellValue = row[col];
            if (cellValue != DBNull.Value)
            {
                if (TestName != null)
                {
                    rowStringBuilder.Append($"{TestName},");
                }
                if (realTimeCol != null)
                {
                    rowStringBuilder.Append($"{row[realTimeCol]},");
                }
                if (modelTimeCol != null)
                {
                    rowStringBuilder.Append($"{row[modelTimeCol]},");
                }
                rowStringBuilder.AppendLine($"{col.ColumnName},{cellValue}");
            }
        }
    }

    public async Task FinalizeAsync(DataTable sourceTable)
    {
        if (OutputPath == null)
        {
            throw new InvalidDataException("OutputPath is not set.");
        }

        var writeHeaders = true;
        if (File.Exists(OutputPath))
        {
            if (Append)
            {
                writeHeaders = false;
            }
            else
            {
                File.Delete(OutputPath);
            }
        }
        if (writeHeaders)
        {
            StringBuilder headerStringBuilder = WriteHeaders(sourceTable);
            headerStringBuilder.Append(rowStringBuilder);
            await File.AppendAllTextAsync(OutputPath, headerStringBuilder.ToString());
        }
        else
        {
            await File.AppendAllTextAsync(OutputPath, rowStringBuilder.ToString());
        }
        
    }

    private StringBuilder WriteHeaders(DataTable sourceTable)
    {
        var realTimeCol = sourceTable.Columns["Real Time"];
        var modelTimeCol = sourceTable.Columns["Model Time"];
        var headerStringBuilder = new StringBuilder();
        if (TestName != null)
        {
            headerStringBuilder.Append($"Test Name,");
        }
        if (realTimeCol != null)
        {
            headerStringBuilder.Append($"Real Time,");
        }
        if (modelTimeCol != null)
        {
            headerStringBuilder.Append($"Model Time,");
        }
        headerStringBuilder.AppendLine($"KPI Name, KPI Value");
        return headerStringBuilder;
    }
}