namespace KCSharp
{
    // 思考エンジンクラス(基底クラス)
    abstract class Engine
    {
        // コンストラクタ (読みの深さと先手/後手を指定)
        public Engine(int depth, int order)
        {
            this.maxDepth = depth;
            this.myOrder = order;
        }

        // 次の手を取得する
        public abstract Move getNextMove(Board board);

        protected int maxDepth; // 読みの深さ
        protected int myOrder; // 先手/後手

        protected Move bestMove; // 最善手
    }
}
