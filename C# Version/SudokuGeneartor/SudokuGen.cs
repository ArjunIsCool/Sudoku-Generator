using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



namespace SudokuGen
{    
    public class Program
    {
        public static List<List<int>> grid = new List<List<int>>();
        public static List<List<int>> solvedGrid = new List<List<int>>();
        public static List<List<int>> experimentGrid = new List<List<int>>();
        static System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        static List<List<List<int>>> gridData = new List<List<List<int>>>();

        //static XmlSerializer serializer = new XmlSerializer(typeof(string));

        static bool finishedGeneration;
        static bool finishedWork = false;

        //static bool finishedStream;

        static int attempts = 5;
        static int maxGaps = 20;

        static int successTimes;

        static int noOfGaps;

        static int maxDesiredGaps;

        static int noOfSolutions;

        static int noOfPuzzles;
        static int totalNoOfPuzzles;

        static int streamNo = 1;

        static int shuffleTimes;
        static int desiredShuffleTimes;

        //static int starStruck;

        public static List<int> numList = new List<int>();
        

        
        public static void Main(string[] args)
        {
           //Stream lastTimeSave = File.Open("DontDeleteThisOrYouLoseItAll.dat",FileMode.OpenOrCreate);
            try
            {
                List<int> saveDat = JsonDeserialize<List<int>>(typeof(List<int>), "DontDeleteThisOrYouLoseItAll.dat");
                streamNo = saveDat[0] + 1;
                totalNoOfPuzzles = saveDat[1];
                Console.WriteLine("Safely remembered previous session :)");
            }
            catch (System.Exception)
            {
                streamNo = 1;
            }
            //stream = File.Open("SudokuPuzzles" + streamNo + ".dat",FileMode.OpenOrCreate);
            for (int i = 1; i < 10; i++)
            {
                numList.Add(i);
            }
            StartProgram();
            
        }

        public static void JsonSerialize(object data, string filePath)
        {
            JsonSerializer serializer = new JsonSerializer();
            //if(!File.Exists(filePath)) {File.Create(filePath).Close();}
            Stream stream = File.Open(filePath,FileMode.Create);
            //StreamWriter sw = new StreamWriter(stream);
            //JsonWriter jsonWriter = new JsonTextWriter(sw);
            using(StreamWriter sw = new StreamWriter(stream))
            {
                serializer.Serialize(sw,data);
            }
            //serializer.Serialize(jsonWriter,data);
            //jsonWriter.Close();
            //sw.Close();
            stream.Close();
        }

        public static T JsonDeserialize<T>(Type type,string filePath)
        {
            //JObject obj = null;
            //JsonSerializer serializer = new JsonSerializer();
            //if(!File.Exists(filePath)) {File.Create(filePath).Close();}

            var serializer = new JsonSerializer();
            try{

                Stream stream = File.Open(filePath,FileMode.Open);
                using(StreamReader sr = new StreamReader(stream))
                {
                    return (T)serializer.Deserialize(sr, typeof(T));
                }

            } catch(System.Exception)
            {
                return default(T);
                throw;
            }

            //obj = serializer.Deserialize(jsonReader) as JObject;
            //StreamReader sr = new StreamReader(filePath);
            //JsonReader jsonReader = new JsonTextReader(sr);
            //obj = serializer.Deserialize(jsonReader) as JObject;
            //jsonReader.Close();
            //sr.Close();

        }


