using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Graphics
{
    class Program
    {
        public const char TOMATO = 'T';
        public const char MASHROOM = 'M';
        public const char EMPTY = '0';

        private static string _directoryForOutput;
        private static string _inputFileName;

        private static void Main(string[] args)
        {
            var defaultDir = @"D:\output\";
            Console.Write($"Enter directory to store outputs ({defaultDir}):\t");
            _directoryForOutput = Console.ReadLine();
            if (!Directory.Exists(_directoryForOutput))
            {
                Console.WriteLine($"Given ouput directory does not exit, will use {defaultDir}...");
                _directoryForOutput = defaultDir;
            }

            Console.WriteLine();
            Console.WriteLine("<-----~-----+- STARTED -+-----~----->");
            Console.WriteLine();

            for (var i = 0; i < args.Length; i++)
            {
                var path = args[i];
                if (File.Exists(path))
                {
                    SliceThisPizza(path);
                }
            }

            Console.WriteLine();
            Console.WriteLine("<-----~-----+- FINISHED -+-----~----->");
            Console.ReadLine();
        }

        private static Pizza _pizza;

        private static string _inputPath;

        private static string PathToSave(string operationName, string extension)
        {
            var combinedFileName = _inputFileName + "__" + operationName + extension;
            return Path.Combine(_directoryForOutput, combinedFileName);
        }

        private static void WritePizzaInfoToConsole()
        {
            //var pathTokens = path.Split(new[] {"\\"}, StringSplitOptions.RemoveEmptyEntries);
            //Console.WriteLine($"[{pathTokens[pathTokens.Length - 1]}]");
            Console.WriteLine($"Pizza is {_pizza.RowsPizzaCount} x {_pizza.ColumnsPizzaCount}");
            Console.WriteLine($"Min. # of cells in each slice = {_pizza.IngredintsMinInSlice * 2}");
            Console.WriteLine($"Max. # of cells in each slice = {_pizza.CellsMaxInSlice}");
        }

        private static void SliceThisPizza(string path)
        {
            _inputPath = path;
            _pizza = new Pizza(path);
            _inputFileName = Path.GetFileName(_inputPath);

            MultiDimensionalSlicing(_pizza, true);

            //MultiDimensionalSlicing(_pizza, false);
        }

        private static void MultiDimensionalSlicing(Pizza pizza, bool auto)
        {
            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine(
                $"+-~-+-~-+-~-+-~-+-~-+-~-+   START SLICING PIZZA {_inputFileName} :: BI-DIMENSIONAL +-~-+-~-+-~-+-~-+-~-+-~-+");
            Console.WriteLine();
            Console.ForegroundColor = defaultColor;

            var slicesAttemps = auto ? Slice(pizza.Clone(), false, true) : Slice(pizza.Clone(), true, false);

            var sortedAttempts = slicesAttemps.OrderBy(attempt => attempt.TotalSlicedCells).ToList();

            for (var i = 0; i < sortedAttempts.Count; i++)
            {
                var sliceAttempt = sortedAttempts[i];
                var operationName =
                    $"bi-dimensional-slice-{sliceAttempt.Dimensions.Height}x{sliceAttempt.Dimensions.Width}";

                Console.WriteLine(
                    $"{i}.\tAttempt Dimensions:: Rows:\t{sliceAttempt.Dimensions.Height}\tx\tColumns:\t{sliceAttempt.Dimensions.Width}\tSize: {sliceAttempt.Size}");
                var slicedRatio = (float) sliceAttempt.TotalSlicedCells / (float) pizza.Size * 100;
                Console.WriteLine(
                    $"\tTotal sliced cells: {sliceAttempt.TotalSlicedCells} of {pizza.Size}, Percentage: {slicedRatio} %");

                DrawPizzaTo2X2(
                    sliceAttempt.SlicePizzaContent, pizza.RowsPizzaCount,
                    pizza.ColumnsPizzaCount, operationName);

                GenerateOutputFile(sliceAttempt.SuccessfulSlices, operationName);

                WritePizzaContent(
                    sliceAttempt.SlicePizzaContent, pizza.RowsPizzaCount,
                    pizza.ColumnsPizzaCount, operationName);

                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine(
                $"+-~-+-~-+-~-+-~-+-~-+-~-+ FINISHED SLICING PIZZA {_inputFileName} :: BI-DIMENSIONAL +-~-+-~-+-~-+-~-+-~-+-~-+");
            Console.WriteLine();
            Console.ForegroundColor = defaultColor;
        }

        private class SlicingAttempts : List<SlicingAttempts>
        {
            public char[,] SlicePizzaContent { set; get; }
            public List<string> SuccessfulSlices { private set; get; }
            public int TotalSlicedCells { set; get; }
            public Size Dimensions { private set; get; }
            public int Size { private set; get; }

            public void PushAttempt(
                char[,] slicePizzaContent,
                List<string> successfulSlices,
                int totalSlicedCells,
                int rowsCount,
                int colsCount)
            {
                Add(new SlicingAttempts
                {
                    SlicePizzaContent = slicePizzaContent,
                    SuccessfulSlices = successfulSlices,
                    TotalSlicedCells = totalSlicedCells,
                    Dimensions = new Size(colsCount, rowsCount),
                    Size = colsCount * rowsCount
                });
            }
        }

        private class AttempsRowsByColumns : List<AttempsRowsByColumns>
        {
            public int RowsCount { private set; get; }
            public int ColumnsCount { private set; get; }

            public void Push(int rows, int columns)
            {
                Add(new AttempsRowsByColumns {RowsCount = rows, ColumnsCount = columns});
            }
        }

        private static SlicingAttempts Slice(Pizza pizza, bool useSamePizzaEachTime, bool autoSizing)
        {
            var slicingAttempts = new SlicingAttempts();
            //in case of using same pizza each time, push one attempt and then increment the result of each slicing to it
            if (useSamePizzaEachTime)
                slicingAttempts.PushAttempt(new char[pizza.RowsPizzaCount, pizza.ColumnsPizzaCount],
                    new List<string>(), 0, 0, 0);

            var attempsPossible = autoSizing
                ? GetPossibleSliceDimensions(pizza.CellsMinInSlice, pizza.CellsMaxInSlice)
                : GetPossibleSliceDimensions();

            //executing the possible slice(s)
            foreach (var attempt in attempsPossible)
            {
                //to use the same pizza, or a new clone for each slice-operation
                var pizzaToSlice = useSamePizzaEachTime ? pizza : pizza.Clone();

                (var content, var slices, var cells) =
                    MultiDimensionalSlicing_SuccessorApproach(pizzaToSlice, attempt.RowsCount, attempt.ColumnsCount);

                if (useSamePizzaEachTime)
                {
                    slicingAttempts[0].SlicePizzaContent = content;
                    slicingAttempts[0].TotalSlicedCells += cells;
                    slicingAttempts[0].SuccessfulSlices.AddRange(slices);
                }
                else
                    slicingAttempts.PushAttempt(content, slices, cells, attempt.RowsCount, attempt.ColumnsCount);
            }

            return slicingAttempts;
        }

        private static AttempsRowsByColumns GetPossibleSliceDimensions()
        {
            var attempsPossible = new AttempsRowsByColumns();
            //big.in pizza
            /*attempsPossible.Push(1, 14);
            attempsPossible.Push(14, 1);
            attempsPossible.Push(2, 7);
            attempsPossible.Push(7, 2);
            attempsPossible.Push(1, 13);
            attempsPossible.Push(13, 1);
            attempsPossible.Push(1, 12);
            attempsPossible.Push(12, 1);
            attempsPossible.Push(2, 6);
            attempsPossible.Push(6, 2);
            attempsPossible.Push(3, 4);
            attempsPossible.Push(4, 3);*/

            //medium.in pizza
            /*attempsPossible.Push(1,12);
            attempsPossible.Push(12,1);
            attempsPossible.Push(2,6);
            attempsPossible.Push(1,11);
            attempsPossible.Push(3,4);
            attempsPossible.Push(11,1);
            attempsPossible.Push(6,2);
            attempsPossible.Push(4,3);
            attempsPossible.Push(10,1);
            attempsPossible.Push(1,10);
            attempsPossible.Push(2,5);
            attempsPossible.Push(5,2);
            attempsPossible.Push(9,1);
            attempsPossible.Push(1,9);
            attempsPossible.Push(3,3);
            attempsPossible.Push(1,8);
            attempsPossible.Push(8,1);
            attempsPossible.Push(2,4);
            attempsPossible.Push(4,2);*/

            //small.in
            /*attempsPossible.Push(5, 1);
            attempsPossible.Push(1, 5);
            attempsPossible.Push(4, 1);
            attempsPossible.Push(2, 2);
            attempsPossible.Push(3, 1);
            attempsPossible.Push(1, 3);
            attempsPossible.Push(1, 4);
            attempsPossible.Push(2, 1);
            attempsPossible.Push(1, 2);*/

            return attempsPossible;
        }

        private static AttempsRowsByColumns GetPossibleSliceDimensions(int minCells, int maxCells)
        {
            var attempsPossible = new AttempsRowsByColumns();

            //finding all possible slices dimensions
            for (var cellsInSlice = minCells; cellsInSlice <= maxCells; cellsInSlice++)
            {
                for (var rowsInSlice = 1; rowsInSlice <= cellsInSlice; rowsInSlice++)
                {
                    //checking for valid rows x columns combination
                    if ((cellsInSlice % rowsInSlice) != 0) continue;
                    //columns count
                    var colsInSlice = cellsInSlice / rowsInSlice;
                    //reaching this far means the combination is acceptable
                    attempsPossible.Push(rowsInSlice, colsInSlice);
                }
            }

            return attempsPossible;
        }

        /// <summary>
        /// This method slices the pizzaCell in way that the origin point of each slice-attempt is next to the
        /// origin of the previous one always.
        /// </summary>
        /// <returns></returns>
        private static (char[,], List<string>, int) MultiDimensionalSlicing_SuccessorApproach(
            Pizza pizza,
            int rowsInSlice,
            int colsInSlice)
        {
            var successfulSlices = new List<string>();
            var totalCellsSliced = 0;

            for (var i = 0; rowsInSlice <= (pizza.RowsPizzaCount - i); i++)
            {
                for (var j = 0; colsInSlice <= (pizza.ColumnsPizzaCount - j); j++)
                {
                    var startPoint = new Point(i, j);
                    var endPoint = new Point(i + rowsInSlice - 1, j + colsInSlice - 1);

                    var pizzaSlice = new SlicePizza(pizza, startPoint, endPoint);

                    if (pizzaSlice.IsValidSlice)
                    {
                        pizza.CutSlice(pizzaSlice);
                        successfulSlices.Add(
                            pizzaSlice.StartPoint.X + " " +
                            pizzaSlice.EndPoint.X + " " +
                            pizzaSlice.StartPoint.Y + " " +
                            pizzaSlice.EndPoint.Y);
                        totalCellsSliced += pizzaSlice.Size;
                    }
                }
            }

            return (pizza.Content, successfulSlices, totalCellsSliced);
        }

        private static void GenerateOutputFile(List<string> successfulSlices, string operationName)
        {
            Console.WriteLine($"Generating output file of operation: {operationName}");
            var outputFileName = PathToSave(operationName, ".out");

            File.WriteAllText(outputFileName, successfulSlices.Count + Environment.NewLine);
            File.AppendAllLines(outputFileName, successfulSlices);

            Console.WriteLine($"Saving Output file of operation: {operationName}, to {outputFileName}");
        }

        private static void WritePizzaContent(char[,] pizzaContent, int rows, int columns, string operationName)
        {
            var pathToSave = PathToSave("sliced-pizzaCell-" + operationName, ".out");

            File.WriteAllText(pathToSave, string.Empty);

            for (int i = 0; i < rows; i++)
            {
                var lineContent = string.Empty;
                for (int j = 0; j < columns; j++)
                {
                    lineContent += pizzaContent[i, j];
                }

                File.AppendAllText(pathToSave, lineContent + Environment.NewLine);
            }
        }

        private static void DrawPizzaTo2X2(char[,] pizzaContent, int rows, int columns, string operationName)
        {
            Console.WriteLine($"Painting operation: {operationName}");
            using (var bitmap = new Bitmap(rows * 2, columns * 2))
            {
                using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                {
                    for (var row = 0; row < rows; row++)
                    {
                        for (var column = 0; column < columns; column++)
                        {
                            var ingredientColor = IngredientColor(pizzaContent[row, column]);

                            var x1 = column * 2;
                            var x2 = x1 + 1;
                            var y1 = row * 2;
                            var y2 = y1 + 1;

                            var pRow1Start = new Point(x1, y1);
                            var pRow1End = new Point(x2, y1);

                            var pRow2Start = new Point(x1, y2);
                            var pRow2End = new Point(x2, y2);

                            var pen = new Pen(ingredientColor, 1);
                            graphics.DrawLine(pen, pRow1Start, pRow1End);
                            graphics.DrawLine(pen, pRow2Start, pRow2End);
                        }
                    }

                    try
                    {
                        var outputFileName = PathToSave(operationName, ".bmp");
                        Console.WriteLine($"Saving Bitmap of operation: {operationName}, to {outputFileName}");
                        bitmap.Save(outputFileName);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static Color IngredientColor(char pizzaCell)
        {
            Color ingredientColor;
            switch (pizzaCell)
            {
                case TOMATO:
                    ingredientColor = Color.Maroon;
                    break;
                case MASHROOM:
                    ingredientColor = Color.DarkSeaGreen;
                    break;
                default:
                    ingredientColor = Color.AntiqueWhite;
                    break;
            }

            return ingredientColor;
        }

        private class SlicePizza
        {
            public int Size { get; }
            public Point StartPoint { get; }
            public Point EndPoint { get; }
            private int TomatoCount { get; }
            private int MashroomCount { get; }
            public bool IsValidSlice { get; }
            private char[,] Content { get; }

            public SlicePizza(Pizza pizza, Point startPoint, Point endPoint)
            {
                StartPoint = startPoint;
                EndPoint = endPoint;

                var rowsCount = endPoint.X - startPoint.X + 1;
                var colCount = endPoint.Y - startPoint.Y + 1;

                Content = new char[rowsCount, colCount];
                Size = rowsCount * colCount;

                var unSliceableArea = false;
                var rowsCounter = 0;
                for (var row = startPoint.X; row <= endPoint.X; row++)
                {
                    if (unSliceableArea) break;
                    var columnsCounter = 0;
                    for (var column = startPoint.Y; column <= endPoint.Y; column++)
                    {
                        var ingredient = pizza.Content[row, column];
                        Content[rowsCounter, columnsCounter] = ingredient;

                        if (ingredient == TOMATO)
                            TomatoCount++;
                        else if (ingredient == MASHROOM)
                            MashroomCount++;
                        else
                        {
                            unSliceableArea = true;
                            break;
                        }

                        columnsCounter++;
                    }

                    rowsCounter++;
                }


                if (unSliceableArea)
                    IsValidSlice = false;
                else
                {
                    var pizzaIngredintsMinInSlice = pizza.IngredintsMinInSlice;
                    IsValidSlice =
                        (TomatoCount >= pizzaIngredintsMinInSlice) &&
                        (MashroomCount >= pizzaIngredintsMinInSlice) &&
                        (Size >= pizza.CellsMinInSlice) &&
                        (Size <= pizza.CellsMaxInSlice);
                }
            }
        }

        private class Pizza
        {
            public int RowsPizzaCount { private set; get; }
            public int ColumnsPizzaCount { private set; get; }
            public int IngredintsMinInSlice { private set; get; }
            public int CellsMaxInSlice { private set; get; }
            public int CellsMinInSlice { private set; get; }
            public int Size { private set; get; }

            public char[,] Content { private set; get; }

            private Pizza()
            {
            }

            public Pizza(string pizzaPath)
            {
                var pizzaInput = File.ReadAllText(pizzaPath, Encoding.ASCII);

                var tokensPizza = pizzaInput.Split(new[] {"\n"}, StringSplitOptions.None);

                var headerTokens = tokensPizza[0].Split(new[] {" "}, StringSplitOptions.None);
                RowsPizzaCount = int.Parse(headerTokens[0]);
                ColumnsPizzaCount = int.Parse(headerTokens[1]);
                IngredintsMinInSlice = int.Parse(headerTokens[2]);
                CellsMaxInSlice = int.Parse(headerTokens[3]);
                CellsMinInSlice = IngredintsMinInSlice * 2;
                Size = ColumnsPizzaCount * RowsPizzaCount;

                Content = new char[RowsPizzaCount, ColumnsPizzaCount];

                for (var counter = 1; counter < tokensPizza.Length; counter++)
                {
                    var rowsIndex = counter - 1;
                    for (var j = 0; j < tokensPizza[counter].Length; j++)
                    {
                        Content[rowsIndex, j] = tokensPizza[counter][j];
                    }
                }

                Console.WriteLine($"Pizza \"{pizzaPath}\" has been loaded successfully...");
            }

            public void CutSlice(SlicePizza slice)
            {
                for (var row = slice.StartPoint.X; row <= slice.EndPoint.X; row++)
                {
                    for (var column = slice.StartPoint.Y; column <= slice.EndPoint.Y; column++)
                    {
                        Content[row, column] = EMPTY;
                    }
                }
            }

            public Pizza Clone()
            {
                return new Pizza
                {
                    RowsPizzaCount = RowsPizzaCount,
                    ColumnsPizzaCount = ColumnsPizzaCount,
                    IngredintsMinInSlice = IngredintsMinInSlice,
                    CellsMaxInSlice = CellsMaxInSlice,
                    CellsMinInSlice = CellsMinInSlice,
                    Content = (char[,]) Content.Clone(),
                    Size = Size
                };
            }
        }
    }
}