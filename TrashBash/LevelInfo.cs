using System;
using System.Collections.Generic;
using System.Text;

namespace TrashBash
{
    public class LevelInfo
    {
        public int LevelNum;
        public bool Cleared;
        public bool Shop;
        public bool ItemRoom;
        public bool Loaded;

        public LevelInfo(int levelNum, bool cleared, bool shop, bool itemRoom, bool loaded)
        {
            this.Cleared = cleared;
            this.LevelNum = levelNum;
            this.Shop = shop;
            this.ItemRoom = itemRoom;
            this.Loaded = loaded;
        }
    }
}