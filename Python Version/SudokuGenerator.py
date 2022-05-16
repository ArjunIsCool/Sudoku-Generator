from random import shuffle,randint
import copy
import time

grid = [[0,0,0,0,0,0,0,0,0],
        [0,0,0,0,0,0,0,0,0],
        [0,0,0,0,0,0,0,0,0],
        [0,0,0,0,0,0,0,0,0],
        [0,0,0,0,0,0,0,0,0],
        [0,0,0,0,0,0,0,0,0],
        [0,0,0,0,0,0,0,0,0],
        [0,0,0,0,0,0,0,0,0],
        [0,0,0,0,0,0,0,0,0]]

gridSolved = [[5,3,0,0,7,0,0,0,0],
        [6,0,0,1,9,5,0,0,0],
        [0,9,8,0,0,0,0,6,0],
        [8,0,0,0,6,0,0,0,3],
        [4,0,0,8,0,3,0,0,1],
        [7,0,0,0,2,0,0,0,6],
        [0,6,0,0,0,0,2,8,0],
        [0,0,0,4,1,9,0,0,5],
        [0,0,0,0,8,0,0,7,9]]

experimentGrid = [[0,0,0,0,0,0,0,0,0],
                  [0,0,0,0,0,0,0,0,0],
                  [0,0,0,0,0,0,0,0,0],
                  [0,0,0,0,0,0,0,0,0],
                  [0,0,0,0,0,0,0,0,0],
                  [0,0,0,0,0,0,0,0,0],
                  [0,0,0,0,0,0,0,0,0],
                  [0,0,0,0,0,0,0,0,0],
                  [0,0,0,0,0,0,0,0,0]]

attempts = 5
finishedGeneration = False

row = 0
col = 0
counter = 0

numList = [1,2,3,4,5,6,7,8,9]

puzzles = 0

aGrid = []
totalDiscoveredPuzzles = 0

gridData = {}

newPuzzleTries = 100

#for y in range(0,9):
#    print(grid[y])

def startWork():
    global grid, gridSolved, experimentGrid, emptyGrid, aGrid, totalDiscoveredPuzzles
    global attempts, counter, finishedGeneration, row, col, numList, puzzles, newPuzzleTries
    print("Choose difficulty : 1: Easy 2: Medium 3: Hard")
    diff = input()
    if(diff == "1"):
        attempts = 1
    elif(diff == "2"):
        attempts = 5
    elif(diff == "3"):
        attempts = 10
    else:
        print("Type either 1, 2 or 3 for changing difficulty!")
        startWork()
        return
    decideNoOfPuzzles()
    
    grid = [[0 for _ in range(9)] for _ in range(9)]
    finishedGeneration = False
    row = 0
    col = 0
    counter = 0
    numList = [1,2,3,4,5,6,7,8,9]
    startTime = time.time()
    for i in range(0,puzzles):
        generateSudoku()
        for j in range(1,totalDiscoveredPuzzles+1):
            if(gridData["grid" + str(j)] == grid):
                if(newPuzzleTries == 0):
                    print("I believe we cannot print any more new sudoku puzzles of this type. Crazy? " + totalDiscoveredPuzzles + " puzzles have been discovered!")
                    newPuzzleTries = 100
                    decision = input("Interested in printing all the solved versions? Type yes to do so or anything else to terminate")
                    if(decision == "yes"):
                        for l in range(1,totalDiscoveredPuzzles+1):
                            printSudoku(gridData["grid"+str(l)])
                            print("__________________________")
                    else:
                        startWork()
                        return
                print("We were almost about to print a previously discovered sudoku! The only difference might have been the number of empty spots and their positions. But then, that would not be a truly unique sudoku puzzle. So im too lazy to worry about it!")
                newPuzzleTries -= 1
                generateSudoku()
                break
        totalDiscoveredPuzzles += 1
        gridData["grid" + str(totalDiscoveredPuzzles)] = copy.deepcopy(gridSolved)
        removeNumber()
        print("__________________")
        grid = [[0 for _ in range(9)] for _ in range(9)]
        row = 0
        col = 0
        counter = 0
        numList = [1,2,3,4,5,6,7,8,9]
        finishedGeneration = False
        attempts = int(diff)
    endTime = time.time()
    totalTime = endTime - startTime
    print("Process took " + str(totalTime) + " seconds")
    print("We have discovered: " + str(totalDiscoveredPuzzles) + " puzzles in total!")
    input("More puzzles?")
    startWork()

def decideNoOfPuzzles():
    global puzzles
    print("How many puzzles to generate?")
    puzzles = input()
    try:
        puzzles = int(puzzles)
    except ValueError:
        print("Wrong input. Type a number please")
        decideNoOfPuzzles()

def removeNumber():
    global attempts, row, col
    global experimentGrid, emptyGrid, grid
    global counter
    row = randint(0,8)
    col = randint(0,8)
    while(grid[row][col] == 0):
        row = randint(0,8)
        col = randint(0,8)
    oldValue = grid[row][col]
    grid[row][col] = 0
    experimentGrid = grid
    counter = 0
    solveSudoku()
    if(counter == 1):
        removeNumber()
    else:
        print("EHHHHHHHHHHHH")
        attempts -= 1
        grid[row][col] = oldValue
        if(attempts > 0):
            removeNumber()
        else:
            #print("___________________")
            printSudoku(grid)


def printSudoku(theGrid):
    for y in range(9):
        for x in range(9):
            if(theGrid[y][x] == 0):
                print(" , ", end = "")
            else:
                print(str(theGrid[y][x]) + ", ", end = "")
        print("")


def generateSudoku():
    global finishedGeneration
    global grid, gridSolved
    global numList
    for y in range(9):
        for x in range(9):
            if(grid[y][x] == 0):
                shuffle(numList)
                for n in numList:
                    if(checkpossible(n,x,y)):
                        grid[y][x] = n
                        generateSudoku()
                        if(finishedGeneration):
                            return
                        grid[y][x] = 0
                return
    if(finishedGeneration == False):
        gridSolved = copy.deepcopy(grid)
        finishedGeneration = True
        #for y in range(9):
            #print(gridSolved[y])
    

def checkpossible(n,x,y):
    global grid
    for i in range(0,9):
        if(grid[i][x] == n):
            return False
    for i in range(0,9):
        if(grid[y][i] == n):
            return False
    x1 = (x//3)*3
    y1 = (y//3)*3

    for i in range(3):
        for j in range(3):
            if(grid[y1 + i][x1 + j] == n):
                return False
    return True

def solveSudoku():
    global counter
    global experimentGrid
    for y in range(9):
        for x in range(9):
            if(experimentGrid[y][x] == 0):
                for n in range(1,10):
                    if(checkpossible(n,x,y)):
                        experimentGrid[y][x] = n
                        solveSudoku()
                        experimentGrid[y][x] = 0
                return
    counter += 1




startWork()




    
