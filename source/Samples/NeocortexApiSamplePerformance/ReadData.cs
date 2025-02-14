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

            // Set EPPlus License Context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0]; // Assuming data is in the first sheet
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++) // Assuming first row contains headers
                {
                    var input = new MultiSequenceInput
                    {
                        ExperimentId = int.TryParse(worksheet.Cells[row, 1].Text, out int experimentId) ? experimentId : 0,
                        ExperimentName = worksheet.Cells[row, 2].Text,
                        CPU = int.TryParse(worksheet.Cells[row, 3].Text, out int cpu) ? cpu : 0,
                        DotnetVersion = worksheet.Cells[row, 4].Text,
                        W = int.TryParse(worksheet.Cells[row, 5].Text, out int w) ? w : 0,
                        N = int.TryParse(worksheet.Cells[row, 6].Text, out int n) ? n : 0,
                        numColumns = int.TryParse(worksheet.Cells[row, 7].Text, out int numColumns) ? numColumns : 0,
                        Radius = double.TryParse(worksheet.Cells[row, 8].Text, out double radius) ? radius : 0.0,
                        MinVal = double.TryParse(worksheet.Cells[row, 9].Text, out double minVal) ? minVal : 0.0,
                        MaxVal = double.TryParse(worksheet.Cells[row, 10].Text, out double maxVal) ? maxVal : 0.0,
                        Periodic = bool.TryParse(worksheet.Cells[row, 11].Text, out bool periodic) && periodic,
                        ClipInput = bool.TryParse(worksheet.Cells[row, 12].Text, out bool clipInput) && clipInput,
                        Name = worksheet.Cells[row, 13].Text,
                        CellsPerColumn = int.TryParse(worksheet.Cells[row, 14].Text, out int cellsPerColumn) ? cellsPerColumn : 0,
                        GlobalInhibition = bool.TryParse(worksheet.Cells[row, 15].Text, out bool globalInhibition) && globalInhibition,
                        LocalAreaDensity = double.TryParse(worksheet.Cells[row, 16].Text, out double localAreaDensity) ? localAreaDensity : 0.0,
                        NumActiveColumnsPerInhArea = double.TryParse(worksheet.Cells[row, 17].Text, out double numActiveColumns) ? (int)numActiveColumns : 0, // Fixed casting
                        PotentialRadius = int.TryParse(worksheet.Cells[row, 18].Text, out int potentialRadius) ? potentialRadius : 0,
                        MaxBoost = double.TryParse(worksheet.Cells[row, 19].Text, out double maxBoost) ? maxBoost : 0.0,
