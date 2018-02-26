using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
            var slicedPizza = new string[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var path = args[i];
                if (File.Exists(path))
                {
                    slicedPizza[i] = SliceThisPizza(path);
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

        private static string SliceThisPizza(string path)
        {
            _inputPath = path;
            var output = string.Empty;
            _pizza = new Pizza(path);
            _inputFileName = Path.GetFileName(_inputPath);

            var defaultColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine(
                $"+-~-+-~-+-~-+-~-+-~-+-~-+   START SLICING PIZZA {_inputFileName}  +-~-+-~-+-~-+-~-+-~-+-~-+");
            Console.WriteLine();
            Console.ForegroundColor = defaultColor;

            WritePizzaInfoToConsole();

            StartSlicing();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine(
                $"+-~-+-~-+-~-+-~-+-~-+-~-+ FINISHED SLICING PIZZA {_inputFileName} +-~-+-~-+-~-+-~-+-~-+-~-+");
            Console.WriteLine();
            Console.ForegroundColor = defaultColor;

            return output;
        }

        private static void StartSlicing()
        {
            HorizontalOptimizedSlicing(_pizza.Clone(), string.Empty);

            VerticalOptimizedSlicing(_pizza.Clone(), string.Empty);

            CombinedHorizontalThenVerticalSlicing(_pizza.Clone());
        }

        private static void CombinedHorizontalThenVerticalSlicing(Pizza clone)
        {
            var pizza = _pizza.Clone();
            var successfulSlices = HorizontalOptimizedSlicing(pizza, "combined-HV-");

            successfulSlices.AddRange(VerticalOptimizedSlicing(pizza, "combined-HV-"));

            GenerateOutputFile(successfulSlices, "combined-HV");
        }

        private static List<string> HorizontalOptimizedSlicing(Pizza pizza, string prefix)
        {
            var operationName = prefix + "horizontal-optimized";
            Console.WriteLine();
            Console.WriteLine($"-----~-+-< STARTING OPERATION [{operationName.ToUpper()}] >-+-~-----");
            Console.WriteLine();

            var higherColumnIndex = pizza.ColumnsPizzaCount - 1;
            var totalSlicesSize = 0;

            var successfulSlices = new List<string>();
            //horizontal linear
            for (var row = 0; row < pizza.RowsPizzaCount; row++)
            {
                var location = 0;
                var leftCells = higherColumnIndex + 1;

                while (leftCells >= pizza.CellsMinInSlice)
                {
                    var sliceHaveBeenMade = false;
                    //trying minimum slice length
                    for (var lengthSlice = pizza.CellsMinInSlice; lengthSlice <= pizza.CellsMaxInSlice; lengthSlice++)
                    {
                        //check if the slice-to-be-cut is out of the pizza
                        var currentHigherIndex = location + lengthSlice - 1;
                        if (currentHigherIndex > higherColumnIndex) break;

                        var slicePizza = CreateHorizontalSlice(row, location, lengthSlice, ref pizza);
                        if (slicePizza.IsValidSlice)
                        {
                            location = slicePizza.EndPoint.Y + 1;
                            pizza.CutSlice(slicePizza);
                            totalSlicesSize += slicePizza.Size;
                            successfulSlices.Add(
                                slicePizza.StartPoint.X + " " +
                                slicePizza.EndPoint.X + " " +
                                slicePizza.StartPoint.Y + " " +
                                slicePizza.EndPoint.Y + " ");
                            sliceHaveBeenMade = true;
                            break;
                        }
                    }

                    if (!sliceHaveBeenMade)
                        location++; // shiftting location of slicing by one-cell only
                    leftCells = higherColumnIndex - location + 1;
                }
            }

            GenerateOutputFile(successfulSlices, operationName);
            DrawPizzaTo2X2(pizza, operationName);
            WritePizzaContent(pizza, operationName);

            var pizzaSize = pizza.ColumnsPizzaCount * pizza.RowsPizzaCount;
            Console.WriteLine(
                $"Optimized Horizontal Slicing Finished, total cells had been cut from the mother-pizza: {totalSlicesSize} of {pizzaSize}");
            var slicedRatio = ((float) totalSlicesSize / (float) pizzaSize) * 100;
            Console.WriteLine(
                $"Pizza left-over: {pizzaSize - totalSlicesSize}, Sliced %: {slicedRatio}");
            Console.WriteLine();
            Console.WriteLine($"-----~-+-< FINISHED OPERATION [{operationName.ToUpper()}] >-+-~-----");

            return successfulSlices;
        }

        private static SlicePizza CreateHorizontalSlice(int row, int startLocation, int length, ref Pizza pizza)
        {
            var endLocation = startLocation + length - 1;
            var startPoint = new Point(row, startLocation);
            var endPoint = new Point(row, endLocation);

            var slicePizza = new SlicePizza(pizza, startPoint, endPoint);
            return slicePizza;
        }

        private static List<string> VerticalOptimizedSlicing(Pizza pizza, string prefix)
        {
            var operationName = prefix + "vertical-optimized";
            Console.WriteLine();
            Console.WriteLine($"-----~-+-< STARTING OPERATION [{operationName.ToUpper()}] >-+-~-----");
            Console.WriteLine();

            var higherRowIndex = pizza.RowsPizzaCount - 1;
            var totalSlicesSize = 0;

            var successfulSlices = new List<string>();
            //horizontal linear
            for (var column = 0; column < pizza.ColumnsPizzaCount; column++)
            {
                var location = 0;
                var leftCells = higherRowIndex + 1;

                while (leftCells >= pizza.CellsMinInSlice)
                {
                    var sliceHaveBeenMade = false;
                    //trying minimum slice length
                    for (var lengthSlice = pizza.CellsMinInSlice; lengthSlice <= pizza.CellsMaxInSlice; lengthSlice++)
                    {
                        //check if the slice-to-be-cut is out of the pizza
                        var currentHigherIndex = location + lengthSlice - 1;
                        if (currentHigherIndex > higherRowIndex) break;

                        var slicePizza = CreateVerticalSlice(column, location, lengthSlice, pizza);
                        if (slicePizza.IsValidSlice)
                        {
                            location = slicePizza.EndPoint.X + 1;
                            pizza.CutSlice(slicePizza);
                            totalSlicesSize += slicePizza.Size;

                            successfulSlices.Add(
                                slicePizza.StartPoint.X + " " +
                                slicePizza.EndPoint.X + " " +
                                slicePizza.StartPoint.Y + " " +
                                slicePizza.EndPoint.Y + " ");

                            sliceHaveBeenMade = true;
                            break;
                        }
                    }

                    if (!sliceHaveBeenMade)
                        location++; // shiftting location of slicing by one-cell only
                    leftCells = higherRowIndex - location + 1;
                }
            }

            GenerateOutputFile(successfulSlices, operationName);
            DrawPizzaTo2X2(pizza, operationName);
            WritePizzaContent(pizza, operationName);

            var pizzaSize = pizza.ColumnsPizzaCount * pizza.RowsPizzaCount;
            Console.WriteLine(
                $"Optimized Vertical Slicing Finished, total cells had been cut from the mother-pizza: {totalSlicesSize} of {pizzaSize}");
            var slicedRatio = ((float) totalSlicesSize / (float) pizzaSize) * 100;
            Console.WriteLine(
                $"Pizza left-over: {pizzaSize - totalSlicesSize}, Sliced %: {slicedRatio}");
            Console.WriteLine();
            Console.WriteLine($"-----~-+-< FINISHED OPERATION [{operationName.ToUpper()}] >-+-~-----");

            return successfulSlices;
        }

        private static void WritePizzaContent(Pizza pizza, string operationName)
        {
            var pathToSave = PathToSave("sliced-pizza-" + operationName, ".out");

            File.WriteAllText(pathToSave, string.Empty);

            for (int i = 0; i < pizza.RowsPizzaCount; i++)
            {
                var lineContent = string.Empty;
                for (int j = 0; j < pizza.ColumnsPizzaCount; j++)
                {
                    lineContent += pizza.Content[i, j];
                }

                File.AppendAllText(pathToSave, lineContent + Environment.NewLine);
            }
        }

        private static SlicePizza CreateVerticalSlice(int column, int startLocation, int length, Pizza pizza)
        {
            var endLocation = startLocation + length - 1;
            var startPoint = new Point(startLocation, column);
            var endPoint = new Point(endLocation, column);

            var slicePizza = new SlicePizza(pizza, startPoint, endPoint);
            return slicePizza;
        }

        private static void GenerateOutputFile(List<string> successfulSlices, string operationName)
        {
            Console.WriteLine($"Generating output file of operation: {operationName}");
            var outputFileName = PathToSave(operationName, ".out");
            
            File.WriteAllText(outputFileName, successfulSlices.Count + Environment.NewLine);
            File.AppendAllLines(outputFileName, successfulSlices);

            Console.WriteLine($"Saving Output file of operation: {operationName}, to {outputFileName}");
        }

        private static void DrawPizzaTo2X2(Pizza pizza, string operationName)
        {
            Console.WriteLine($"Painting operation: {operationName}");
            using (var bitmap = new Bitmap(
                pizza.RowsPizzaCount * 2,
                pizza.ColumnsPizzaCount * 2))
            {
                using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                {
                    for (var row = 0; row < pizza.RowsPizzaCount; row++)
                    {
                        for (var column = 0; column < pizza.ColumnsPizzaCount; column++)
                        {
                            var ingredientColor = IngredientColor(pizza, row, column);

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

        private static Color IngredientColor(Pizza pizza, int row, int column)
        {
            Color ingredientColor;
            switch (pizza.Content[row, column])
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
                    Content = (char[,]) Content.Clone()
                };
            }
        }
    }
}