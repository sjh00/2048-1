﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Globalization.DateTimeFormatting;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace _2048
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private const int _ROWS = 4;
        private const int _COLS = 4;

        private GameTile[][] _tiles;

        public MainPage()
        {
            this.InitializeComponent();

            _tiles = new GameTile[_COLS][];
            for (int i = 0; i < _COLS; ++i)
            {
                _tiles[i] = new GameTile[_ROWS];
            }

            for (int y = 0; y < _ROWS; ++y)
            {
                for (int x = 0; x < _COLS; ++x)
                {
                    _tiles[x][y] = new GameTile();
                    _tiles[x][y].Width = 150;
                    _tiles[x][y].Height = 150;
                    _tiles[x][y].SetValue(Canvas.LeftProperty, x * 150);
                    _tiles[x][y].SetValue(Canvas.TopProperty, y * 150);
                    GameGrid.Children.Add(_tiles[x][y]);
                }
            }

            StartGame();
        }

        private void StartGame()
        {
            var first = GetRandomEmptyTile();
            _tiles[first.Item1][first.Item2].Value = GetRandomStartingNumber();

            var second = GetRandomEmptyTile();
            _tiles[second.Item1][second.Item2].Value = GetRandomStartingNumber();

            Window.Current.CoreWindow.KeyDown += OnKeyDown;
        }

        private void OnKeyDown(CoreWindow Sender, KeyEventArgs Args)
        {
            if (Args.VirtualKey != VirtualKey.Up && Args.VirtualKey != VirtualKey.Down && Args.VirtualKey != VirtualKey.Left && Args.VirtualKey != VirtualKey.Right)
            {
                return;
            }

            moves.Clear();
            if (PackTiles(Args.VirtualKey) | MergeTiles(Args.VirtualKey))
            {
                foreach (var move in moves)
                {
                    //_tiles[move.Item1.Item1][move.Item1.Item1].MoveTo(move.Item2.Item1, move.Item2.Item2);
                }

                for (var y = 0; y < _ROWS; ++y)
                {
                    for (var x = 0; x < _COLS; ++x)
                    {
                        if (_tiles[x][y].WasDoubled)
                        {
                            _tiles[x][y].WasDoubled = false;
                            _tiles[x][y].BeginDoubledAnimation();
                        }
                    }
                }

                var newTile = GetRandomEmptyTile();

                if (newTile != null)
                {
                    _tiles[newTile.Item1][newTile.Item2].Value = GetRandomStartingNumber();
                    _tiles[newTile.Item1][newTile.Item2].BeginNewTileAnimation();
                }
                else
                {
                    // Game over?
                }
            }
        }

        private List<Tuple<Tuple<int, int>, Tuple<int, int>>> moves = new List<Tuple<Tuple<int, int>, Tuple<int, int>>>(); 

        private bool PackTiles(VirtualKey MoveDirection)
        {
            var changed = false;
            if (MoveDirection == VirtualKey.Up)
            {
                for (var x = 0; x < _COLS; ++x)
                {
                    var lastEmptyY = 0;

                    while (lastEmptyY < _ROWS && _tiles[x][lastEmptyY].Value != 0)
                    {
                        ++lastEmptyY;
                    }

                    var currentY = lastEmptyY + 1;
                    while (currentY < _ROWS)
                    {
                        if (_tiles[x][currentY].Value > 0)
                        {
                            changed = true;
                            moves.Add(new Tuple<Tuple<int, int>, Tuple<int, int>>(new Tuple<int, int>(x, currentY), new Tuple<int, int>(x, lastEmptyY)));
                            _tiles[x][lastEmptyY].Value = _tiles[x][currentY].Value;
                            _tiles[x][currentY].Value = 0;
                            ++lastEmptyY;
                        }
                        ++currentY;
                    }
                }
            }
            else if (MoveDirection == VirtualKey.Down)
            {
                for (var x = 0; x < _COLS; ++x)
                {
                    var lastEmptyY = _ROWS - 1;

                    while (lastEmptyY >= 0 && _tiles[x][lastEmptyY].Value != 0)
                    {
                        --lastEmptyY;
                    }

                    var currentY = lastEmptyY - 1;
                    while (currentY >= 0)
                    {
                        if (_tiles[x][currentY].Value > 0)
                        {
                            changed = true;
                            _tiles[x][lastEmptyY].Value = _tiles[x][currentY].Value;
                            _tiles[x][currentY].Value = 0;
                            --lastEmptyY;
                        }
                        --currentY;
                    }
                }
            }
            else if (MoveDirection == VirtualKey.Left)
            {
                for (var y = 0; y < _ROWS; ++y)
                {
                    var lastEmptyX = 0;

                    while (lastEmptyX < _COLS && _tiles[lastEmptyX][y].Value != 0)
                    {
                        ++lastEmptyX;
                    }

                    var currentX = lastEmptyX + 1;
                    while (currentX < _COLS)
                    {
                        if (_tiles[currentX][y].Value > 0)
                        {
                            changed = true;
                            _tiles[lastEmptyX][y].Value = _tiles[currentX][y].Value;
                            _tiles[currentX][y].Value = 0;
                            ++lastEmptyX;
                        }
                        ++currentX;
                    }
                }
            }
            else if (MoveDirection == VirtualKey.Right)
            {

                for (var y = 0; y < _ROWS; ++y)
                {
                    var lastEmptyX = _COLS - 1;

                    while (lastEmptyX >= 0 && _tiles[lastEmptyX][y].Value != 0)
                    {
                        --lastEmptyX;
                    }

                    var currentX = lastEmptyX - 1;
                    while (currentX >= 0)
                    {
                        if (_tiles[currentX][y].Value > 0)
                        {
                            changed = true;
                            _tiles[lastEmptyX][y].Value = _tiles[currentX][y].Value;
                            _tiles[currentX][y].Value = 0;
                            --lastEmptyX;
                        }
                        --currentX;
                    }
                }
            }
            return changed;
        }

        private bool MergeTiles(VirtualKey MoveDirection)
        {
            var changed = false;
            if (MoveDirection == VirtualKey.Up)
            {
                for (var x = 0; x < _COLS; ++x)
                {
                    for (var y = 0; y < _ROWS - 1 && _tiles[x][y].Value > 0; ++y)
                    {
                        if (_tiles[x][y].Value == _tiles[x][y + 1].Value)
                        {
                            changed = true;
                            _tiles[x][y].Value *= 2;
                            _tiles[x][y].WasDoubled = true;
                            _tiles[x][y + 1].Value = 0;
                            PackTiles(MoveDirection);
                        }
                    }
                }
            }
            else if (MoveDirection == VirtualKey.Down)
            {
                for (var x = 0; x < _COLS; ++x)
                {
                    for (var y = _ROWS - 1; y >= 1 && _tiles[x][y].Value > 0; --y)
                    {
                        if (_tiles[x][y].Value == _tiles[x][y - 1].Value)
                        {
                            changed = true;
                            _tiles[x][y].Value *= 2;
                            _tiles[x][y].WasDoubled = true;
                            _tiles[x][y - 1].Value = 0;
                            PackTiles(MoveDirection);
                        }
                    }
                }
            }
            else if (MoveDirection == VirtualKey.Left)
            {
                for (var y = 0; y < _ROWS; ++y)
                {
                    for (var x = 0; x < _COLS - 1 && _tiles[x][y].Value > 0; ++x)
                    {
                        if (_tiles[x][y].Value == _tiles[x + 1][y].Value)
                        {
                            changed = true;
                            _tiles[x][y].Value *= 2;
                            _tiles[x][y].WasDoubled = true;
                            _tiles[x + 1][y].Value = 0;
                            PackTiles(MoveDirection);
                        }
                    }
                }
            }
            else if (MoveDirection == VirtualKey.Right)
            {
                for (var y = 0; y < _ROWS; ++y)
                {
                    for (var x = _COLS - 1; x >= 1 && _tiles[x][y].Value > 0; --x)
                    {
                        if (_tiles[x][y].Value == _tiles[x - 1][y].Value)
                        {
                            changed = true;
                            _tiles[x][y].Value *= 2;
                            _tiles[x][y].WasDoubled = true;
                            _tiles[x - 1][y].Value = 0;
                            PackTiles(MoveDirection);
                        }
                    }
                }
            }

            return changed;
        }

        private Random rnd = new Random();
        private Tuple<int, int> GetRandomEmptyTile()
        {
            var emptyIndices = new List<Tuple<int, int>>();
            for (int y = 0; y < _ROWS; ++y)
            {
                for (int x = 0; x < _COLS; ++x)
                {
                    if (_tiles[x][y].Value == 0)
                    {
                        emptyIndices.Add(new Tuple<int, int>(x,y));
                    }
                }
            }

            if (emptyIndices.Count == 0)
            {
                return null;
            }

            var next = rnd.Next(0, emptyIndices.Count - 1);
            return emptyIndices[next];
        }

        private int GetRandomStartingNumber()
        {
            return rnd.NextDouble() < 0.75 ? 2 : 4;
        }
    }
}
