using System.Runtime.InteropServices;

namespace KCSharp
{
    // 位置データ
    [StructLayout(LayoutKind.Explicit)]
    struct Position
    {
        public static readonly Position NONE = new Position(0xFFFF);

        [FieldOffset(0)]
        public byte x;
        [FieldOffset(1)]
        public byte y;
        [FieldOffset(0)]
        public UInt16 data;

        public Position(int x, int y)
        {
            this.x = (byte)x;
            this.y = (byte)y;
        }
        public Position(UInt16 rawdata)
        {
            this.data = rawdata;
        }
        public static bool operator ==(Position p1, Position p2)
        {
            return p1.data == p2.data;
        }
        public static bool operator !=(Position p1, Position p2)
        {
            return p1.data != p2.data;
        }
    }

    // 着手データ
    [StructLayout(LayoutKind.Explicit)]
    struct Move
    {
        public static readonly Move NONE = new Move(0xFFFFFFFF);

        [FieldOffset(0)]
        public Position from; // 移動元
        [FieldOffset(2)]
        public Position to;   // 移動先
        [FieldOffset(0)]
        public UInt32 data;

        public Move(Position from, Position to)
        {
            this.from = from;
            this.to = to;
        }
        public Move(UInt32 rawdata)
        {
            this.data = rawdata;
        }
        public static bool operator ==(Move m1, Move m2)
        {
            return m1.data == m2.data;
        }
        public static bool operator !=(Move m1, Move m2)
        {
            return m1.data != m2.data;
        }
    }

    // 盤面データ
    struct Board
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
        public Move lastMoveB;
        public Move lastMoveW;
        public Move getLastMove(int player)
        {
            return (player == FIRST_MOVE) ? lastMoveB : lastMoveW;
        }
        public void setLastMove(int player, Move move)
        {
            if (player == FIRST_MOVE) {
                lastMoveB = move;
            } else {
                lastMoveW = move;
            }
        }

        // 盤面
        public UInt32 blackStones; // (x,y) -> bit位置: (x + y * SIZE)
        public UInt32 whiteStones; // (x,y) -> bit位置: (x + y * SIZE)

        // 指定された場所の石を取得する
        public int getStone(int x, int y)
        {
            UInt32 mask = 1u << (x + y * SIZE);

            if ((blackStones & mask) != 0) return FIRST_MOVE;
            if ((whiteStones & mask) != 0) return SECOND_MOVE;
            return NO_STONE;
        }

        // 指定された場所の石を設定する
        public void setStone(int x, int y, int owner)
        {
            UInt32 mask = 1u << (x + y * SIZE);

            if (owner == FIRST_MOVE)
            {
                blackStones |= mask;
                whiteStones &= ~mask;
            }
            else if (owner == SECOND_MOVE)
            {
                blackStones &= ~mask;
                whiteStones |= mask;
            }
        }

        // 指定された場所の石を取り除く
        public void clearStone(int x, int y)
        {
            UInt32 mask = 1u << (x + y * SIZE);

            blackStones &= ~mask;
            whiteStones &= ~mask;
        }

        // 対局開始からの手順数
        public int turn;

        // 手番持ちのプレイヤー（先手 or 後手）
        public int turnHolder {
            get {
                return turn % 2;
            }
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
            // 盤面をクリア
            blackStones = 0x0000000;
            whiteStones = 0x0000000;
            turn = 0;
            lastMoveB = Move.NONE;
            lastMoveW = Move.NONE;

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
                        setStone(bx, by, black.player);
                        int wx = white.stones[i].x;
                        int wy = white.stones[i].y;
                        setStone(wx, wy, white.player);
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
                        } while (getStone(x, y) != NO_STONE);
                        setStone(x, y, (i % 2 == 0) ? FIRST_MOVE : SECOND_MOVE);
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
            if (getStone(pos.x, pos.y) != mine) return false;
            return true;
        }

        // そこに石がないことを判定する
        public bool isNoStone(Position pos)
        {
            if (!isInBoard(pos.x, pos.y)) return false;
            if (getStone(pos.x, pos.y) != NO_STONE) return false;
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
            Move last = getLastMove(turnHolder);
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
            clearStone(p1.x, p1.y);
            Position p2 = move.to;
            setStone(p2.x, p2.y, mine);

            // 最後の着手を更新
            setLastMove(mine, move);
        }

        // 正方形か判定する
        public bool isSquare(int player)
        {
            // 石の座標を取得
            int[] sx = new int[4];
            int[] sy = new int[4];
            int idx = 0;
            for (int x = 0; x < SIZE; x++) {
                for (int y = 0; y < SIZE; y++) {
                    if (getStone(x, y) == player) {
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
            int min = d[0];
            int cnt = 0;
            for (int i = 1; i < 6; i++) {
                if (d[i] == min) {
                    cnt++;
                }
                else if (d[i] < min){
                    cnt = 0;
                    min = d[i];
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
                            Move last = getLastMove(turnHolder);
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