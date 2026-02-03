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
            this.hisOrder = 1 - order;
        }

        // 次の手を取得する
        public abstract Move getNextMove(Board board);

        // 中断する
        public void cancel()
        {
            canceling = true;
            while(!canceled)
            {
                System.Threading.Thread.Sleep(10);
            }
        }

        protected int maxDepth; // 読みの深さ
        protected int myOrder;  // 自分の手番 (先手/後手)
        protected int hisOrder; // 相手の手番 (先手/後手)

        protected Move bestMove; // 最善手

        // 中断用フラグ
        protected bool canceling = false;
        protected bool canceled = false;
    }
}
