using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

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

        StringBuilder sb = new StringBuilder();

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
            //System.Diagnostics.Debug.WriteLine("\nThe path is: ");
            double dx = Math.Abs(col - destinationCol);
            double dy = Math.Abs(row - destinationRow);

            return (1 * (dx + dy) + (Math.Sqrt(2) - 2 * 1) * Math.Min(dx, dy));
        }

        private Stack<(int, int)> tracePath(cell[,] cellDetails, int destinationRow, int destinationCol)
        {
            int row = destinationRow;
            int col = destinationCol;
            Stack<(int, int)> Path = new Stack<(int, int)>();
            Stack<(int, int)> temp = new Stack<(int, int)>();

            while (row >= 0 && col >= 0 && !(cellDetails[row, col].parentI == row && cellDetails[row, col].parentJ == col))
            {
                Path.Push((row, col));
                temp.Push((row, col));
                
                int tempRow = cellDetails[row, col].parentI;
                int tempCol = cellDetails[row, col].parentJ;
                row = tempRow;
                col = tempCol;
            }

            Path.Push((row, col));
            temp.Push((row, col));

            while (Path.Count != 0)
            {
                (int, int) p = Path.Pop();
                sb.Append("-> " + p.Item2 + " " + p.Item1);
                
            }
            //MessageBox.Show(sb.ToString());
            return temp;
        }

        public Stack<(int, int)> aStarSearch(int startX, int startY, int endX, int endY)
        {
            if (isValid(startY, startX) == false)
            {
                //MessageBox.Show("Source is invalid");
                return null;
            }

            if (isValid(endY, endX) == false)
            {
                //MessageBox.Show("Destination is invalid");
                return null;
            }

            if (isUnblocked(startY, startX) == false || isUnblocked(endY, endX) == false)
            {
                //MessageBox.Show("Source or Destination is blocked");
                return null;
            }

            if (isDestination(startY, startX, endY, endX) == true)
            {
                //MessageBox.Show("Already at destination");
                return null;
            }

            bool[,] closedList = new bool[totalRows, totalCols];

            cell[,] cellDetails = new cell[totalRows, totalCols];

            int i, j;

            for (i = 0; i < totalRows; i++)
            {
                for (j = 0; j < totalCols; j++)
                {
                    cellDetails[i, j].f = double.MaxValue;
                    cellDetails[i, j].g = double.MaxValue;
                    cellDetails[i, j].h = double.MaxValue;
                    cellDetails[i, j].parentI = -1;
                    cellDetails[i, j].parentJ = -1;
                }
            }

            i = startY;
            j = startX;

            cellDetails[i, j].f = 0.0;
            cellDetails[i, j].g = 0.0;
            cellDetails[i, j].h = 0.0;
            cellDetails[i, j].parentI = i;
            cellDetails[i, j].parentJ = j;

            List<pPair> openList = new List<pPair>();

            openList.Add(new pPair(0.0, (i, j)));

            bool foundDest = false;

            while (openList.Count != 0)
            {
                pPair p = openList[0];
                openList.Remove(p);

                i = p.index.Item1;
                j = p.index.Item2;
                closedList[i, j] = true;

                //variables to store the g, h, and f of the 8 successors
                double gNew, hNew, fNew;

                //North Successor
                if (isValid(i - 1, j) == true)
                {
                    //if the cell is the destination
                    if (isDestination(i - 1, j, endY, endX) == true)
                    {
                        cellDetails[i - 1, j].parentI = i;
                        cellDetails[i - 1, j].parentJ = j;
                        //MessageBox.Show("Destination is found");
                        foundDest = true;
                        return tracePath(cellDetails, endY, endX); ;
                    }
                    //if the cell is already on closed list or it is blocked ignore it otherwise do the following
                    else if (closedList[i - 1, j] == false && isUnblocked(i - 1, j) == true)
                    {
                        gNew = cellDetails[i, j].g + 1;
                        hNew = calculateHValue(i - 1, j, endY, endX);
                        fNew = gNew + hNew;

                        //if not on open list add to open list, make current square the parent of this square, record f g and h cost of the cell
                        //or
                        //if it is on the open list already check to see if this path to that square is better using f cost as the measure
                        if (cellDetails[i - 1, j].f == float.MaxValue || cellDetails[i - 1, j].f > fNew)
                        {
                            openList.Add(new pPair(fNew, (i - 1, j)));

                            cellDetails[i - 1, j].f = fNew;
                            cellDetails[i - 1, j].g = gNew;
                            cellDetails[i - 1, j].h = hNew;
                            cellDetails[i - 1, j].parentI = i;
                            cellDetails[i - 1, j].parentJ = j;
                        }
                    }
                }

                //South Successor
                if (isValid(i + 1, j) == true)
                {
                    //if the cell is the destination
                    if (isDestination(i + 1, j, endY, endX) == true)
                    {
                        cellDetails[i + 1, j].parentI = i;
                        cellDetails[i + 1, j].parentJ = j;
                        //MessageBox.Show("Destination is found");
                        foundDest = true;
                        return tracePath(cellDetails, endY, endX); ;
                    }
                    //if the cell is already on closed list or it is blocked ignore it otherwise do the following
                    else if (closedList[i + 1, j] == false && isUnblocked(i + 1, j) == true)
                    {
                        gNew = cellDetails[i, j].g + 1;
                        hNew = calculateHValue(i + 1, j, endY, endX);
                        fNew = gNew + hNew;

                        //if not on open list add to open list, make current square the parent of this square, record f g and h cost of the cell
                        //or
                        //if it is on the open list already check to see if this path to that square is better using f cost as the measure
                        if (cellDetails[i + 1, j].f == float.MaxValue || cellDetails[i + 1, j].f > fNew)
                        {
                            openList.Add(new pPair(fNew, (i + 1, j)));

                            cellDetails[i + 1, j].f = fNew;
                            cellDetails[i + 1, j].g = gNew;
                            cellDetails[i + 1, j].h = hNew;
                            cellDetails[i + 1, j].parentI = i;
                            cellDetails[i + 1, j].parentJ = j;
                        }
                    }
                }

                //East Successor
                if (isValid(i, j + 1) == true)
                {
                    //if the cell is the destination
                    if (isDestination(i, j + 1, endY, endX) == true)
                    {
                        cellDetails[i, j + 1].parentI = i;
                        cellDetails[i, j + 1].parentJ = j;
                        //MessageBox.Show("Destination is found");
                        foundDest = true;
                        return tracePath(cellDetails, endY, endX); ;
                    }
                    //if the cell is already on closed list or it is blocked ignore it otherwise do the following
                    else if (closedList[i, j + 1] == false && isUnblocked(i, j + 1) == true)
                    {
                        gNew = cellDetails[i, j].g + 1;
                        hNew = calculateHValue(i, j + 1, endY, endX);
                        fNew = gNew + hNew;

                        //if not on open list add to open list, make current square the parent of this square, record f g and h cost of the cell
                        //or
                        //if it is on the open list already check to see if this path to that square is better using f cost as the measure
                        if (cellDetails[i, j + 1].f == float.MaxValue || cellDetails[i, j + 1].f > fNew)
                        {
                            openList.Add(new pPair(fNew, (i, j + 1)));

                            cellDetails[i, j + 1].f = fNew;
                            cellDetails[i, j + 1].g = gNew;
                            cellDetails[i, j + 1].h = hNew;
                            cellDetails[i, j + 1].parentI = i;
                            cellDetails[i, j + 1].parentJ = j;
                        }
                    }
                }
                //West Successor
                if (isValid(i, j - 1) == true)
                {
                    //if the cell is the destination
                    if (isDestination(i, j - 1, endY, endX) == true)
                    {
                        cellDetails[i, j - 1].parentI = i;
                        cellDetails[i, j - 1].parentJ = j;
                        //MessageBox.Show("Destination is found");
                        foundDest = true;
                        return tracePath(cellDetails, endY, endX); ;
                    }
                    //if the cell is already on closed list or it is blocked ignore it otherwise do the following
                    else if (closedList[i, j - 1] == false && isUnblocked(i, j - 1) == true)
                    {
                        gNew = cellDetails[i, j].g + 1;
                        hNew = calculateHValue(i, j - 1, endY, endX);
                        fNew = gNew + hNew;

                        //if not on open list add to open list, make current square the parent of this square, record f g and h cost of the cell
                        //or
                        //if it is on the open list already check to see if this path to that square is better using f cost as the measure
                        if (cellDetails[i, j - 1].f == float.MaxValue || cellDetails[i, j - 1].f > fNew)
                        {
                            openList.Add(new pPair(fNew, (i, j - 1)));

                            cellDetails[i, j - 1].f = fNew;
                            cellDetails[i, j - 1].g = gNew;
                            cellDetails[i, j - 1].h = hNew;
                            cellDetails[i, j - 1].parentI = i;
                            cellDetails[i, j - 1].parentJ = j;
                        }
                    }
                }
                //North-east Successor
                if (isValid(i - 1, j + 1) == true)
                {
                    //if the cell is the destination
                    if (isDestination(i - 1, j + 1, endY, endX) == true)
                    {
                        cellDetails[i - 1, j + 1].parentI = i;
                        cellDetails[i - 1, j + 1].parentJ = j;
                        //MessageBox.Show("Destination is found");
                        foundDest = true;
                        return tracePath(cellDetails, endY, endX); ;
                    }
                    //if the cell is already on closed list or it is blocked ignore it otherwise do the following
                    else if (closedList[i - 1, j + 1] == false && isUnblocked(i - 1, j + 1) == true)
                    {
                        gNew = cellDetails[i, j].g + 1;
                        hNew = calculateHValue(i - 1, j + 1, endY, endX);
                        fNew = gNew + hNew;

                        //if not on open list add to open list, make current square the parent of this square, record f g and h cost of the cell
                        //or
                        //if it is on the open list already check to see if this path to that square is better using f cost as the measure
                        if (cellDetails[i - 1, j + 1].f == float.MaxValue || cellDetails[i - 1, j + 1].f > fNew)
                        {
                            openList.Add(new pPair(fNew, (i - 1, j + 1)));

                            cellDetails[i - 1, j + 1].f = fNew;
                            cellDetails[i - 1, j + 1].g = gNew;
                            cellDetails[i - 1, j + 1].h = hNew;
                            cellDetails[i - 1, j + 1].parentI = i;
                            cellDetails[i - 1, j + 1].parentJ = j;
                        }
                    }
                }

                //North-west Successor
                if (isValid(i - 1, j - 1) == true)
                {
                    //if the cell is the destination
                    if (isDestination(i - 1, j - 1, endY, endX) == true)
                    {
                        cellDetails[i - 1, j - 1].parentI = i;
                        cellDetails[i - 1, j - 1].parentJ = j;
                        //MessageBox.Show("Destination is found");
                        foundDest = true;
                        return tracePath(cellDetails, endY, endX); ;
                    }
                    //if the cell is already on closed list or it is blocked ignore it otherwise do the following
                    else if (closedList[i - 1, j - 1] == false && isUnblocked(i - 1, j - 1) == true)
                    {
                        gNew = cellDetails[i, j].g + 1;
                        hNew = calculateHValue(i - 1, j - 1, endY, endX);
                        fNew = gNew + hNew;

                        //if not on open list add to open list, make current square the parent of this square, record f g and h cost of the cell
                        //or
                        //if it is on the open list already check to see if this path to that square is better using f cost as the measure
                        if (cellDetails[i - 1, j - 1].f == float.MaxValue || cellDetails[i - 1, j - 1].f > fNew)
                        {
                            openList.Add(new pPair(fNew, (i - 1, j - 1)));

                            cellDetails[i - 1, j - 1].f = fNew;
                            cellDetails[i - 1, j - 1].g = gNew;
                            cellDetails[i - 1, j - 1].h = hNew;
                            cellDetails[i - 1, j - 1].parentI = i;
                            cellDetails[i - 1, j - 1].parentJ = j;
                        }
                    }
                }

                //South-east Successor
                if (isValid(i + 1, j + 1) == true)
                {
                    //if the cell is the destination
                    if (isDestination(i + 1, j + 1, endY, endX) == true)
                    {
                        cellDetails[i + 1, j + 1].parentI = i;
                        cellDetails[i + 1, j + 1].parentJ = j;
                        //MessageBox.Show("Destination is found");
                        foundDest = true;
                        return tracePath(cellDetails, endY, endX); ;
                    }
                    //if the cell is already on closed list or it is blocked ignore it otherwise do the following
                    else if (closedList[i + 1, j + 1] == false && isUnblocked(i + 1, j + 1) == true)
                    {
                        gNew = cellDetails[i, j].g + 1;
                        hNew = calculateHValue(i + 1, j + 1, endY, endX);
                        fNew = gNew + hNew;

                        //if not on open list add to open list, make current square the parent of this square, record f g and h cost of the cell
                        //or
                        //if it is on the open list already check to see if this path to that square is better using f cost as the measure
                        if (cellDetails[i + 1, j + 1].f == float.MaxValue || cellDetails[i + 1, j + 1].f > fNew)
                        {
                            openList.Add(new pPair(fNew, (i + 1, j + 1)));

                            cellDetails[i + 1, j + 1].f = fNew;
                            cellDetails[i + 1, j + 1].g = gNew;
                            cellDetails[i + 1, j + 1].h = hNew;
                            cellDetails[i + 1, j + 1].parentI = i;
                            cellDetails[i + 1, j + 1].parentJ = j;
                        }
                    }
                }

                //South-west Successor
                if (isValid(i + 1, j - 1) == true)
                {
                    //if the cell is the destination
                    if (isDestination(i + 1, j - 1, endY, endX) == true)
                    {
                        cellDetails[i + 1, j - 1].parentI = i;
                        cellDetails[i + 1, j - 1].parentJ = j;
                        //MessageBox.Show("Destination is found");
                        foundDest = true;
                        return tracePath(cellDetails, endY, endX); ;
                    }
                    //if the cell is already on closed list or it is blocked ignore it otherwise do the following
                    else if (closedList[i + 1, j - 1] == false && isUnblocked(i + 1, j - 1) == true)
                    {
                        gNew = cellDetails[i, j].g + 1;
                        hNew = calculateHValue(i + 1, j - 1, endY, endX);
                        fNew = gNew + hNew;

                        //if not on open list add to open list, make current square the parent of this square, record f g and h cost of the cell
                        //or
                        //if it is on the open list already check to see if this path to that square is better using f cost as the measure
                        if (cellDetails[i + 1, j - 1].f == float.MaxValue || cellDetails[i + 1, j - 1].f > fNew)
                        {
                            openList.Add(new pPair(fNew, (i + 1, j + 1)));

                            cellDetails[i + 1, j - 1].f = fNew;
                            cellDetails[i + 1, j - 1].g = gNew;
                            cellDetails[i + 1, j - 1].h = hNew;
                            cellDetails[i + 1, j - 1].parentI = i;
                            cellDetails[i + 1, j - 1].parentJ = j;
                        }
                    }
                }
            }

            if(foundDest == false)
            {
                //MessageBox.Show("failed to find destination");
            }

            return null;

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