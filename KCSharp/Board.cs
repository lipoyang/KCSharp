namespace KCSharp
{
    // 位置クラス
    class Position
    {
        public static readonly Position NONE = new Position(-1, -1);

        // 着手位置
        public int x;
        public int y;

        // コンストラクタ(位置を指定)
        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    // 着手クラス
    class Move
    {
        public static readonly Move NONE = new Move(Position.NONE, Position.NONE);

        public Position from;   // 着手前位置
        public Position to;     // 着手先位置

        // コンストラクタ(位置を指定)
        public Move(Position from, Position to)
        {
            this.from = from;
            this.to = to;
        }

        // コピーする
        public Move copy()
        {
            return new Move(new Position(this.from.x, this.from.y),
                            new Position(this.to.x, this.to.y));
        }
    }

    // 盤面クラス
    class Board
    {
        // 先手
        public const int FIRST_MOVE = 0;
        // 後手
        public const int SECOND_MOVE = 1;
        // 石が無い
        public const int NO_STONE = -1;
        // 盤のサイズ (5×5マス)
        public const int SIZE = 5;

        // 盤面種別
        public enum InitialPosition
        {
            NONE   = -1,    // 石無し
            FIXED  = 0,     // 10番勝負
            RANDOM = 1      // ランダム配置
        }

        // 最後の着手
        public Move[] lastMove = new Move[2]; // 0:先手,1:後手

        // 盤面
        public int[,] stone = new int[SIZE, SIZE];

        // 対局開始からの手順数
        public int turn;

        // 手番持ちのプレイヤー（先手 or 後手）
        public int turnHolder
        {
            get
            {
                return turn % 2;
            }
        }

        // 盤面のコピーを生成する
        public Board copy()
        {
            Board board = new Board();

            for (int x = 0; x < SIZE; x++)
            {
                for (int y = 0; y < SIZE; y++)
                {
                    board.stone[x, y] = this.stone[x, y];
                }
            }
            board.turn = this.turn;
            board.lastMove[0] = this.lastMove[0].copy();
            board.lastMove[1] = this.lastMove[1].copy();

            return board;
        }

        // 盤面の範囲内か判定する
        private static bool isInBoard(int x, int y)
        {
            if ((x < 0) || (x >= SIZE)) return false;
            if ((y < 0) || (y >= SIZE)) return false;
            return true;
        }

        // 盤面を初期状態にリセットする
        public void reset(InitialPosition type, Kifu black = null, Kifu white = null)
        {
            for (int x = 0; x < SIZE; x++)
            {
                for (int y = 0; y < SIZE; y++)
                {
                    stone[x, y] = NO_STONE;
                }
            }

            turn = 0;
            lastMove[0] = Move.NONE;
            lastMove[1] = Move.NONE;

            // 初期配置
            switch (type)
            {
                // 10番勝負の初期配置
                case InitialPosition.FIXED:
                    // 盤面に石を配置
                    for (int i = 0; i < 4; i++)
                    {
                        int bx = black.stones[i].x;
                        int by = black.stones[i].y;
                        stone[bx, by] = black.player;
                        int wx = white.stones[i].x;
                        int wy = white.stones[i].y;
                        stone[wx, wy] = white.player;
                    }
                    break;

                // ランダム配置
                case InitialPosition.RANDOM:
                    Random rand = new Random();
                    for (int i = 0; i < 8; i++)
                    {
                        int x, y;
                        do {
                            x = rand.Next(SIZE);
                            y = rand.Next(SIZE);
                        } while (stone[x, y] != NO_STONE);
                        stone[x, y] = (i % 2 == 0) ? FIRST_MOVE : SECOND_MOVE;
                    }
                    break;

                // 石無し
                case InitialPosition.NONE:
                default:
                    break;
            }
        }

        // そこに自分の石があるか判定する
        public bool isMyStone(Position pos)
        {
            int mine = turnHolder;
            if (!isInBoard(pos.x, pos.y)) return false;
            if (stone[pos.x, pos.y] != mine) return false;
            return true;
        }

        // そこに石がないことを判定する
        public bool isNoStone(Position pos)
        {
            if (!isInBoard(pos.x, pos.y)) return false;
            if (stone[pos.x, pos.y] != NO_STONE) return false;
            return true;
        }

        // 有効な着手か判定する
        public bool isAvailableMove(Position from, Position to)
        {
            // 着手前の位置に自分の石があることをチェック
            if(isMyStone(from) == false) return false;

            // 着手先の位置に石が無いことをチェック
            if(isNoStone(to) == false) return false;

            // うろちょろ禁止ルールチェック
            Move last = lastMove[turnHolder];
            if ((last.from.x == to.x) && (last.from.y == to.y) &&
                (last.to.x == from.x) && (last.to.y == from.y))
            {
                return false;
            }

            // 選択中の石に隣接しているかチェック
            int dx = Math.Abs(to.x - from.x);
            int dy = Math.Abs(to.y - from.y);
            if ((dx > 1) || (dy > 1)) return false;

            return true;
        }
        
        // 着手する
        public void doMove(Move move)
        {
            // 先手/後手か
            int mine = turnHolder;
            // int his = (mine == FIRST_MOVE) ? SECOND_MOVE : FIRST_MOVE;
            // 手順数を進める
            turn++;

            // 石の移動
            Position p1 = move.from;
            stone[p1.x, p1.y] = NO_STONE;
            Position p2 = move.to;
            stone[p2.x, p2.y] = mine;

            // 最後の着手を更新
            lastMove[mine] = move.copy();
        }

        // 正方形か判定する
        public bool isSquare(int player)
        {
            bool result = isSquare(player, out int min, out int max);
            return result;
        }
        public bool isSquare(int player, out int min, out int max)
        {
            min = 1; max = 2;
#if false
            // 端四判定
            if ((stone[0, 0] == player) && (stone[0, 4] == player) &&
                (stone[4, 0] == player) && (stone[4, 4] == player))
            {
                return true;
            }
            // 菱四判定
            if ((stone[2, 0] == player) && (stone[0, 2] == player) &&
                (stone[4, 2] == player) && (stone[2, 4] == player))
            {
                return true;
            }
            // 崩四判定
            if ((stone[1, 0] == player) && (stone[0, 3] == player) &&
                (stone[4, 1] == player) && (stone[3, 4] == player))
            {
                return true;
            }
            if ((stone[0, 1] == player) && (stone[1, 4] == player) &&
                (stone[3, 0] == player) && (stone[4, 3] == player))
            {
                return true;
            }
            // 格四判定
            for(int x = 0; x <= 1; x++){
                for(int y = 0; y <= 1; y++){
                    if ((stone[x, y  ] == player) && (stone[x+3, y  ] == player) &&
                        (stone[x, y+3] == player) && (stone[x+3, y+3] == player))
                    {
                        return true;
                    }
                }
            }
            // 桂四判定
            for(int x = 0; x <= 1; x++){
                for(int y = 0; y <= 1; y++){
                    if ((stone[0+x, 1+y] == player) && (stone[1+x, 3+y] == player) &&
                        (stone[2+x, 0+y] == player) && (stone[3+x, 2+y] == player))
                    {
                        return true;
                    }
                    if ((stone[1+x, 0+y] == player) && (stone[3+x, 1+y] == player) &&
                        (stone[0+x, 2+y] == player) && (stone[2+x, 3+y] == player))
                    {
                        return true;
                    }
                }
            }
            // 間四判定
            for(int x = 0; x <= 2; x++){
                for(int y = 0; y <= 2; y++){
                    if ((stone[x, y  ] == player) && (stone[x+2, y  ] == player) &&
                        (stone[x, y+2] == player) && (stone[x+2, y+2] == player))
                    {
                        return true;
                    }
                }
            }
            // 十四判定
            for(int x = 0; x <= 2; x++){
                for(int y = 0; y <= 2; y++){
                    if ((stone[1+x, 0+y] == player) && (stone[0+x, 1+y] == player) &&
                        (stone[2+x, 1+y] == player) && (stone[1+x, 2+y] == player))
                    {
                        return true;
                    }
                }
            }
            // 方四判定
            for(int x = 0; x <= 3; x++){
                for(int y = 0; y <= 3; y++){
                    if ((stone[x, y  ] == player) && (stone[x+1, y  ] == player) &&
                        (stone[x, y+1] == player) && (stone[x+1, y+1] == player))
                    {
                        return true;
                    }
                }
            }
            return false;
#endif
            // 石の座標を取得
            int[] sx = new int[4];
            int[] sy = new int[4];
            int idx = 0;
            for (int x = 0; x < SIZE; x++) {
                for (int y = 0; y < SIZE; y++) {
                    if (stone[x, y] == player) {
                        sx[idx] = x;
                        sy[idx] = y;
                        idx++;
                        if(idx >= 4) break;
                    }
                }
                if (idx >= 4) break;
            }
            if (idx != 4) return false;

            // 4点間の距離の2乗を全部列挙（6個）
            int[] d = new int[6];
            idx = 0;
            for (int i = 0; i < 4; i++) {
                for (int j = i + 1; j < 4; j++) {
                    int dx = sx[i] - sx[j];
                    int dy = sy[i] - sy[j];
                    d[idx] = dx * dx + dy * dy;
                    idx++;
                }
            }

            // 正方形か判定
            min = d[0];
            max = d[0];
            int cnt = 0;
            for (int i = 1; i < 6; i++) {
                if (d[i] == min) {
                    cnt++;
                }
                else if (d[i] < min){
                    cnt = 0;
                    min = d[i];
                }
                if(d[i] > max) {
                    max = d[i];
                }
            }
            return (cnt == 3);
        }

        // 次の局面を列挙する
        public List<Move> enumNextMoves()
        {
            List<Move> nextMoves = new List<Move>();

            // 盤面から有効な着手を探す
            for (int x = 0; x < SIZE; x++)
            {
                for (int y = 0; y < SIZE; y++)
                {
                    // そこに自分の石があるかチェック
                    Position from = new Position(x, y);
                    if (isMyStone(from) == false) continue;

                    // 隣接するマスを調べる
                    for(int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            // 元の位置はスキップ
                            if (dx == 0 && dy == 0) continue;

                            // 石がないことをチェック
                            Position to = new Position(x + dx, y + dy);
                            if(isNoStone(to) == false) continue;

                            // うろちょろ禁止ルールチェック
                            Move last = lastMove[turnHolder];
                            if ((last.from.x == to.x) && (last.from.y == to.y) &&
                                (last.to.x == from.x) && (last.to.y == from.y))
                            {
                                continue;
                            }

                            // 有効な着手を追加
                            Move move = new Move(from, to);
                            nextMoves.Add(move);
                        }
                    }
                }
            }
            return nextMoves;
        }
    }
}