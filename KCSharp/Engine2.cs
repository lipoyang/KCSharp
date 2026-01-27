using System.Diagnostics;

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
        public int getSKR (Board board, int player)
        {
            // 石の座標を取得
            const int SIZE = Board.SIZE;
            int[,] stone = board.stone;
            int[] sx = new int[4];
            int[] sy = new int[4];
            int idx = 0;
            for (int x = 0; x < SIZE; x++)
            {
                for (int y = 0; y < SIZE; y++)
                {
                    if (stone[x, y] == player)
                    {
                        sx[idx] = x;
                        sy[idx] = y;
                        idx++;
                        if (idx >= 4) break;
                    }
                }
                if (idx >= 4) break;
            }
            if (idx != 4) return 0; // 不正

            // 4点間の距離の2乗を全部列挙（6個）
            int[] d = new int[6];
            idx = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    int dx = sx[i] - sx[j];
                    int dy = sy[i] - sy[j];
                    d[idx] = dx * dx + dy * dy;
                    idx++;
                }
            }

            // 正方形か判定
            int min = d[0];
            int max = d[0];
            int cnt = 0;
            for (int i = 1; i < 6; i++)
            {
                if (d[i] == min)
                {
                    cnt++;
                }
                else if (d[i] < min)
                {
                    cnt = 0;
                    min = d[i];
                }
                if (d[i] > max)
                {
                    max = d[i];
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
            int eval_black = getSKR(board, Board.FIRST_MOVE) + rand.Next(10);
            // 後手の自乗形四率 * 重み + ランダム
            int eval_white = getSKR(board, Board.SECOND_MOVE) + rand.Next(10);

            // 評価値
            int eval = eval_black - eval_white;
            if (eval >=  100) eval =  99;
            if (eval <= -100) eval = -99;
            if (myOrder == Board.SECOND_MOVE)
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
            List<Move> nextMoves = board.enumNextMoves();

            // 自分の手番なら最も自分に有利な手を選択（自分にとっての最善手）
            // 相手の手番なら最も自分に不利な手を選択（相手にとっての最善手）
            int best = (board.turnHolder == myOrder) ? int.MinValue : int.MaxValue;
            for (int i = 0; i < nextMoves.Count; i++)
            {
                Board nextBoard = board.copy();
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