        public static void StartProgram()
        {
            //DAMN THIS FUNCTION IS COMPLEX
            watch.Reset();
            int diff = 0;
            Console.WriteLine("Choose Difficulty: 1)Easy(fastest) 2)Medium 3)Hard 4)Extreme(slowest)");
            try
            {
                diff = Convert.ToInt32(Console.ReadLine());
                switch (diff)
                {
                    case 1:
                        maxDesiredGaps = 20;
                        desiredShuffleTimes = 10;
                        break;
                    case 2:
                        maxDesiredGaps = 40;
                        desiredShuffleTimes = 30;
                        break;
                    case 3:
                        maxDesiredGaps = 60;
                        desiredShuffleTimes = 50;
                        break;
                    case 4:
                        maxDesiredGaps = 80;
                        desiredShuffleTimes = 70;
                        break;
                    default:
                        Console.WriteLine("Type either 1,2,3,4 to choose difficulty!");
                        StartProgram();
                        break;
                }
            }
            catch (System.Exception)
            {
                Console.WriteLine("Type either 1,2,3,4 to choose difficulty!");
                StartProgram();
                //return;
                throw;
            }
            noOfPuzzles = 0;
            ChooseNoOfPuzzles();
            watch.Start();
            for (int i = 0; i < noOfPuzzles; i++)
            {
                //Console.WriteLine("Current Board: " +i);
                grid = new List<List<int>>();
                InitializeEmptyGrid(grid);
                solvedGrid = new List<List<int>>(grid);
                shuffleTimes = desiredShuffleTimes;
                finishedGeneration = false;
                GenerateSudoku();
                totalNoOfPuzzles++;
                maxGaps = maxDesiredGaps;
                RemoveNumbers();
            }
            watch.Stop();
            if(gridData.Count < 10000 && gridData.Count > 0)
            {
                JsonSerialize(gridData,"SudokuPuzzles" + streamNo + ".dat");
            } else 
            {
                JsonSerialize(gridData,"SudokuPuzzles" + streamNo + ".dat");
                streamNo++;
                gridData.Clear();
            }
            // Stream streamNoSave = File.Open("DontDeleteThisOrYouLoseItAll.dat",FileMode.OpenOrCreate);
            List<int> saveDatList = new List<int>();
            saveDatList.Add(streamNo);
            saveDatList.Add(totalNoOfPuzzles);
            // bf.Serialize(streamNoSave,saveDatList);
            // streamNoSave.Close();
            JsonSerialize(saveDatList,"DontDeleteThisOrYouLoseItAll.dat");
            Console.WriteLine("Process took " + watch.ElapsedMilliseconds + " milliseconds");
            Console.WriteLine("You generated a total of " + totalNoOfPuzzles + " puzzles!");
            //Console.WriteLine("STAR STRUCKED " + starStruck + " TIMES!?");
            Console.WriteLine("More puzzles? Type DUP to check for duplicates. WARNING: System CAN GET REKT");
            Console.WriteLine("Type SEE to print all your generated puzzles one by one");
            string theInput = Console.ReadLine();
            if(theInput == "DUP")
            {
                watch.Reset();
                watch.Start();
                int totalDuplicates = 0;
                int noOfDuplicates = 0;
                List<List<List<List<int>>>> listOfFiles = new List<List<List<List<int>>>>();
                for (int i = 1; i < streamNo+1; i++)
                {
                    var tempList = JsonDeserialize<List<List<List<int>>>>(typeof(List<List<List<int>>>),"SudokuPuzzles" + i + ".dat");
                    if(tempList != null)
                    {
                        listOfFiles.Add(tempList);
                    }
                }

                int datFile = 0;

                List<List<List<List<int>>>> newlistOfFiles = new List<List<List<List<int>>>> (listOfFiles);
                foreach (List<List<List<int>>> item1 in listOfFiles)
                {
                    datFile++;
                    foreach (List<List<int>> grid1 in item1)
                    {
                        noOfDuplicates = 0;
                        foreach (List<List<List<int>>> item2 in listOfFiles)
                        {
                            List<List<List<int>>> newItem = new List<List<List<int>>> (newlistOfFiles[newlistOfFiles.IndexOf(item2)]);
                            foreach (List<List<int>> grid2 in item2)
                            {
                                List<List<int>> newGrid = new List<List<int>> (newItem[newItem.IndexOf(grid2)]);
                                if(CheckDuplicate(grid1, grid2))
                                {
                                    noOfDuplicates++;
                                    if(noOfDuplicates > 1)
                                    {
                                        
                        
                                        Console.WriteLine("We reached here?");
                                        List<List<List<int>>> editedItem = newlistOfFiles[newlistOfFiles.IndexOf(item2)];

                                        editedItem.Remove(newGrid);
                                        
                                        totalDuplicates++;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    JsonSerialize(newlistOfFiles,"SudokuPuzzles" + datFile + ".dat");
                }
                watch.Stop();
                listOfFiles.Clear();
                if(totalDuplicates > 0)
                {
                    Console.WriteLine("Removed " + totalDuplicates + " duplicates in " + watch.ElapsedMilliseconds + " milliseconds! FACT: There is 0.000000034 chance out of 1 to get a duplicate!");
                } else {
                    Console.WriteLine("No Duplicates! All is Well!");
                }
            } else if(theInput == "SEE")
            {
                watch.Reset();
                watch.Start();
                int total = 0;
                File.Create("RawPuzzlesData.txt").Close();
                using (StreamWriter writer = new StreamWriter("RawPuzzlesData.txt", true))
                {
                    for (int i = 1; i < streamNo+1; i++)
                    {
                        //StreamWriter writer = new StreamWriter("RawPuzzlesData.txt",true);
                        //Stream stream1 = File.Open("SudokuPuzzles" + i + ".dat",FileMode.Open, FileAccess.Read);
                        //stream = File.Open("SudokuPuzzles" + i + ".dat",FileMode.Open, FileAccess.Read);
                        //stream1.Seek(0,SeekOrigin.Begin);
                        //List<List<List<int>>> storedGrids = (List<List<List<int>>>)bf.Deserialize(stream1);
                        List<List<List<int>>> storedGrids = JsonDeserialize<List<List<List<int>>>>(typeof(List<List<List<int>>>),"SudokuPuzzles" + i + ".dat");
                        if(storedGrids == null)
                        {
                            break;
                        }
                            foreach (List<List<int>> item in storedGrids)
                            {
                                PrintGrid(item);
                                Console.WriteLine(" ");
                                Console.WriteLine("__________________");

                                for (int y = 0; y < 9; y++)
                                {
                                    string row = "";
                                    for (int x = 0; x < 9; x++)
                                    {
                                        row += item[y][x] + " ";
                                    }
                                    writer.WriteLine(row);

                                }
                                total++;
                                writer.WriteLine("__________________");
                            }
                        
                    }
                }
                watch.Stop();
                Console.WriteLine("Printed " + total + " puzzles in " + watch.ElapsedMilliseconds + " milliseconds");
            }

            StartProgram();
        }

        public static bool CheckDuplicate(List<List<int>> grid1, List<List<int>> grid2)
        {
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if(grid1[y][x] != grid2[y][x])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static void CreateUniquePuzzle()
        {
            GenerateSudoku();
            //Yeah who knows why this person created a second function for nothing XD
        }

        public static void ChooseNoOfPuzzles()
        {
            Console.WriteLine("How many puzzles to generate?");
            try
            {
                noOfPuzzles = Convert.ToInt32(Console.ReadLine());
            }
            catch (System.Exception)
            {
                Console.WriteLine("Type a number please...");
                ChooseNoOfPuzzles();
                return;
                throw;
            }
        }

        public static void InitializeEmptyGrid(List<List<int>> theGrid)
        {
            for (int y = 0; y < 9; y++)
            {
                theGrid.Add(new List<int>());
                //Console.WriteLine("");
                for (int x = 0; x < 9; x++)
                {
                    theGrid[y].Add(0);
                    //Console.Write(theGrid[y][x] + ", ");
                }
            }
        }

        public static void PrintGrid(List<List<int>> theGrid)
        {
            for (int y = 0; y < 9; y++)
            {
                Console.WriteLine("");
                for (int x = 0; x < 9; x++)
                {
                    if(theGrid[y][x] == 0)
                    {
                        Console.Write(" " + " ");
                    } else {
                        Console.Write(theGrid[y][x] + " ");
                    }
                }
            }
        }

        public static bool CheckPossible(List<List<int>> theGrid, int n, int x, int y)
        {
            for (int i = 0; i < 9; i++)
            {
                if(grid[y][i] == n)
                    return false;
            }
            for (int i = 0; i < 9; i++)
            {
                if(grid[i][x] == n)
                    return false;
            }
            int y0 = (y / 3) * 3;
            int x0 = (x / 3) * 3;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if(theGrid[y0 + i][x0 + j] == n)
                        return false;
                }
            }
            return true;
        }
        public static void GenerateSudoku()
        {
            //Console.WriteLine("Generating");
            shuffleTimes--;
            if(shuffleTimes > 0)
            {
                Shuffle(numList);
            }
            
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if(solvedGrid[y][x] == 0)
                    {
                        List<int> myNumList = numList;
                        for (int n = 0; n < 9; n++)
                        {
                            if(CheckPossible(solvedGrid,myNumList[n],x,y))
                            {
                                solvedGrid[y][x] = numList[n];
                                GenerateSudoku();
                                if(finishedGeneration)
                                    return;
                                solvedGrid[y][x] = 0;
                            }
                        }
                        return;
                    }
                }
            }
            //PrintGrid(solvedGrid);
            //noOfPuzzles++;
            //Console.WriteLine("We completed generation work");
            if(finishedGeneration == false)
            {
                finishedGeneration = true;
                //Grid newGrid = new Grid(solvedGrid);
                gridData.Add(solvedGrid);
                //Console.WriteLine("Generation done!");
                //Console.WriteLine("GridCount: " + gridData.Count);
                grid = new List<List<int>>(solvedGrid);
            }
        }

        public static void Shuffle(List<int> theList)  
        {  
            Random rng = new Random();
            int n = theList.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                int value = theList[k];  
                theList[k] = theList[n];  
                theList[n] = value;  
            }  
        }

        public static void RemoveNumbers()
        {
            //Console.WriteLine("Removing");
            //Console.WriteLine(maxGaps);
            Random rand = new Random();
            int row = rand.Next(0,9);
            int col = rand.Next(0,9);
            while (grid[row][col] == 0)
            {
                row = rand.Next(0,9);
                col = rand.Next(0,9);
            }
            noOfGaps = 0;
            FindNoOfGaps();
            //Console.WriteLine("Gaps: " + noOfGaps);
            if(noOfGaps < maxGaps)
            {
                int oldValue = grid[row][col];
                grid[row][col] = 0;
                noOfSolutions = 0;
                finishedWork = false;
                //Console.WriteLine("Gonna check..");
                experimentGrid = new List<List<int>>(grid);
                SolveSudoku();
                if(noOfSolutions == 1)
                {
                    successTimes++;
                    //Console.WriteLine("Success " + successTimes);
                    RemoveNumbers();
                } else {
                    //Console.WriteLine("Fail");
                    attempts--;
                    grid[row][col] = oldValue;
                    if(attempts <= 0)
                    {
                        attempts = 1;
                        maxGaps--;
                        RemoveNumbers();
                    } else {
                        RemoveNumbers();
                    }
                }
            } else
            {
                //PrintGrid(grid);
                //PrintGrid(solvedGrid);
                noOfGaps = 0;
                FindNoOfGaps();
                //Console.WriteLine("There are " + noOfGaps + " gaps");
                
            }

        }

        public static bool CheckGrid(List<List<int>> theGrid)
        {
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if(theGrid[y][x] == 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static void FindNoOfGaps()
        {
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if(grid[y][x] == 0)
                        noOfGaps++;
                }
            }
        }

        public static void SolveSudoku()
        {
            //Console.WriteLine("Solving");
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if(experimentGrid[y][x] == 0)
                    {
                        for (int n = 1; n < 10; n++)
                        {
                            if(CheckPossible(experimentGrid,n,x,y))
                            {
                                experimentGrid[y][x] = n;
                                SolveSudoku();
                                if(finishedWork)
                                    return;
                                experimentGrid[y][x] = 0;
                            }
                        }
                        return;
                    }
                }
            }
            if(CheckGrid(experimentGrid))
            {
                noOfSolutions++;
                if(noOfSolutions > 1)
                    finishedWork = true;
                    return;
            }
        }
    }
}
