using System.Numerics;
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
        public Move invert()
        {
            return new Move(this.to, this.from);
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
            } else {
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
            if ((last.from == to) && (last.to == from))
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
            UInt32 stones = (player == FIRST_MOVE) ? blackStones : whiteStones;

            // 端四判定
            if (stones == 0x1100011) {
                return true;
            }
            // 菱四判定
            if (stones == 0x0404404) {
                return true;
            }
            // 崩四判定
            if (stones == 0x0808202 || stones == 0x0280028)
            {
                return true;
            }
            // 格四判定
            if ((stones & 0x0000063) != 0)
            {
                const UInt32 pattern = 0x0048009;
                if ( stones       == pattern) return true;
                if ((stones >> 1) == pattern) return true;
                if ((stones >> 5) == pattern) return true;
                if ((stones >> 6) == pattern) return true;
            }
            // 桂四判定
            if ((stones & 0x000018C) != 0)
            {
                const UInt32 pattern = 0x0012024;
                if ( stones       == pattern) return true;
                if ((stones >> 1) == pattern) return true;
                if ((stones >> 5) == pattern) return true;
                if ((stones >> 6) == pattern) return true;
            }
            if ((stones & 0x00000C6) != 0)
            {
                const UInt32 pattern = 0x0020502;
                if ( stones       == pattern) return true;
                if ((stones >> 1) == pattern) return true;
                if ((stones >> 5) == pattern) return true;
                if ((stones >> 6) == pattern) return true;
            }
            // 間四判定
            if ((stones & 0x0001CE7) != 0)
            {
                int tz = BitOperations.TrailingZeroCount(stones);
                if ((tz % 5 <= 2) && (tz / 5 <= 2))
                {
                    UInt32 shifted = stones >> tz;
                    if (shifted == 0x0001405) return true;
                }
            }
            // 十四判定
            if ((stones & 0x00039CE) != 0)
            {
                int tz = BitOperations.TrailingZeroCount(stones);
                int _tz = tz - 1;
                if ((_tz % 5 <= 2) && (_tz / 5 <= 2))
                {
                    UInt32 shifted = stones >> _tz;
                    if (shifted == 0x00008A2) return true;
                }
            }
            // 方四判定
            if ((stones & 0x007BDEF) != 0)
            {
                int tz = BitOperations.TrailingZeroCount(stones);
                if ((tz % 5 <= 3) && (tz / 5 <= 3))
                {
                    UInt32 shifted = stones >> tz;
                    if (shifted == 0x0000063) return true;
                }
            }
            return false;
        }

        // 次の手を列挙する
        public int enumNextMoves(Span<Move> nextMoves)
        {
            int moveCount = 0;
            UInt32 stones = (turnHolder == FIRST_MOVE) ? blackStones : whiteStones;
            Move urochoro = getLastMove(turnHolder).invert(); // うろちょろ禁止着手

            // 盤面から有効な着手を探す
            int cnt = 0;
            while (stones != 0 && cnt < 4)
            {
                // 最下位の 1 ビットの位置を取得
                int tz = BitOperations.TrailingZeroCount(stones);
                // 最下位の 1 ビットを落とす
                stones &= stones - 1;

                // 座標に変換
                int x = tz % SIZE;
                int y = tz / SIZE;
                Position from = new Position(x, y);
                cnt++;

                // 隣接するマスを調べる
                UInt32 occupied = blackStones | whiteStones;
                int dx1 = (x > 0) ? x - 1 : 0;
                int dx2 = (x < SIZE - 1) ? x + 1 : SIZE - 1;
                int dy1 = (y > 0) ? y - 1 : 0;
                int dy2 = (y < SIZE - 1) ? y + 1 : SIZE - 1;
                for (int dx = dx1; dx <= dx2; dx++)
                {
                    for (int dy = dy1; dy <= dy2; dy++)
                    {
                        // 元の位置はスキップ
                        if (dx == x && dy == y) continue;

                        // 石がないことをチェック
                        UInt32 pos = 1u << (dx + dy * SIZE);
                        if ((pos & occupied) != 0) continue;

                        Position to = new Position(dx, dy);

                        // うろちょろ禁止ルールチェック
                        if ((to == urochoro.to) && (from == urochoro.from))
                        {
                            continue;
                        }

                        // 有効な着手を追加
                        Move move = new Move(from, to);
                        nextMoves[moveCount] = move;
                        moveCount++;
                    } // for dx
                } // for dy
            } // while (stones != 0 && cnt < 4)
            return moveCount;
        }
    }
}