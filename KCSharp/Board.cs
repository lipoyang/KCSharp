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
        public static bool operator ==(Move m1, Move m2)
        {
            return m1.data == m2.data;
        }
        public static bool operator !=(Move m1, Move m2)
        {
            return m1.data != m2.data;
        }
    }

    // ビット位置→座標変換器
    class BitPos2Coord
    {
        public static int[] x = [
            0,1,2,3,4,
            0,1,2,3,4,
            0,1,2,3,4,
            0,1,2,3,4,
            0,1,2,3,4
        ];
        public static int[] y = [
            0,0,0,0,0,
            1,1,1,1,1,
            2,2,2,2,2,
            3,3,3,3,3,
            4,4,4,4,4
        ];
    }

    // 盤面データ
    struct Board
    {
        /**********  定数  **********/
        // マスの値
        public const int BLACK = 0;     // 先手
        public const int WHITE = 1;     // 後手
        public const int NONE = -1;     // 石が無い
        // 盤のサイズ (5×5マス)
        public const int SIZE = 5;

        /**********  変数  **********/
        // 最後の着手
        public Move lastMoveB;
        public Move lastMoveW;
        // 盤面
        public UInt32 blackStones; // (x,y) -> bit位置: (x + y * SIZE)
        public UInt32 whiteStones; // (x,y) -> bit位置: (x + y * SIZE)
        // 手番持ちのプレイヤー（先手 or 後手）
        public int turn;

        /**********  メソッド  **********/
        // 最後の着手を取得
        public Move getLastMove(int player)
        {
            return (player == BLACK) ? lastMoveB : lastMoveW;
        }
        public void getLastMove(int player, out Move move)
        {
            move = (player == BLACK) ? lastMoveB : lastMoveW;
        }

        // 最後の着手を設定
        public void setLastMove(int player, Move move)
        {
            if (player == BLACK) {
                lastMoveB = move;
            } else {
                lastMoveW = move;
            }
        }

        // 盤面を初期状態にリセットする
        public void reset()
        {
            // 盤面をクリア
            blackStones = 0x0000000;
            whiteStones = 0x0000000;
            turn = 0; // 先手
            lastMoveB = Move.NONE;
            lastMoveW = Move.NONE;
        }
        public void reset(Kifu black, Kifu white)
        {
            reset();

            // 初期配置
            for (int i = 0; i < 4; i++)
            {
                int bx = black.stones[i].x;
                int by = black.stones[i].y;
                setStone(bx, by, black.player);
                int wx = white.stones[i].x;
                int wy = white.stones[i].y;
                setStone(wx, wy, white.player);
            }
        }

        // 指定された場所の石を取得する
        public int getStone(int x, int y)
        {
            UInt32 mask = 1u << (x + y * SIZE);

            if ((blackStones & mask) != 0) return BLACK;
            if ((whiteStones & mask) != 0) return WHITE;
            return NONE;
        }

        // 指定された場所の石を設定する
        public void setStone(int x, int y, int owner)
        {
            UInt32 mask = 1u << (x + y * SIZE);

            if (owner == BLACK)
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

        // 盤面の範囲内か判定する
        private static bool isInBoard(int x, int y)
        {
            if ((x < 0) || (x >= SIZE)) return false;
            if ((y < 0) || (y >= SIZE)) return false;
            return true;
        }

        // そこに自分の石があるか判定する
        public bool isMyStone(Position pos)
        {
            int mine = turn;
            if (!isInBoard(pos.x, pos.y)) return false;
            if (getStone(pos.x, pos.y) != mine) return false;
            return true;
        }

        // そこに石がないことを判定する
        public bool isNoStone(Position pos)
        {
            if (!isInBoard(pos.x, pos.y)) return false;
            if (getStone(pos.x, pos.y) != NONE) return false;
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
            Move last;
            getLastMove(turn, out last);
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
            // 石の移動
            Position p1 = move.from;
            clearStone(p1.x, p1.y);
            Position p2 = move.to;
            setStone(p2.x, p2.y, turn);

            // 最後の着手を更新
            setLastMove(turn, move);

            // 手番交替
            turn = 1 - turn;
        }

        // 正方形か判定する
        public bool isSquare(int player)
        {
            UInt32 stones = (player == BLACK) ? blackStones : whiteStones;

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
            
            int tz = BitOperations.TrailingZeroCount(stones);
            int x = BitPos2Coord.x[tz];
            int y = BitPos2Coord.y[tz];

            // 間四判定
            if ((stones & 0x0001CE7) != 0)
            {
                if (x <= 2 && y <= 2)
                {
                    UInt32 shifted = stones >> tz;
                    if (shifted == 0x0001405) return true;
                }
            }
            // 十四判定
            if ((stones & 0x00039CE) != 0)
            {
                if (x >= 1 && x <= 3 && y <= 2)
                {
                    UInt32 shifted = stones >> (tz - 1);
                    if (shifted == 0x00008A2) return true;
                }
            }
            // 方四判定
            if ((stones & 0x007BDEF) != 0)
            {
                if (x <= 3 && y <= 3)
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
            int player = turn;
            UInt32 stones = (player == BLACK) ? blackStones : whiteStones;
            Move lastMove = getLastMove(player);

            // 盤面から有効な着手を探す
            int cnt = 0;
            while (stones != 0 && cnt < 4)
            {
                // 最下位の 1 ビットの位置を取得
                int tz = BitOperations.TrailingZeroCount(stones);
                // 最下位の 1 ビットを落とす
                stones &= stones - 1;

                // 座標に変換
                int x = BitPos2Coord.x[tz];
                int y = BitPos2Coord.y[tz];
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
                        if ((to == lastMove.from) && (from == lastMove.to))
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

    // 大パンチチェッカー
    class DaiPunch
    {
        // 大パンチチェック用の候補手バッファ
        private Move[] movesBuffer = new Move[32];

        public Board positions = new Board();

        // 大パンチ位置チェック
        public void check(Board board)
        {
            positions.reset();

            Board _board = board;
            for (int player = Board.BLACK; player <= Board.WHITE; player++)
            {
                _board.turn = player;
                Span<Move> nextMoves = new Span<Move>(movesBuffer, 0, 32);
                int cnt = _board.enumNextMoves(movesBuffer);
                for (int i = 0; i < cnt; i++)
                {
                    Board nextBoard = _board;
                    nextBoard.doMove(nextMoves[i]);
                    if (nextBoard.isSquare(player))
                    {
                        int x = nextMoves[i].to.x;
                        int y = nextMoves[i].to.y;
                        positions.setStone(x, y, player);
                    }
                }
            }
        }

        // リセットする
        public void reset()
        {
            positions.reset();
        }

        // 指定された場所の石を取得する
        public int getStone(int x, int y)
        {
            return positions.getStone(x, y);
        }
    }
}