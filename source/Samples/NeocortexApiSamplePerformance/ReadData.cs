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
        