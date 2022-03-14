using System;
using System.Collections.Generic;
using System.Text;

namespace TrashBash
{
    public class LevelInfo
    {
        public int levelNum;
        public bool cleared;

        public LevelInfo(int levelNum, bool cleared)
        {
            this.cleared = cleared;
            this.levelNum = levelNum;
        }
    }
}