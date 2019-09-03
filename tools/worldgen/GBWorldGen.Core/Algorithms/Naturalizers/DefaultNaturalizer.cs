﻿using GBWorldGen.Core.Algorithms.Abstractions;
using GBWorldGen.Core.Algorithms.Methods;
using GBWorldGen.Core.Models;
using GBWorldGen.Misc.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GBWorldGen.Core.Algorithms.Naturalize
{
    public class DefaultNaturalizer : WorldData, INaturalizeWorld
    {
        private int LowestY { get; set; }
        private int LowestYVisible { get; set; }
        private int HighestY { get; set; }

        public Map Naturalize(Map map)
        {
            CalcuateExtremes(map);
            map = YAdjustMap(map);
            CalcuateExtremes(map); // Need to recalculate after moving

            if (LowestY != HighestY)
            {
                map = PaintAndFillWater(map);
                map = FillBottom(map);
                //map = CreateTunnels(map);
            }            

            return map;
        }

        private Map YAdjustMap(Map map)
        {            
            Console.WriteLine("Y-adjusting map...");

            List<short> ys = new List<short>();
            short averageY = 0;
            for (int x = 0; x < map.BlockData.GetLength(0); x++)
                for (int y = 0; y < map.BlockData.GetLength(1); y++)
                    for (int z = 0; z < map.BlockData.GetLength(2); z++)
                        ys.Add(map.BlockData[x, y, z].Y);

            averageY = (short)ys.Average(a => a);
            short adjustY = (short)(averageY * -1.0d);
            int tries = 10;

            while (tries > 0 &&
                (averageY + adjustY < MinWorldY ||
                averageY + adjustY > MaxWorldY ||
                LowestY + adjustY < MinWorldY ||
                HighestY + adjustY > MaxWorldY))
            {
                adjustY = (short)(adjustY / 2.0d);
                tries--;
            }

            // Move all blocks lower/higher more towards Y=0
            for (int x = 0; x < map.BlockData.GetLength(0); x++)
                for (int y = 0; y < map.BlockData.GetLength(1); y++)
                    for (int z = 0; z < map.BlockData.GetLength(2); z++)
                    {
                        map.BlockData[x, y, z].Y += adjustY;

                        // Set extremes
                        if (map.BlockData[x, y, z].Y < MinWorldY) map.BlockData[x, y, z].Y = (short)MinWorldY;
                        if (map.BlockData[x, y, z].Y > MaxWorldY) map.BlockData[x, y, z].Y = (short)MaxWorldY;
                    }

            return map;
        }

        private Map PaintAndFillWater(Map map)
        {
            Console.WriteLine("Painting map and filling in water...");

            // Calculate variables
            short lowestVisibleY = (short)MinWorldY;
            short highestVisibleY = (short)MaxWorldY;
            short lowest = 999; //blockData.Min(m => m.Y);
            short highest = -999; //blockData.Max(m => m.Y);

            for (int x = 0; x < map.BlockData.GetLength(0); x++)
                for (int y = 0; y < map.BlockData.GetLength(1); y++)
                    for (int z = 0; z < map.BlockData.GetLength(2); z++)
                    {
                        if (map.BlockData[x, y, z].Y < lowest) lowest = map.BlockData[x, y, z].Y;
                        if (map.BlockData[x, y, z].Y > highest) highest = map.BlockData[x, y, z].Y;
                    }

            if (lowest >= MinWorldY)
                lowestVisibleY = lowest;
            if (highest <= MaxWorldY)
                highestVisibleY = highest;

            List<Block> fillBlocks = new List<Block>();
            Block.STYLE[] styleRange = BlockStyleRatios(highestVisibleY - lowestVisibleY);
            int maxWaterIndex = Array.FindIndex(styleRange, x => x == styleRange.First(s => s != Block.STYLE.Water)) - 1;
            int temp = 0;

            // Paint blocks
            for (int x = 0; x < map.BlockData.GetLength(0); x++)
                for (int y = 0; y < map.BlockData.GetLength(1); y++)
                    for (int z = 0; z < map.BlockData.GetLength(2); z++)
                    {
                        if (map.BlockData[x, y, z].Y < MinWorldY || map.BlockData[x, y, z].Y > MaxWorldY) continue;

                        map.BlockData[x, y, z].Style = styleRange[map.BlockData[x, y, z].Y - lowestVisibleY];

                        // Fill water
                        if (map.BlockData[x, y, z].Style == Block.STYLE.Water)
                        {
                            temp = maxWaterIndex + lowestVisibleY - map.BlockData[x, y, z].Y;
                            for (int j = map.BlockData[x, y, z].Y + 1; j <= map.BlockData[x, y, z].Y + temp; j++)
                            {
                                fillBlocks.Add(new Block
                                {
                                    X = map.BlockData[x, y, z].X,
                                    Y = (short)j,
                                    Z = map.BlockData[x, y, z].Z,
                                    Shape = map.BlockData[x, y, z].Shape,
                                    Direction = map.BlockData[x, y, z].Direction,
                                    Style = map.BlockData[x, y, z].Style
                                });
                            }
                        }
                    }

            //var group = blockData
            //    .GroupBy(b => b.Style)
            //    .Select(g => new { Style = g.Key, Sum = g.Count() });

            return map;
        }

        private Block.STYLE[] BlockStyleRatios(int yRange)
        {            
            Block.STYLE[] styleRange = new Block.STYLE[yRange + 1];

            int dataKey = 0;            
            List<(double, Block.STYLE)> data = new List<(double, Block.STYLE)>            
            {
                ( 0.1d, Block.STYLE.Water ), // 10%
                ( 0.06d, Block.STYLE.Sand ), // 6%
                ( 0.55d, Block.STYLE.Grass ), // 55%
                ( 0.08d, Block.STYLE.Dirt ), // 8%
                ( 0.14d, Block.STYLE.GrayCraters ), // 14%
                ( 0.07d, Block.STYLE.Snow ) // 7%
            };
            double runningSum = data[0].Item1;

            for (int i = 0; i < styleRange.Length; i++)
            {                
                while (((double)i / styleRange.Length) > runningSum && 
                    dataKey + 1 < data.Count)
                {
                    dataKey++;
                    runningSum = 0;
                    for (int j = 0; j <= dataKey; j++)
                        runningSum += data[j].Item1;
                }

                styleRange[i] = data[dataKey].Item2;
            }

            return styleRange;
        }

        private void CalcuateExtremes(Map map)
        {            
            Console.WriteLine("Calculating map extremes...");
            
            int lowest = 999;
            int highest = -999;

            for (int x = 0; x < map.BlockData.GetLength(0); x++)
                for (int y = 0; y < map.BlockData.GetLength(1); y++)
                    for (int z = 0; z < map.BlockData.GetLength(2); z++)
                    {
                        if (map.BlockData[x, y, z].Y < lowest)
                            lowest = map.BlockData[x, y, z].Y;
                        if (map.BlockData[x, y, z].Y > highest)
                            highest = map.BlockData[x, y, z].Y;
                    }

            LowestY = lowest;
            HighestY = highest;
        }

        private Map FillBottom(Map map)
        {
            Console.WriteLine("Filling in bottom of map...");

            // Fill
            //for (int x = 0; x < map.BlockData.GetLength(0); x++)
            //    for (int y = 0; y < map.BlockData.GetLength(1); y++)
            //        for (int z = 0; z < map.BlockData.GetLength(2); z++)
            //        {
            //            for (int q = map.BlockData[x, y, z].Y - 1; q >= LowestY; q--)
            //            {
            //                fillBlocks.Add(new Block
            //                {
            //                    X = blockData[i].X,
            //                    Y = (short)j,
            //                    Z = blockData[i].Z,
            //                    Shape = blockData[i].Shape,
            //                    Direction = blockData[i].Direction,
            //                    Style = blockData[i].Style
            //                });
            //            }
            //        }
            //Block[] blockData = map.BlockData;
            //List<Block> fillBlocks = new List<Block>();
            //for (int i = 0; i < blockData.Length; i++)
            //    for (int j = blockData[i].Y - 1; j >= LowestY; j--)
            //    {
            //        fillBlocks.Add(new Block
            //        {
            //            X = blockData[i].X,
            //            Y = (short)j,
            //            Z = blockData[i].Z,
            //            Shape = blockData[i].Shape,
            //            Direction = blockData[i].Direction,
            //            Style = blockData[i].Style
            //        });
            //    }

            //int originalLength = blockData.Length;
            //Array.Resize(ref blockData, originalLength + fillBlocks.Count);
            //fillBlocks.ToArray().CopyTo(blockData, originalLength);

            //map.BlockData = blockData;
            return map;
        }

        private Map CreateTunnels(Map map)
        {
            Console.WriteLine("Creating tunnels...");

            PerlinNoise perlin = new PerlinNoise();
            short[,,] perlinTunnel = new short[
                map.BlockData.GetLength(0),
                map.BlockData.GetLength(1),
                map.BlockData.GetLength(2)];
            
            float nx = 0.0F;
            float ny = 0.0F;
            float nz = 0.0F;

            for (int x = 0; x < map.BlockData.GetLength(0); x++)
                for (int y = 0; y < map.BlockData.GetLength(1); y++)
                    for (int z = 0; z < map.BlockData.GetLength(2); z++)
                    {
                        nx = ((float)x / map.Width) - 0.5F;
                        ny = ((float)y / map.Height) - 0.5F;
                        nz = ((float)z / map.Length) - 0.5F;

                        perlinTunnel[x, y, z] = AffineTransformation.MapToWorld(
                            perlin.CreateOctave(1.6F * nx, 1.6F * ny, 1.6F * nz, 4), MinWorldY, MaxWorldY);
                    }

            // Create tunnels
            for (int x = 0; x < map.BlockData.GetLength(0); x++)
                for (int y = 0; y < map.BlockData.GetLength(1); y++)
                    for (int z = 0; z < map.BlockData.GetLength(2); z++)
                    {
                        if (perlinTunnel[x, y, z] < 115) map.BlockData[x, y, z].Shape = Block.SHAPE.Empty;

                        if (perlinTunnel[x, y, z] >= 115) map.BlockData[x, y, z].Style = Block.STYLE.Blue;
                        if (perlinTunnel[x, y, z] >= 120) map.BlockData[x, y, z].Style = Block.STYLE.Red;
                        if (perlinTunnel[x, y, z] >= 125) map.BlockData[x, y, z].Style = Block.STYLE.Yellow;
                        if (perlinTunnel[x, y, z] >= 130) map.BlockData[x, y, z].Style = Block.STYLE.Pink;
                        if (perlinTunnel[x, y, z] >= 135) map.BlockData[x, y, z].Style = Block.STYLE.Pavement;
                    }
            
            return map;
        }
    }
}