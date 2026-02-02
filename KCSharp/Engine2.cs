using System.Diagnostics;
using System.Numerics;

namespace KCSharp
{
    // 思考エンジン 2号（探索：ミニマックス法、評価：速く詰むか+詰まないなら自乗形四率(SKR) ）
    class Engine2 : Engine
    {
        // デバッグ用
        const bool debugLog = false;

        // コンストラクタ (読みの深さと先手/後手を指定)
        public Engine2(int depth, int order) : base(depth, order) { }

        // 乱数生成器
        private Random rand = new Random();

        // 次の手の候補用バッファ (1局面最大32通り×最大探索深さ)
        private const int MAX_MOVE = 32;
        private const int MAX_DEPTH = 10;
        private Move[] moveBuffer = new Move[MAX_MOVE * MAX_DEPTH];

        // 次の手を取得する
        public override Move getNextMove(Board board)
        {
            // 次の手を考える
            int eval = readMinMax(board, 0);

            // 中断判定
            if (eval == int.MinValue)
            {
                canceled = true;
                return Move.NONE;
            }

            return bestMove;
        }

        // 自乗形四率(Square Keishi Rate)の計算 (0～100)
        int[] sx = new int[4];
        int[] sy = new int[4];
        public int getSKR (Board board, int player)
        {
            // 石の座標を取得
            const int SIZE = Board.SIZE;
            UInt32 stones = (player == Board.BLACK) ? board.blackStones : board.whiteStones;
            int idx = 0;
            while (stones != 0 && idx < 4)
            {
                // 最下位の 1 ビットの位置を取得
                int pos = BitOperations.TrailingZeroCount(stones);

                // 座標に変換
                sx[idx] = pos % SIZE;
                sy[idx] = pos / SIZE;
                idx++;

                // 最下位の 1 ビットを落とす
                stones &= stones - 1;
            }
            if (idx != 4) return 0; // 不正

            // 4点間の距離の2乗を全部列挙（6個）
            // そのうちの最大値と最小値を求める
            int min = int.MaxValue;
            int max = int.MinValue;
            idx = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    int dx = sx[i] - sx[j];
                    int dy = sy[i] - sy[j];
                    int dist = dx * dx + dy * dy;
                    if (dist < min) min = dist;
                    if (dist > max) max = dist;
                    idx++;
                }
            }

            // 自乗形四率(Square Keishi Rate)の計算 (0～100)
            int skr = 100 * 2 * min / max;
            return skr;
        }


        // 評価関数
        public int evalFunction(Board board)
        {
            // 先手の自乗形四率 * 重み + ランダム
            int eval_black = getSKR(board, Board.BLACK) + rand.Next(10);
            // 後手の自乗形四率 * 重み + ランダム
            int eval_white = getSKR(board, Board.WHITE) + rand.Next(10);

            // 評価値
            int eval = eval_black - eval_white;
            if (eval >=  100) eval =  99;
            if (eval <= -100) eval = -99;
            if (myOrder == Board.WHITE)
            {
                eval = -eval;
            }
            return eval;
        }

        // ミニマックス法による先読み (再帰)
        public int readMinMax(Board board, int depth)
        {
            // 中断判定
            if (canceling) return int.MinValue;

            // 先読み深さの末端に到達したら評価値を返す
            if (depth == maxDepth)
            {
                // 評価関数
                int eval = evalFunction(board);
                //Debug.Write(depth + ":" + eval + " ");
                return eval;
            }

            // 次の局面を列挙
            Span<Move> nextMoves = new Span<Move>(moveBuffer, MAX_MOVE * depth, MAX_MOVE);
            int moveCount = board.enumNextMoves(nextMoves);

            // 自分の手番なら最も自分に有利な手を選択（自分にとっての最善手）
            // 相手の手番なら最も自分に不利な手を選択（相手にとっての最善手）
            int best = (board.turnHolder == myOrder) ? int.MinValue : int.MaxValue;
            for (int i = 0; i < moveCount; i++)
            {
                Board nextBoard = board;
                nextBoard.doMove(nextMoves[i]);
                int eval; // 評価値

                // 決まり手か？ (正方形判定)
                if (nextBoard.isSquare(board.turnHolder))
                {
                    if (board.turnHolder == myOrder)
                    {
                        eval = (maxDepth - depth) * 100;
                    }
                    else
                    {
                        eval = -(maxDepth - depth) * 100;
                    }
                }
                // 決まり手でないなら再帰呼び出し
                else
                {
                    eval = readMinMax(nextBoard, depth + 1);
                    // 中断判定
                    if(eval == int.MinValue) return int.MinValue;
                }
                if(debugLog && depth == 0)
                {
                    Debug.WriteLine($"{i} : ({nextMoves[i].from.x}, {nextMoves[i].from.y})->({nextMoves[i].to.x}, {nextMoves[i].to.y}) : {eval}");
                }

                if ((board.turnHolder == myOrder) && (eval > best))
                {
                    best = eval;
                    if (depth == 0)
                    {
                        bestMove = nextMoves[i];
                    }
                }
                if ((board.turnHolder != myOrder) && (eval < best))
                {
                    best = eval;
                    if (depth == 0)
                    {
                        bestMove = nextMoves[i];
                    }
                }
            }
            if (debugLog && depth == 0)
            {
                Debug.WriteLine($"Move ({bestMove.from.x}, {bestMove.from.y})->({bestMove.to.x}, {bestMove.to.y}) : {best}");
            }
            return best;
        }
    }
}
