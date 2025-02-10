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
                        Radius = double.Parse(worksheet.Cells[row, 7].Text),
                        MinVal = double.Parse(worksheet.Cells[row, 8].Text),
                        MaxVal = double.Parse(worksheet.Cells[row, 9].Text),
                        Periodic = bool.Parse(worksheet.Cells[row, 10].Text),
                        ClipInput = bool.Parse(worksheet.Cells[row, 11].Text),
                        Name = worksheet.Cells[row, 12].Text,
                        CellsPerColumn = int.Parse(worksheet.Cells[row, 13].Text),
                        GlobalInhibition = bool.Parse(worksheet.Cells[row, 14].Text),
                        LocalAreaDensity = double.Parse(worksheet.Cells[row, 15].Text),
                        NumActiveColumnsPerInhArea = double.Parse(worksheet.Cells[row, 16].Text),
                        PotentialRadius = int.Parse(worksheet.Cells[row, 17].Text),
                        MaxBoost = double.Parse(worksheet.Cells[row, 18].Text),
                        DutyCyclePeriod = int.Parse(worksheet.Cells[row, 19].Text),
                        MinPctOverlapDutyCycles = double.Parse(worksheet.Cells[row, 20].Text),
                        MaxSynapsesPerSegment = int.Parse(worksheet.Cells[row, 21].Text),