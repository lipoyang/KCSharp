using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KCSharp
{
    // 棋譜クラス
    class Kifu
    {
        public const int BLACK = 0;  // 先手
        public const int WHITE = 1;  // 後手
        public const int NONE  = -1; // なし
        public const int SIZE = 5;   // 盤のサイズ (5×5マス)

        public int player = NONE; // 先手/後手
        public Position[] stones = new Position[4]; // 石の位置

        public Kifu(string str)
        {
            player = NONE; // 無効なデータ

            // 文字数チェック
            if (str.Length < 12) return; // 文字数が足りない

            // 先手か後手か
            if (str[0] == 'B' || str[0] == 'b') {
                player = BLACK;
            }
            else if (str[0] == 'W' || str[0] == 'w') {
                player = WHITE;
            } else {
                return; // 1文字目が不正
            }

            // 石の座標を解釈
            var s = str.Substring(1);
            var items = s.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (items.Length != 4) return; // 座標データが4個無い
            for (int i = 0; i < 4; i++)
            {
                var item = items[i];
                if (item.Length != 2) return; // 座標データが2文字でない
                int x = item[0] - '0' - 1;
                int y = item[1] - '0' - 1;
                if (x < 0 || x >= SIZE) return; // xの値が不正
                if (y < 0 || y >= SIZE) return; // yの値が不正
                stones[i] = new Position(x, y);
            }
        }
    }

    // 初期局面クラス
    class InitialPosition
    {
        // 初期局面の棋譜ファイルパスと棋譜データ
        private string InitialPositionFilePath;
        private int InitialPositionNum;
        public Kifu[] black;
        public Kifu[] white;

        public InitialPosition(string path, int max)
        {
            InitialPositionNum = max;
            InitialPositionFilePath = path;
            black = new Kifu[InitialPositionNum];
            white = new Kifu[InitialPositionNum];
        }

        // 初期局面のロード
        public bool load()
        {
            try
            {
                // 初期局面を読み込む
                var lines = File.ReadAllLines(InitialPositionFilePath);
                if (lines.Length < InitialPositionNum * 3 - 1)
                {
                    return false; // 行数が足りない
                }
                for (int i = 0; i < InitialPositionNum; i++)
                {
                    string strB = lines[i * 3];
                    string strW = lines[i * 3 + 1];
                    Kifu kifB = new Kifu(strB);
                    Kifu kifW = new Kifu(strW);
                    if (kifB.player != Kifu.BLACK) return false; // 先手の棋譜が不正
                    if (kifW.player != Kifu.WHITE) return false; // 後手の棋譜が不正
                    black[i] = kifB;
                    white[i] = kifW;
                }
                return true;
            }catch{
                return false; // ファイルアクセスエラーなど
            }
        }
    }
}
