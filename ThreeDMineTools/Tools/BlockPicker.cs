﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.VisualBasic.FileIO;

namespace ThreeDMineTools.Tools;

public class BlockPicker
{
    private Dictionary<(byte, byte), Color> blocks = new Dictionary<(byte, byte), Color>();

    public void Init()
    {
        //// Wool
        //blocks[(35, 0)] = Color.FromRgb(255, 255, 255);
        //blocks[(35, 1)] = Color.FromRgb(234, 130, 60);
        //blocks[(35, 2)] = Color.FromRgb(188, 67, 199);
        //blocks[(35, 3)] = Color.FromRgb(110, 142, 212);
        //blocks[(35, 4)] = Color.FromRgb(206, 192, 30);
        //blocks[(35, 5)] = Color.FromRgb(77, 207, 65);
        //blocks[(35, 6)] = Color.FromRgb(228, 167, 184);
        //blocks[(35, 7)] = Color.FromRgb(76, 76, 76);
        //blocks[(35, 8)] = Color.FromRgb(156, 163, 163);
        //blocks[(35, 9)] = Color.FromRgb(45, 133, 171);
        //blocks[(35, 10)] = Color.FromRgb(147, 77, 209);
        //blocks[(35, 11)] = Color.FromRgb(44, 59, 175);
        //blocks[(35, 12)] = Color.FromRgb(99, 59, 32);
        //blocks[(35, 13)] = Color.FromRgb(64, 89, 28);
        //blocks[(35, 14)] = Color.FromRgb(187, 52, 47);
        //blocks[(35, 15)] = Color.FromRgb(28, 24, 24);

        //// Clay
        //blocks[(82, 0)] = Color.FromRgb(174, 184, 213);
        //blocks[(159, 0)] = Color.FromRgb(208, 174, 157);
        //blocks[(159, 1)] = Color.FromRgb(159, 83, 37);
        //blocks[(159, 2)] = Color.FromRgb(147, 85, 108);
        //blocks[(159, 3)] = Color.FromRgb(115, 110, 137);
        //blocks[(159, 4)] = Color.FromRgb(188, 134, 36);
        //blocks[(159, 5)] = Color.FromRgb(103, 117, 52);
        //blocks[(159, 6)] = Color.FromRgb(159, 75, 74);
        //blocks[(159, 7)] = Color.FromRgb(57, 42, 36);
        //blocks[(159, 8)] = Color.FromRgb(134, 107, 98);
        //blocks[(159, 9)] = Color.FromRgb(86, 91, 91);
        //blocks[(159, 10)] = Color.FromRgb(115, 68, 84);
        //blocks[(159, 11)] = Color.FromRgb(73, 59, 91);
        //blocks[(159, 12)] = Color.FromRgb(77, 50, 36);
        //blocks[(159, 13)] = Color.FromRgb(76, 83, 42);
        //blocks[(159, 14)] = Color.FromRgb(141, 61, 47);
        //blocks[(159, 15)] = Color.FromRgb(37, 22, 16);
        //blocks[(172, 0)] = Color.FromRgb(146, 88, 62);
        using (TextFieldParser parser = new TextFieldParser(Assembly.GetExecutingAssembly().GetManifestResourceStream("ThreeDMineTools.Textures.blocks.csv")))
        {
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(";");
            parser.ReadFields();
            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
                var block = (byte.Parse(fields[0]), byte.Parse(fields[1]));
                blocks[block] = (Color)ColorConverter.ConvertFromString("#FF" + fields[2]);
            }
        }
    }
    public void Init(Dictionary<(byte, byte), Color> blocksColors)
    {
        blocks = blocksColors;
    }
    public void Init(List<(byte, byte)> blocksFilter)
    {
        using (TextFieldParser parser = new TextFieldParser(Assembly.GetExecutingAssembly().GetManifestResourceStream("ThreeDMineTools.Textures.blocks.csv")))
        {
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(";");
            parser.ReadFields();
            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
                var block = (byte.Parse(fields[0]), byte.Parse(fields[1]));
                if (blocksFilter.Contains(block))
                    blocks[block] = (Color)ColorConverter.ConvertFromString("#FF" + fields[2]);
            }
        }
    }

    public (byte, byte) GetBlockByColor(Color color)
    {
        if (blocks.Count == 0)
            Init();
        (byte, byte) MinColor = (1, 0);

        var tmpColor = System.Drawing.Color.FromArgb(255, color.R, color.G, color.B);
        float minDif = float.MaxValue;
        var (h, s, v) = (tmpColor.GetHue(), tmpColor.GetSaturation(), tmpColor.GetBrightness());
        foreach (var block in blocks.Keys)
        {
            var tmpColor2 = System.Drawing.Color.FromArgb(255, blocks[block].R, blocks[block].G, blocks[block].B);
            var (h2, s2, v2) = (tmpColor2.GetHue(), tmpColor2.GetSaturation(), tmpColor2.GetBrightness());
            int dif = (int)Math.Sqrt(Math.Pow(blocks[block].R - color.R, 2) + Math.Pow(blocks[block].G - color.G, 2) + Math.Pow(blocks[block].B - color.B, 2) + Math.Pow(h - h2, 2));
            if (dif < minDif)
            {
                minDif = dif;
                MinColor = block;
            }
        }

        return MinColor;
    }
}