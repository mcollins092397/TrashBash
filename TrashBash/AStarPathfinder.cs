using System;
using System.Collections.Generic;
using System.Text;

namespace TrashBash
{
    public class pPair
    {
        public double f;
        public (int, int) index;
        public pPair(double f, (int, int) index)
        {
            this.f = f;
            this.index = index;
        }
    }


    public class AStarPathfinder
    {
        //grid representing the game world
        private int[,] grid;
        //grid size rows
        private static int totalRows;
        //grid size col
        private static int totalCols;

        public struct cell
        {
            public int parentI, parentJ;

            public double f, g, h;
        }

        //private cell[,] cellDetails = new cell[totalRows, totalCols];

        /// <summary>
        /// constructor for pathfinder 
        /// </summary>
        /// <param name="grid">grid the pathfinder works on</param>
        public AStarPathfinder(int[,] grid)
        {
            this.grid = grid;
            totalCols = grid.GetLength(1);
            totalRows = grid.GetLength(0);
        }

        /// <summary>
        /// A Utility Function to check whether given cell (row, col) is valid
        /// </summary>
        /// <param name="row">row of cell to check</param>
        /// <param name="col">col of cell to check</param>
        /// <returns>bool with value of true if cell is valid and false otherwise</returns>
        private bool isValid(int row, int col)
        {
            return (row >= 0) && (row < totalRows) && (col >= 0) && (col < totalCols);
        }

        /// <summary>
        /// A Utility Function to check whether the given cell is blocked or not
        /// </summary>
        /// <param name="row">row of cell to check</param>
        /// <param name="col">col of cell to check</param>
        /// <returns>bool with value of true if cell is unblocked and false otherwise</returns>
        private bool isUnblocked(int row, int col)
        {
            if (grid[row, col] == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// A Utility Function to check whether destination cell has been reached or not
        /// </summary>
        /// <param name="row">row of cell to check</param>
        /// <param name="col">col of cell to check</param>
        /// <param name="destinationRow">destination cell row</param>
        /// <param name="destinationCol">destination cell col</param>
        /// <returns>bool with value of true if cell is destination and false otherwise</returns>
        private bool isDestination(int row, int col, int destinationRow, int destinationCol)
        {
            if (row == destinationRow && col == destinationCol)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// A Utility Function to calculate the 'h' heuristics.
        /// </summary>
        /// <param name="row">row of the cell to calculate h value of</param>
        /// <param name="col">col of the cell to calculate h value of</param>
        /// <param name="destinationRow">destination cell row</param>
        /// <param name="destinationCol">destination cell col</param>
        /// <returns>the h value of cell [row,col]</returns>
        private double calculateHValue(int row, int col, int destinationRow, int destinationCol)
        {
            System.Diagnostics.Debug.WriteLine("\nThe path is: ");
            double dx = Math.Abs(col - destinationCol);
            double dy = Math.Abs(row - destinationRow);

            return (1 * (dx + dy) + (Math.Sqrt(2) - 2 * 1) * Math.Min(dx, dy));
        }

        private void tracePath(cell[,] cellDetails, int destinationRow, int destinationCol)
        {
            int row = destinationRow;
            int col = destinationCol;
            Stack<(int, int)> Path = new Stack<(int, int)>();

            while (!(cellDetails[row, col].parentI == row && cellDetails[row, col].parentJ == col))
            {
                Path.Push((row, col));
                int tempRow = cellDetails[row, col].parentI;
                int tempCol = cellDetails[row, col].parentJ;
                row = tempRow;
                col = tempCol;
            }

            Path.Push((row, col));
            while(Path.Count != 0)
            {
                (int, int) p = Path.Pop();
                System.Diagnostics.Debug.Write("-> " + p.Item1 + " " + p.Item2);
            }

            return;
        }

        public void aStarSearch(int startCol, int startRow, int destinationCol, int destinationRow)
        {
            if (isValid(startRow, startCol) == false)
            {
                System.Diagnostics.Debug.WriteLine("Source is invalid");
                return;
            }

            if (isValid(destinationRow, destinationCol) == false)
            {
                System.Diagnostics.Debug.WriteLine("Destination is invalid");
                return;
            }

            if(isUnblocked(startRow, startCol) == false || isUnblocked(destinationRow, destinationCol) == false)
            {
                System.Diagnostics.Debug.WriteLine("Source or Destination is blocked");
                return;
            }

            if(isDestination(startRow, startCol, destinationRow, destinationCol) == true)
            {
                System.Diagnostics.Debug.WriteLine("Already at destination");
                return;
            }

            bool[,] closedList = new bool[totalRows, totalCols];

            cell[,] cellDetails = new cell[totalRows, totalCols];

            int i, j;

            for(i = 0; i < totalRows; i++)
            {
                for(j = 0; j < totalCols; j++)
                {
                    cellDetails[i, j].f = double.MaxValue;
                    cellDetails[i, j].g = double.MaxValue;
                    cellDetails[i, j].h = double.MaxValue;
                    cellDetails[i, j].parentI = -1;
                    cellDetails[i, j].parentJ = -1;
                }
            }

            i = startRow;
            j = startCol;

            cellDetails[i, j].f = 0.0;
            cellDetails[i, j].g = 0.0;
            cellDetails[i, j].h = 0.0;
            cellDetails[i, j].parentI = i;
            cellDetails[i, j].parentJ = j;

            List<pPair> openList = new List<pPair>();

            openList.Add(new pPair(0.0, (i, j)));

            bool foundDest = false;

            while(openList.Count != 0)
            {
                pPair p = openList[0];
                openList.Remove(p);

                i = p.index.Item1;
                j = p.index.Item2;
                closedList[i, j] = true;
            }
        }

        /*
           A* start
           f: sum of g and h
           g: movement cost to move from the starting point to a given square on the grid following the path generated to get there
           h: estimated movement cost to move from that given square on the grid to the final destination. Known as heuristic (smart guess) 

           steps:

           initialize the open list

           initialize the closed list

           put starting node on closed list and leave its f at zero

           while open list is not empty
               find node with the least f on the open list, call it q

               pop q from the open list

               generate q's 8 successors (the squares around q) and set their parents to q

               for each successor
                   if successor is goal 
                       stop search

                   else
                       compute both g and h for successor
                       successor.g = q.g + distance between successor and q (always 1 for my grid)
                       successor.h = distance from goal to successor (using heuristic)
                       successor.f = successor.g + successor.h

                   if a node with the same position as successor is in the open list with a lower f than successor
                       skip this successor

                   if a node with the same position as successor is in the closed list which has a lower f than successor
                       skip this successor

                   else
                       add the node to the open list

               end for loop

           push q on the closed list

           end while loop


           heuristic:
           dx = abs(current_cell.x – goal.x)
           dy = abs(current_cell.y – goal.y)

           h = D * (dx + dy) + (D2 - 2 * D) * min(dx, dy)

           where D is length of each node(usually = 1) and D2 is diagonal distance between each node (usually = sqrt(2) ). 

           */
    }
}
