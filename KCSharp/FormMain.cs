using System.Drawing.Drawing2D;

namespace KCSharp
{
    public partial class FormMain : Form
    {
        // 升目の幅
        const int BOX_WIDTH = 100;
        // 石の直径
        const int STONE_SIZE = 80;
        // 着手可能位置の印の直径
        const int AVEILABLE_MARK_SIZE = 40;
        // 矢印のペンの太さとサイズ
        Pen arrowPen = new Pen(Color.Black, 4);
        AdjustableArrowCap arrowCap = new AdjustableArrowCap(6, 6);

        // 盤面データ
        Board board = new Board();
        // 対局は開始したか？
        bool isStarted = false;

        // 選択中の石の位置
        Position selectedStone = Position.NONE;
        // 最後の着手
        Move[] lastMove = new Move[2]; // 0:先手,1:後手
        // 石を選択中か？
        //bool isStoneSelected = false;

        // プレイヤーの先手/後手
        int you = Board.FIRST_MOVE;
        // CPUの先手/後手
        int cpu = Board.SECOND_MOVE;
        // 勝者
        int winner = Board.NO_STONE;

        // CPUの思考エンジン
        Engine cpuEngine;

        // 初期化
        public FormMain()
        {
            InitializeComponent();

            // ゲーム設定の初期値
            comboMove.SelectedIndex = 0;
            textDepth.Text = "6";

            // 盤面の初期化
            board.reset(false); // 初期配置はしない
            lastMove[Board.FIRST_MOVE] = KCSharp.Move.NONE;
            lastMove[Board.SECOND_MOVE] = KCSharp.Move.NONE;
            selectedStone = Position.NONE;
            drawBoard();
            textTurn.Text = "対局前";
            textTurn.ForeColor = Color.Green;
        }

        // 盤面の描画
        private void drawBoard()
        {
            Bitmap canvas = new Bitmap(pictureBoard.Width, pictureBoard.Height);
            Graphics g = Graphics.FromImage(canvas);

            // 盤面の背景色 
            Rectangle rect = new Rectangle(0, 0, pictureBoard.Width - 1, pictureBoard.Height - 1);
            g.FillRectangle(Brushes.DarkTurquoise, rect);

            // 升目の黒線
            int px1 = 0;
            int px2 = BOX_WIDTH * Board.SIZE;
            int py1 = 0;
            int py2 = BOX_WIDTH * Board.SIZE;
            for (int x = 0; x <= Board.SIZE; x++)
            {
                int px = x * BOX_WIDTH;
                g.DrawLine(Pens.Black, px, py1, px, py2);
            }
            for (int y = 0; y <= Board.SIZE; y++)
            {
                int py = y * BOX_WIDTH;
                g.DrawLine(Pens.Black, px1, py, px2, py);
            }

            // 石
            for (int x = 0; x < Board.SIZE; x++)
            {
                for (int y = 0; y < Board.SIZE; y++)
                {
                    // プレイヤーの石が選択されている場合
                    if (selectedStone != Position.NONE)
                    {
                        // 選択中の石はハイライト
                        if (selectedStone.x == x && selectedStone.y == y)
                        {
                            int px = x * BOX_WIDTH + 1;
                            int py = y * BOX_WIDTH + 1;
                            int pw = BOX_WIDTH - 2;
                            g.FillRectangle(Brushes.Gold, px, py, pw, pw);
                        }
                        // 着手可能位置の升目には印を付ける
                        Position pos = new Position(x, y);
                        if (board.isAvailableMove(selectedStone, pos))
                        {
                            int px = x * BOX_WIDTH + (BOX_WIDTH - AVEILABLE_MARK_SIZE) / 2;
                            int py = y * BOX_WIDTH + (BOX_WIDTH - AVEILABLE_MARK_SIZE) / 2;
                            g.FillEllipse(Brushes.Gold, px, py, AVEILABLE_MARK_SIZE, AVEILABLE_MARK_SIZE);
                        }
                    }
                    // 勝負がついている場合、勝者の石をハイライト
                    if (winner != Board.NO_STONE)
                    {
                        if (board.stone[x, y] == winner)
                        {
                            int px = x * BOX_WIDTH + 1;
                            int py = y * BOX_WIDTH + 1;
                            int pw = BOX_WIDTH - 2;
                            g.FillRectangle(Brushes.DeepPink, px, py, pw, pw);
                        }
                    }
                    // 直前の着手を矢印で示す
                    for (int i = 0; i < 2; i++)
                    {
                        if (lastMove[i] == KCSharp.Move.NONE)
                        {
                            continue;
                        }
                        if(lastMove[i].from.x == x && lastMove[i].from.y == y)
                        {
                            int x2 = lastMove[i].to.x;
                            int y2 = lastMove[i].to.y;

                            px1 = x * BOX_WIDTH + BOX_WIDTH / 2;
                            py1 = y * BOX_WIDTH + BOX_WIDTH / 2;
                            px2 = x2 * BOX_WIDTH + BOX_WIDTH / 2;
                            py2 = y2 * BOX_WIDTH + BOX_WIDTH / 2;
                            px2 = (px1 + px2) / 2;
                            py2 = (py1 + py2) / 2;

                            arrowPen.CustomEndCap = arrowCap;
                            g.DrawLine(arrowPen, px1, py1, px2, py2);
                        }
                    }

                    // 先手の石（黒）
                    if (board.stone[x, y] == Board.FIRST_MOVE)
                    {
                        int px = x * BOX_WIDTH + (BOX_WIDTH - STONE_SIZE) / 2;
                        int py = y * BOX_WIDTH + (BOX_WIDTH - STONE_SIZE) / 2;
                        g.FillEllipse(Brushes.Black, px, py, STONE_SIZE, STONE_SIZE);
                    }
                    // 後手の石（白）
                    else if (board.stone[x, y] == Board.SECOND_MOVE)
                    {
                        int px = x * BOX_WIDTH + (BOX_WIDTH - STONE_SIZE) / 2;
                        int py = y * BOX_WIDTH + (BOX_WIDTH - STONE_SIZE) / 2;
                        g.FillEllipse(Brushes.White, px, py, STONE_SIZE, STONE_SIZE);
                    }
                }
            }

            g.Dispose();
            pictureBoard.Image = canvas;

            // どちらが手番持ちかを表示
            textTurn.Text = (board.turnHolder == you) ? "あなた" : "CPU";
            textTurn.ForeColor = (board.turnHolder == you) ? Color.Red : Color.Blue;
        }

