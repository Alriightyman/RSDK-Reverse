﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace RSDKv3
{
    class Tiles128x128
    {
        //The file is always 96kb in size

        public List<Tile128> BlockList { get; set; }

        public Tiles128x128(Reader strm)
        {
            BlockList = new List<Tile128>();
            byte[] mappingEntry = new byte[3];
            Tile128 currentBlock = new Tile128();
            int tileIndex = 0;
            while (strm.Read(mappingEntry, 0, mappingEntry.Length) > 0)
            {
                if (tileIndex >= currentBlock.Mapping.Length)
                {
                    tileIndex = 0;
                    BlockList.Add(currentBlock);
                    currentBlock = new Tile128();
                }
                mappingEntry[0] = (byte)(mappingEntry[0] - (mappingEntry[0] >> 6 << 6));
                currentBlock.Mapping[tileIndex].VisualPlane = (byte)(mappingEntry[0] >> 4);
                mappingEntry[0] = (byte)(mappingEntry[0] - (mappingEntry[0] >> 4 << 4));
                currentBlock.Mapping[tileIndex].Direction = (byte)(mappingEntry[0] >> 2);
                mappingEntry[0] = (byte)(mappingEntry[0] - (mappingEntry[0] >> 2 << 2));
                currentBlock.Mapping[tileIndex].Tile16x16 = (ushort)((mappingEntry[0] << 8) + mappingEntry[1]);
                currentBlock.Mapping[tileIndex].CollisionFlag0 = (byte)(mappingEntry[2] >> 4);
                currentBlock.Mapping[tileIndex].CollisionFlag1 = (byte)(mappingEntry[2] - (mappingEntry[2] >> 4 << 4));
                tileIndex++;
            }
            if (tileIndex >= currentBlock.Mapping.Length)
            {
                tileIndex = 0;
                BlockList.Add(currentBlock);
                currentBlock = new Tile128();
            }
        }

        public void Write(string filename)
        {
            using (Writer writer = new Writer(filename))
                this.Write(writer);
        }

        public void Write(System.IO.Stream stream)
        {
            using (Writer writer = new Writer(stream))
                this.Write(writer);
        }

        internal void Write(Writer writer)
        {

        }

    }

    public class Tile128
    {
        public Tile16[] Mapping;
        public Tile128()
        {
            Mapping = new Tile16[8 * 8];
            for (int i = 0; i < Mapping.Length; i++)
            {
                Mapping[i] = new Tile16();
            }
        }

        public Bitmap Render(Image tiles)
        {
            Bitmap retval = new Bitmap(128, 128);
            using (Graphics rg = Graphics.FromImage(retval))
            {
                int i = 0;
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        Rectangle destRect = new Rectangle(x * 16, y * 16, 16, 16);
                        Rectangle srcRect = new Rectangle(0, Mapping[i].Tile16x16 * 16, 16, 16);
                        using (Bitmap tile = new Bitmap(16, 16))
                        {
                            using (Graphics tg = Graphics.FromImage(tile))
                            {
                                tg.DrawImage(tiles, 0, 0, srcRect, GraphicsUnit.Pixel);
                            }
                            if (Mapping[i].Direction == 1)
                            {
                                tile.RotateFlip(RotateFlipType.RotateNoneFlipX);
                            }
                            else if (Mapping[i].Direction == 2)
                            {
                                tile.RotateFlip(RotateFlipType.RotateNoneFlipY);
                            }
                            else if (Mapping[i].Direction == 3)
                            {
                                tile.RotateFlip(RotateFlipType.RotateNoneFlipXY);
                            }
                            rg.DrawImage(tile, destRect);
                        }
                        i++;
                    }
                }
            }
            return retval;
        }
    }

    public class Tile16
    {
        public byte VisualPlane { get; set; }
        public byte Direction { get; set; }
        public ushort Tile16x16 { get; set; }
        public byte CollisionFlag0 { get; set; }
        public byte CollisionFlag1 { get; set; }
    }
}