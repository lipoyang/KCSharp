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

        // 乱数生成器
        private Random rand = new Random();

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
        public void reset(bool initialPlase)
        {
            for (int x = 0; x < SIZE; x++)
            {
                for (int y = 0; y < SIZE; y++)
                {
                    stone[x, y] = NO_STONE;
                }
            }

            // 初期配置を行うかどうか
            if (initialPlase == false) return;

            // ランダムに初期配置
            Random rand = new Random();
            for (int i = 0; i < 8; i++)
            {
                int x, y;
                do
                {
                    x = rand.Next(SIZE);
                    y = rand.Next(SIZE);
                } while (stone[x, y] != NO_STONE);
                stone[x, y] = (i % 2 == 0) ? FIRST_MOVE : SECOND_MOVE;
            }

            turn = 0;
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
        }

        // 正方形か判定する
        public bool isSquare(int player)
        {
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
        }

        // 評価関数
        public int evalFunction(int player)
        {
            // TODO: 評価関数を実装する
            int eval = rand.Next(100);
            return eval;
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