using System.Diagnostics;

namespace KCSharp
{
    // 思考エンジン 1号（探索：ミニマックス法、評価：速く詰むか+詰まないならランダム）
    class Engine2 : Engine
    {
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

        // 評価関数
        public int evalFunction(Board board)
        {
            int min, max;
            bool dummy;
            
            // 先手の形の評価
            dummy = board.isSquare(Board.FIRST_MOVE, out min, out max);
            int eval_black = 100 * min / max + rand.Next(10);
            // 後手の形の評価
            dummy = board.isSquare(Board.SECOND_MOVE, out min, out max);
            int eval_white = 100 * min / max + rand.Next(10);

            // 評価値
            int eval = eval_black - eval_white;
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
                if(depth == 0)
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
            if (depth == 0)
            {
                Debug.WriteLine($"({bestMove.from.x}, {bestMove.from.y})->({bestMove.to.x}, {bestMove.to.y}) : {best}");
            }
            return best;
        }
    }
}
