using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml; 
using System.IO;

namespace NeocortexApiSamplePerformance
{
    public class ReadData
    {

        public static List<MultiSequenceInput> MultiSequenceExcelInput(string filePath)
        {
            var inputs = new List<MultiSequenceInput>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0]; // Assuming data is in the first sheet
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++) // Assuming first row contains headers
                {
                    var input = new MultiSequenceInput
                    {
                        ExperimentId = int.Parse(worksheet.Cells[row, 1].Text),
                        ExperimentName = worksheet.Cells[row, 2].Text,
                        CPU = int.Parse(worksheet.Cells[row, 3].Text),
                        DotnetVersion = worksheet.Cells[row, 4].Text,
                        W = int.Parse(worksheet.Cells[row, 5].Text),
                        N = int.Parse(worksheet.Cells[row, 6].Text),