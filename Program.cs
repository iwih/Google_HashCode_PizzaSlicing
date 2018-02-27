using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


        private static async void SliceThisPizza(string path)
        {
            _inputPath = path;
            _pizza = new Pizza(path);
            _inputFileName = Path.GetFileName(_inputPath);

            //MultiDimensionalSlicing(_pizza, true, false, false);

            //MultiDimensionalSlicing(_pizza, false, false, false);


            //MultiDimensionalSlicing(_pizza, true, true, false);

            
            SlicingUltimate(_pizza.Clone());
        }

        private static void MultiDimensionalSlicing(Pizza pizza, bool auto, bool optimized, bool ultimate)
        {
            
                
                var defaultColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine(
                    $"+-~-+-~-+-~-+-~-+-~-+-~-+   START SLICING PIZZA {_inputFileName} :: BI-DIMENSIONAL +-~-+-~-+-~-+-~-+-~-+-~-+");
                Console.WriteLine();
                Console.ForegroundColor = defaultColor;
                var slicesAttemps = new SlicingAttempts();
                if (optimized)
                    slicesAttemps = Slice(pizza.Clone());
                else
                    slicesAttemps = auto ? Slice(pizza.Clone(), false, true) : Slice(pizza.Clone(), true, false);


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

        private static SizePossibleSlice GetPossibleSliceDimensions()
        {
            var attempsPossible = new SizePossibleSlice();
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
            attempsPossible.Push(5, 1);
            attempsPossible.Push(1, 5);
            attempsPossible.Push(4, 1);
            attempsPossible.Push(2, 2);
            attempsPossible.Push(3, 1);
            attempsPossible.Push(1, 3);
            attempsPossible.Push(1, 4);
            attempsPossible.Push(2, 1);
            attempsPossible.Push(1, 2);

            return attempsPossible;
        }

        private static SizePossibleSlice GetPossibleSliceDimensions(int minCells, int maxCells)
        {
            var attempsPossible = new SizePossibleSlice();

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
                            pizzaSlice.StartPoint.Y + " " +
                            pizzaSlice.EndPoint.X + " " +
                            pizzaSlice.EndPoint.Y);
                        totalCellsSliced += pizzaSlice.Size;
                    }
                }
            }

            return (pizza.Content, successfulSlices, totalCellsSliced);
        }

        private static SlicingAttempts Slice(Pizza pizza)
        {
            var slicingAttempts = new SlicingAttempts();
            slicingAttempts.PushAttempt(null, new List<string>(), 0, 0, 0);

            var sizePossibleSlice = GetPossibleSliceDimensions(pizza.CellsMinInSlice, pizza.CellsMaxInSlice);

            //optimized bi-dimensional slicing
            for (var row = 0; row < pizza.RowsPizzaCount; row++)
            {
                for (var column = 0; column < pizza.ColumnsPizzaCount; column++)
                {
                    foreach (var possibleSliceSize in sizePossibleSlice)
                    {
                        var startPointSlice = new Point(row, column);
                        var quadDirectionsEndPoints =
                            GetPossibleQuadDirectionalEndPoints(
                                row, column,
                                pizza.RowsPizzaCount, pizza.ColumnsPizzaCount,
                                possibleSliceSize);

                        foreach (var endPoint in quadDirectionsEndPoints)
                        {
                            var slice = new SlicePizza(pizza, startPointSlice, endPoint);
                            if (slice.IsValidSlice)
                            {
                                pizza.CutSlice(slice);

                                slicingAttempts[0].SuccessfulSlices.Add(
                                    slice.StartPoint.X + " " +
                                    slice.StartPoint.Y + " " +
                                    slice.EndPoint.X + " " +
                                    slice.EndPoint.Y);

                                slicingAttempts[0].TotalSlicedCells += slice.Size;
                            }
                        }
                    }
                }
            }

            slicingAttempts[0].SlicePizzaContent = pizza.Content;

            return slicingAttempts;
        }

        private static async void SlicingUltimate(Pizza pizza)
        {
            var slicingAttempts = new SlicingAttempts();
            var possibleSlices = new List<PossibleSlices>();

            var sizesPossibleSlice = GetPossibleSliceDimensions(pizza.CellsMinInSlice, pizza.CellsMaxInSlice);

            //optimized bi-dimensional slicing
            for (var row = 0; row < pizza.RowsPizzaCount; row++)
            {
                for (var column = 0; column < pizza.ColumnsPizzaCount; column++)
                {
                    var startPointSlice = new Point(row, column);
                    var slicesValid = new List<SlicePizza>();
                    foreach (var sizeSlice in sizesPossibleSlice)
                    {
                        var quadDirectionsEndPoints =
                            GetPossibleQuadDirectionalEndPoints(
                                row, column,
                                pizza.RowsPizzaCount, pizza.ColumnsPizzaCount,
                                sizeSlice);


                        slicesValid.AddRange(GetAllPossibleSliceForAnOrigin(pizza, quadDirectionsEndPoints,
                            startPointSlice));
                    }

                    if (slicesValid.Any())
                        possibleSlices.Add(new PossibleSlices(startPointSlice, slicesValid));
                }
            }

            //generating all possible slicing layout

            
        }

        private static List<SlicePizza> GetAllPossibleSliceForAnOrigin(
            Pizza pizza,
            List<Point> quadDirectionsEndPoints,
            Point startPointSlice)
        {
            var slicesValid = new List<SlicePizza>();
            foreach (var endPoint in quadDirectionsEndPoints)
            {
                var slice = new SlicePizza(pizza, startPointSlice, endPoint);
                if (slice.IsValidSlice)
                {
                    slicesValid.Add(slice);
                }
            }

            return slicesValid;
        }

        private static List<Point> GetPossibleQuadDirectionalEndPoints(
            int row,
            int column,
            int rowsCount,
            int colsCount,
            SizePossibleSlice possibleSliceSize)
        {
            //only negative coordinations are rejected
            var quadDirectionsEndPoints = new List<Point>();

            var maxRow = rowsCount - 1;
            var maxCol = colsCount - 1;

            var forwardSliceX = row + possibleSliceSize.RowsCount - 1;
            var forwardSliceY = column + possibleSliceSize.ColumnsCount - 1;

            var backwrdSliceX = row + possibleSliceSize.RowsCount - 1;
            var backwrdSliceY = column - possibleSliceSize.ColumnsCount + 1;

            var upMrrorSliceX = row - possibleSliceSize.RowsCount + 1;
            var upMrrorSliceY = column + possibleSliceSize.ColumnsCount - 1;

            var diagnalSliceX = row - possibleSliceSize.RowsCount + 1;
            var diagnalSliceY = column - possibleSliceSize.ColumnsCount + 1;


            if (forwardSliceX >= 0 && forwardSliceY >= 0)
                if (forwardSliceX <= maxRow && forwardSliceY <= maxCol)
                    quadDirectionsEndPoints.Add(new Point(forwardSliceX, forwardSliceY));

            if (backwrdSliceX >= 0 && backwrdSliceY >= 0)
                if (backwrdSliceX <= maxRow && backwrdSliceY <= maxCol)
                    quadDirectionsEndPoints.Add(new Point(backwrdSliceX, backwrdSliceY));

            if (upMrrorSliceX >= 0 && upMrrorSliceY >= 0)
                if (upMrrorSliceX <= maxRow && upMrrorSliceY <= maxCol)
                    quadDirectionsEndPoints.Add(new Point(upMrrorSliceX, upMrrorSliceY));

            if (diagnalSliceX >= 0 && diagnalSliceY >= 0)
                if (diagnalSliceX <= maxRow && diagnalSliceY <= maxCol)
                    quadDirectionsEndPoints.Add(new Point(diagnalSliceX, diagnalSliceY));

            return quadDirectionsEndPoints;
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

        private class PossibleSlices
        {
            public Point OriginPoint { get; }
            public List<SlicePizza> Slices { get; }

            public PossibleSlices(Point originPoint, List<SlicePizza> slices)
            {
                OriginPoint = originPoint;
                Slices = slices;
            }
        }

        private class SlicingAttempts : List<SlicingAttempts>
        {
            public char[,] SlicePizzaContent { set; get; }
            public List<string> SuccessfulSlices { private set; get; }
            public int TotalSlicedCells { set; get; }
            public Size Dimensions { private set; get; }
            public int Size { set; get; }

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

        private class SizePossibleSlice : List<SizePossibleSlice>
        {
            public int RowsCount { private set; get; }
            public int ColumnsCount { private set; get; }

            public void Push(int rows, int columns)
            {
                Add(new SizePossibleSlice {RowsCount = rows, ColumnsCount = columns});
            }
        }

        public class SlicePizza
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

                var rowsCount = Math.Abs(endPoint.X - startPoint.X) + 1;
                var colCount = Math.Abs(endPoint.Y - startPoint.Y) + 1;

                Content = new char[rowsCount, colCount];
                Size = rowsCount * colCount;

                var unSliceableArea = false;
                var rowsCounter = 0;

                var strtngX = Math.Min(startPoint.X, endPoint.X);
                var endingX = Math.Max(startPoint.X, endPoint.X);
                for (var row = strtngX; row <= endingX; row++)
                {
                    if (unSliceableArea) break;
                    var columnsCounter = 0;

                    var strtngY = Math.Min(startPoint.Y, endPoint.Y);
                    var endingY = Math.Max(startPoint.Y, endPoint.Y);
                    for (var column = strtngY; column <= endingY; column++)
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

        public class Pizza
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

                WritePizzaInfoToConsole(pizzaPath);
            }

            private void WritePizzaInfoToConsole(
                string pizzaPath)
            {
                Console.WriteLine($"Pizza \"{pizzaPath}\" has been loaded successfully...");
                Console.WriteLine($"Pizza is {RowsPizzaCount} x {ColumnsPizzaCount}");
                Console.WriteLine($"Min. # of cells in each slice = {CellsMinInSlice}");
                Console.WriteLine($"Max. # of cells in each slice = {CellsMaxInSlice}");
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