        // ボタン等の表示変更
        private void updateControls(string status)
        {
            // 対局中か？
            if (isStarted)
            {
                buttonStart.Text = "投了";    
                comboMove.Enabled = false;
                textDepth.Enabled = false;
            }
            else
            {
                buttonStart.Text = "対局開始";
                comboMove.Enabled = true;
                textDepth.Enabled = true;
                textTurn.Text = status;
                textTurn.ForeColor = Color.Green;
            }
        }

        // 対局開始/投了ボタン
        private void buttonStart_Click(object sender, EventArgs e)
        {
            // 投了する
            if (isStarted)
            {
                isStarted = false;
                updateControls("投了");
            }
            // 対局開始する
            else
            {
                // レベル(先読み深さ)の設定をチェック
                int depth;
                try
                {
                    depth = int.Parse(textDepth.Text);
                    if (depth < 1) throw new Exception();
                }
                catch
                {
                    MessageBox.Show("読みの深さは1以上の整数を設定してください。");
                    return;
                }
                // 先手/後手のチェック
                you = (comboMove.SelectedIndex == 0) ? Board.FIRST_MOVE : Board.SECOND_MOVE;
                cpu = (comboMove.SelectedIndex == 0) ? Board.SECOND_MOVE : Board.FIRST_MOVE;
                winner = Board.NO_STONE;

                // CPUの思考エンジンを生成
                cpuEngine = new Engine1(depth, cpu);

                // 盤面のリセット
                board.reset(true); // 初期配置をする
                lastMove[Board.FIRST_MOVE] = KCSharp.Move.NONE;
                lastMove[Board.SECOND_MOVE] = KCSharp.Move.NONE;
                selectedStone = Position.NONE;
                drawBoard();

                isStarted = true;
                updateControls("");

                // CPUが先手の場合
                if (cpu == Board.FIRST_MOVE)
                {
                    // CPUの手番タスク
                    Task.Run(() =>
                    {
                      taskCpuTurn();
                    });
                }
            }
        }

        // 盤面のクリック（着手の操作）
        private void pictureBoard_MouseClick(object sender, MouseEventArgs e)
        {
            // 対局中かチェック
            if (!isStarted)
            {
                return;
            }
            // プレイヤーの手番かチェック
            if (board.turnHolder == cpu)
            {
                MessageBox.Show("あなたの手番ではありません!");
                return;
            }

            // 着手位置の計算
            int px = e.X;
            int py = e.Y;
            int x = px / BOX_WIDTH;
            int y = py / BOX_WIDTH;
            Position pos = new Position(x, y);

            // 石を選択前
            if (selectedStone == Position.NONE)
            {
                // そこにプレイヤーの石があるか判定
                if (board.isMyStone(pos))
                {
                    selectedStone = pos;
                    drawBoard();
                }
            }
            // 石を選択後
            else
            {
                // 有効な着手かチェック
                if (board.isAvailableMove(selectedStone, pos))
                {
                    // 着手する
                    Move move = new Move(selectedStone, pos);
                    board.doMove(move);
                    lastMove[you] = move;
                    selectedStone = Position.NONE;

                    // 盤面の描画
                    drawBoard();

                    // 勝利チェック
                    if (board.isSquare(you))
                    {
                        winner = you;
                        drawBoard(); // 盤面の再描画
                        MessageBox.Show("あなたの勝ちです！");
                        isStarted = false;
                        updateControls("終了");
                        return;
                    }

                    // CPUの手番タスク
                    Task.Run(() =>
                    {
                        taskCpuTurn();
                    });
                }
            }
        }

        // CPUの手番タスク関数
        void taskCpuTurn()
        {
            // 次の着手を計算
            Move move = cpuEngine.getNextMove(board);
            // 着手
            board.doMove(move);
            lastMove[cpu] = move;

            this.Invoke((Action)(() =>
            {
                // 盤面の描画
                drawBoard();

                // 勝利チェック
                if (board.isSquare(cpu))
                {
                    winner = cpu;
                    drawBoard(); // 盤面の再描画
                    MessageBox.Show("あなたの負けです！");
                    isStarted = false;
                    updateControls("終了");
                    return;
                }
            }));
        }
    }
}
