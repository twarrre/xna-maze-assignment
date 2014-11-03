using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Assignment3
{
    // Code borrowed and based off of http://xnafan.net/2012/03/maze-creation-in-c/
    /// <summary>
    /// Class to create random mazes with tiles as walls
    /// Jakob Krarup (www.xnafan.net)
    /// Use, alter and redistribute this code freely,
    /// but please leave this comment :)
    /// </summary>
    public class MazeGenerator
    {
        #region Variables and properties

        /// <summary>
        /// Defines whether all corridors should stay with one tile separation, keeping to neat horizontal and vertical lines
        /// </summary>
        public bool DiagonalTunnelingAllowed { get; private set; }

        /// <summary>
        /// The maze as it looks now. Empty Points are zeroes and walls are ones
        /// </summary>
        public byte[,] Maze { get; private set; }

        //The stack of points not tested yet
        private Stack<Point> _tiletoTry = new Stack<Point>();

        /// <summary>
        /// The list of offsets to use to get the tiles above, below, to the right and the left of a specific tile
        /// </summary>
        private List<Point> NSEW = new List<Point> { new Point(0, 1), new Point(0, -1), new Point(1, 0), new Point(-1, 0) };

        /// <summary>
        /// Used to generate random values
        /// </summary>
        static Random rnd = new Random();

        private int _width, _height;
        private Point _currentTile;

        /// <summary>
        /// The width of the maze
        /// </summary>
        public int Width
        {
            get { return _width; }
            private set
            {
                //must be larger than two
                if (value < 3)
                {
                    throw new ArgumentException("Width must be larger than two for the generator to work");
                }

                _width = value;
            }
        }

        /// <summary>
        /// The height of the maze
        /// </summary>
        public int Height
        {

            get { return _height; }
            private set
            {
                //must be larger than two
                if (value < 3)
                {
                    throw new ArgumentException("Height must be larger than two for the generator to work");
                }

                _height = value;

            }

        }

        /// <summary>
        /// The tile where development of the maze is currently at
        /// </summary>
        public Point CurrentTile
        {
            get { return _currentTile; }
            private set
            {
                if (value.X < 1 || value.X >= this.Width - 1 || value.Y < 1 || value.Y >= this.Height - 1)
                {
                    throw new ArgumentException("CurrentTile must be within the one tile border all around the maze");
                }
                if (value.X % 2 == 1 || value.Y % 2 == 1 || DiagonalTunnelingAllowed)
                { _currentTile = value; }
                else
                {
                    throw new ArgumentException("The current square must not be both on an even X-axis and an even Y-axis when DiagonalTunnelingAllowed is false, to ensure we can get walls around all tunnels");
                }
            }

        }

        #endregion


        #region Constructors

        /// <summary>
        /// Creates a new mazegenerator with mazes starting at (1,1), and no diagonal tunneling.
        /// </summary>
        /// <param name="width">The number of tiles across in the mazes to generate. Must include the two ekstra tiles for a wall in both sides.</param>
        /// <param name="height">The number of tiles from top to bottom in the mazes to generate. Must include the two ekstra tiles for a wall both at top and bottom.</param>
        public MazeGenerator(int width, int height) : this(width, height, new Point(1, 1)) { }


        /// <summary>
        /// Creates a new mazegenerator with mazes starting at the requested startingPosition, and no diagonal tunneling.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="startingPosition"></param>
        public MazeGenerator(int width, int height, Point startingPosition) : this(width, height, startingPosition, false) { }


        /// <summary>
        /// Creates a new mazegenerator with mazes starting at the requested startingPosition, and no diagonal tunneling.
        /// </summary>
        /// <param name="width">The number of tiles across in the mazes to generate. Must include the two ekstra tiles for a wall in both sides.</param>
        /// <param name="height">The number of tiles from top to bottom in the mazes to generate. Must include the two ekstra tiles for a wall both at top and bottom.</param>
        /// <param name="startingPosition">Where to start tunneling from</param>
        /// <param name="diagonalTunnelingAllowed">Whether the tunneling should allow diagonal routes, e.g. up, left, up, left</param>
        public MazeGenerator(int width, int height, Point startingPosition, bool diagonalTunnelingAllowed)
        {

            this.Width = width;
            this.Height = height;

            DiagonalTunnelingAllowed = diagonalTunnelingAllowed;

            Maze = new byte[Width, Height];

            //initialize all fields as taken
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Maze[x, y] = 1;
                }
            }

            //start the excavation from the current position
            CurrentTile = startingPosition;
            //add the beginning position to the tiles to try
            _tiletoTry.Push(CurrentTile);
        }

        #endregion


        #region CreateMaze

        /// <summary>
        /// Creates a new maze with the current size and starting position
        /// </summary>
        /// <returns>A freshly generated, random maze</returns>
        public byte[,] CreateMaze()
        {
            //local variable to store neighbors to the current square
            //as we work our way through the maze
            List<Point> neighbors;

            //as long as there are still tiles to try
            while (_tiletoTry.Count > 0)
            {
                //excavate the square we are on
                Maze[CurrentTile.X, CurrentTile.Y] = 0;

                //get all valid neighbors for the new tile  
                neighbors = GetValidNeighbors(CurrentTile);

                //if there are any interesting looking neighbors
                if (neighbors.Count > 0)
                {
                    //remember this tile, by putting it on the stack
                    _tiletoTry.Push(CurrentTile);
                    //move on to a random of the neighboring tiles
                    CurrentTile = neighbors[rnd.Next(neighbors.Count)];
                }
                else
                {
                    //toss this tile out 
                    //(thereby returning to a previous tile in the list to check).
                    CurrentTile = _tiletoTry.Pop();
                }
            }

            return Maze;
        }

        #endregion


        #region Helpermethods

        /// <summary>
        /// Get all the prospective neighboring tiles
        /// </summary>
        /// <param name="centerTile">The tile to test</param>
        /// <returns>All and any valid neighbors</returns>
        private List<Point> GetValidNeighbors(Point centerTile)
        {

            List<Point> validNeighbors = new List<Point>();

            //Check all four directions around the tile
            foreach (var offset in NSEW)
            {
                //find the neighbor's position
                Point toCheck = new Point(centerTile.X + offset.X, centerTile.Y + offset.Y);

                //make sure the tile is not on both an even X-axis and an even Y-axis
                //to ensure we can get walls around all tunnels
                if (toCheck.X % 2 == 1 || toCheck.Y % 2 == 1 || DiagonalTunnelingAllowed)
                {
                    //if the potential neighbor is unexcavated (==1)
                    //and still has three walls intact (new territory)
                    if (Maze[toCheck.X, toCheck.Y] == 1 && HasThreeWallsIntact(toCheck))
                    {
                        //add the neighbor
                        validNeighbors.Add(toCheck);
                    }
                }
            }

            return validNeighbors;
        }


        /// <summary>
        /// Counts the number of intact walls around a tile
        /// </summary>
        /// <param name="pointToCheck">The coordinates of the tile to check</param>
        /// <returns>Whether there are three intact walls (the tile has not been dug into earlier.</returns>
        private bool HasThreeWallsIntact(Point pointToCheck)
        {
            int intactWallCounter = 0;

            //Check all four directions around the tile
            foreach (var offset in NSEW)
            {
                //find the neighbor's position
                Point neighborToCheck = new Point(pointToCheck.X + offset.X, pointToCheck.Y + offset.Y);

                //make sure it is inside the maze, and it hasn't been dug out yet
                if (IsInside(neighborToCheck) && Maze[neighborToCheck.X, neighborToCheck.Y] == 1)
                {
                    intactWallCounter++;
                }
            }

            //tell whether three walls are intact
            return intactWallCounter == 3;

        }

        /// <summary>
        /// Find out whether a tile is inside the maze
        /// </summary>
        /// <param name="p">The coordinates of the tile to check</param>
        /// <returns>Whether the tile is inside the maze</returns>
        private bool IsInside(Point p)
        {
            return p.X >= 0 && p.Y >= 0 && p.X < Width && p.Y < Height;
        }

        #endregion

        public int[,] ToIntMap()
        {
            int[,] intMaze = new int[Width, Height];
            for (int y = Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (Maze[x, y] == 0)
                    {
                        intMaze[x, y] = 0;
                    }
                    else
                    {
                        intMaze[x, y] = 1;
                    }
                }
            }
            return intMaze;
        }
    }
}