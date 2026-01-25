namespace KCSharp
{
    // 思考エンジン 1号（探索：ミニマックス法、評価：速く詰むか+詰まないならランダム）
    class Engine1 : Engine
    {
        // コンストラクタ (読みの深さと先手/後手を指定)
        public Engine1(int depth, int order) : base(depth, order) { }

        // 次の手を取得する
        public override Move getNextMove(Board board)
        {
            // 次の手を考える
            readMinMax(board, 0);

            return bestMove;
        }

        // ミニマックス法による先読み (再帰)
        public int readMinMax(Board board, int depth)
        {
            // 先読み深さの末端に到達したら評価値を返す
            if (depth == maxDepth)
            {
                // 評価関数
                int eval = board.evalFunction(myOrder);
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
            //Debug.WriteLine(depth + ":" + best.eval + " ");
            return best;
        }
    }
}
