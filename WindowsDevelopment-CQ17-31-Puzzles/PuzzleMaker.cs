using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsDevelopment_CQ17_31_Puzzles
{
    public class phase
    {
        public Tuple<int, int> from;
        public Tuple<int, int> to;
        public int piece;

    }


    class PuzzleMaker
    {
        public static int Easy
        {
            get
            {
                return 12;
            }
        }

        public static int Medium
        {
            get
            {
                return 42;
            }
        }

        public static int Hard
        {
            get
            {
                return 72;
            }
        }

        static private PuzzleMaker maker = null;
        static int rows = 3;
        static int columms = 3;
        int[,] _a { get; set; }
        public int Difficulty;
        public int[,] PieceOrder
        {
            get
            {
                return _a;
            }
        }
        private List<phase> phases;
        int currentPhase;

        static public PuzzleMaker GetInstance()
        {
            if (maker == null)
            {
                maker = new PuzzleMaker
                {
                    _a = new int[rows, columms],
                    phases = new List<phase>(),
                    currentPhase = -1,
                    Difficulty = 32,
                };
            }
            return maker;
        }

        public void GeneratePuzzle()
        {
            phases.Clear();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columms; j++)
                {
                    _a[i, j] = j * 3 + i;
                }
            }

            int x = rows - 1;
            int y = columms - 1;

            

            Random rng = new Random();
            int swapTime = rng.Next(Difficulty - 2, Difficulty + 2);

            int oldDirection = 0;
            for (int i = 0; i < swapTime; i++)
            {
                int direction = 0;
                bool moveSuccess = false;

                while (moveSuccess != true)
                {
                    int oldX = x;
                    int oldY = y;
                    do
                    {
                        int seed = rng.Next(100, 1000);
                        direction = seed % 4;
                    } while (Math.Abs(direction - oldDirection) == 2); // prevent repeat shuffle
                    

                    // move to the left

                    switch (direction)
                    {
                        case 0: //move left
                            {
                                x--;
                                break;
                            }
                        case 1: //move up
                            {
                                y--;
                                break;
                            }
                        case 2: //move right
                            {
                                x++;
                                break;
                            }
                        case 3: //move down
                            {
                                y++;
                                break;
                            }
                    }

                    if (x > rows - 1 || x < 0 || y > columms - 1 || y < 0)
                    {
                        x = oldX;
                        y = oldY;
                        moveSuccess = false;
                        continue;
                    }
                    else
                    {
                        _a[oldX, oldY] = _a[x, y];
                        _a[x, y] = 8;
                        moveSuccess = true;
                        oldDirection = direction;
                        Test();
                    }

                }
            }
        }

        public void LoadPuzzle(int[,] save)
        {
            phases.Clear();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columms; j++)
                {
                    _a[i, j] = save[i, j];
                }
            }
        }

        /// <summary>
        /// get position of the piece in puzzle by tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public Tuple<int, int> GetPiecePosition(int tag)
        {
            Tuple<int, int> result = new Tuple<int, int>(-1, -1);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columms; j++)
                {
                    if (_a[i, j] == tag)
                    {
                        result = new Tuple<int, int>(i, j);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// move a piece in puzzle to new location
        /// </summary>
        /// <param name="oldPos">old location</param>
        /// <param name="newPos">new location</param>
        /// <returns></returns>
        public bool MovePiece(Tuple<int, int> oldPos, Tuple<int, int> newPos)
        {
            //if the position is out of range
            if (newPos.Item1 < 0 || newPos.Item1 > columms - 1 || newPos.Item2 < 0 || newPos.Item2 > rows - 1)
            {
                return false;
            }

            //if the moved to postion is not the empty space
            if (_a[newPos.Item1, newPos.Item2] != 8)
            {
                return false;
            }

            //if the moved-to-position is not aligned with the old one
            if (newPos.Item1 == oldPos.Item1 && Math.Abs(newPos.Item2 - oldPos.Item2) != 1)
            {
                return false;
            }

            if (newPos.Item2 == oldPos.Item2 && Math.Abs(newPos.Item1 - oldPos.Item1) != 1)
            {
                return false;
            }

            if (newPos.Item1 != oldPos.Item1 && newPos.Item2 != oldPos.Item2)
            {
                return false;
            }

            //if all conditions are met
            phase newPhase = new phase()
            {
                from = new Tuple<int, int>(oldPos.Item1, oldPos.Item2),
                to = new Tuple<int, int>(newPos.Item1, newPos.Item2),
                piece = _a[oldPos.Item1, oldPos.Item2],
            };

            savePhase(newPhase);

            _a[newPos.Item1, newPos.Item2] = _a[oldPos.Item1, oldPos.Item2];
            _a[oldPos.Item1, oldPos.Item2] = 8;
            Test();
            return true;
            
        }

        public bool CheckWin()
        {
            int checkedTag = 0;

            //swept through whole matrix to see if checkedTag is in the right position
            while (checkedTag < 8)
            {
                if (_a[checkedTag % 3, checkedTag / 3] != checkedTag)
                    return false;
                checkedTag++;
            }
            Test();
            return true;
        }

        private void savePhase(phase NewPhase)
        {
            if (phases.Count - 1 > currentPhase)
            {
                int range = phases.Count - 1 - currentPhase;
                phases.RemoveRange(currentPhase + 1, range);
            }
            phases.Add(NewPhase);
            currentPhase++;
        }

        public bool UndoMove()
        {
            if (currentPhase >= 0)
            {
                phase oldPhase = phases[currentPhase];
                _a[oldPhase.from.Item1, oldPhase.from.Item2] = _a[oldPhase.to.Item1, oldPhase.to.Item2];
                _a[oldPhase.to.Item1, oldPhase.to.Item2] = 8;
                currentPhase--;
                return true;
            }
            return false;
        }

        public bool RedoMove()
        {
            if (currentPhase >= -1 && currentPhase < phases.Count - 1)
            {
                phase newPhase = phases[currentPhase + 1];
                _a[newPhase.to.Item1, newPhase.to.Item2] = _a[newPhase.from.Item1, newPhase.from.Item2];
                _a[newPhase.from.Item1, newPhase.from.Item2] = 8;
                currentPhase++;
                return true;
            }
            return false;
        }

        public phase GetCurrentPhase()
        {
            if (currentPhase >= 0)
            {
                return phases[currentPhase];
            }
            else
            {
                return null;
            }
        }

        public phase GetNextPhase()
        {
            if (currentPhase >= -1 && currentPhase < phases.Count - 1)
            {
                return phases[currentPhase + 1];
            }
            else
            {
                return null;
            }
        }

        void Test()
        {
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    Console.Write(_a[i, j]);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}

