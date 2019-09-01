﻿using GBWorldGen.Core.Algorithms;
using GBWorldGen.Core.Algorithms.Abstractions;
using GBWorldGen.Core.Models;
using System;
using System.Collections.Generic;

namespace Algorithms.Generators
{
    public class BaseGenerator : WorldData, IGenerateWorld
    {       
        protected int Width { get; set; }
        protected int Length { get; set; }
        private Block.STYLE DefaultBlockStyle { get; }
        protected short[] YValues { get; set; }

        public BaseGenerator(int width, int length, Block.STYLE defaultBlockStyle = Block.STYLE.Grass)
        {
            Width = width;
            Length = length;
            DefaultBlockStyle = defaultBlockStyle;
            YValues = new short[Width * Length];
        }

        public virtual Map Generate(params float[] values)
        {
            Console.WriteLine("Generating map...");

            short originX = (short)(Width * -0.5d);
            short originZ = (short)(Length * -0.5d);
            List<Block> blocks = new List<Block>();

            for (int i = 0; i < YValues.Length; i++)
            {
                blocks.Add(new Block
                {
                    X = (short)(i / Length + originZ),
                    Y = YValues[i],
                    Z = (short)(i % Width + originX),
                    Shape = Block.SHAPE.Box,
                    Direction = Block.DIRECTION.East,
                    Style = DefaultBlockStyle
                });
            }

            return new Map
            {
                Width = (int)(Width * 2.5d),
                Length = (int)(Length * 2.5d),
                BlockData = blocks.ToArray()
            };
        }
    }
}
