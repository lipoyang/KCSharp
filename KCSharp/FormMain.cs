using System.Drawing.Drawing2D;

namespace KCSharp
{
    public partial class FormMain : Form
    {
        /********** デバッグ用 **********/
        const bool isDebug = false; // trueならCPU対CPU
        Engine cpu2Engine;         // CPU2の思考エンジン

        /********** 定数 **********/
        // 升目の幅
        const int BOX_WIDTH = 100;
        // 石の直径
        const int STONE_SIZE = 80;
        // マークの直径
        const int MARK_SIZE = 40;
        // 矢印のペンの太さとサイズ
        Pen[] arrowPen = { new Pen(Color.Black, 4), new Pen(Color.White, 4) };
        AdjustableArrowCap arrowCap = new AdjustableArrowCap(6, 6);

        /********** 変数 **********/
        // CPUの思考エンジン
        Engine cpuEngine;

        // 盤面データ
        Board board = new Board();
        // 盤面履歴
        List<Board> record = new List<Board>();

        // 対局の状態
        enum GameStatus
        {
            READY,       // 対局前
            PLAYING,     // 対局中
            FINISHED,    // 終了
            RESIGNED     // 投了
        }
        GameStatus gameStatus = GameStatus.READY;
        // 何手目か
        int turnCnt = 0;
        // 選択中の石の位置
        Position selectedStone = Position.NONE;
        // 大パンチ位置データ
        DaiPunch daiPunch = new DaiPunch();

        // プレイヤーの先手/後手
        int you = Board.BLACK;
        // CPUの先手/後手
        int cpu = Board.WHITE;
        // 勝者
        int winner = Board.NONE;

        // 初期局面番号
        int initPosNo = 1;
        // 初期局面の数
        const int initPosMax = 96;
        // 初期局面データ
        InitialPosition initialPosition =
            new InitialPosition("InitialPosition.csv", initPosMax);

        // ランダム局面データ
        Random rand = new Random();
        Board randomPosition = new Board();

        // 盤面の描画用
        Bitmap canvas;

        /********** メソッド **********/
        #region コンストラクタ
        public FormMain()
        {
            InitializeComponent();
            buttonChange.Location = new Point(710, 218); // ボタン位置調整

            // 盤面の描画用
            canvas = new Bitmap(pictureBoard.Width, pictureBoard.Height);

            // 初期局面データの読み込み
            if (initialPosition.load() == false)
            {
                MessageBox.Show("初期局面データの読み込みに失敗しました。");
            }
            // ランダム局面の生成
            generateRandomPosition();

            // ゲーム設定の初期値
            comboMove.SelectedIndex = 0;
            comboGameType.SelectedIndex = 0;
            initPosNo = 1;
            textGameNumber.Text = "1";
            comboLevel.SelectedIndex = 6; // レベル7

            // 盤面の初期化
            setInitialPosition();
        }
        #endregion

        #region 一般メソッド
        // ランダム盤面の生成
        private void generateRandomPosition()
        {
            randomPosition.reset();
            for (int i = 0; i < 8; i++)
            {
                int x, y;
                do
                {
                    x = rand.Next(Board.SIZE);
                    y = rand.Next(Board.SIZE);
                } while (randomPosition.getStone(x, y) != Board.NONE);
                randomPosition.setStone(
                    x, y, (i % 2 == 0) ? Board.BLACK : Board.WHITE);
            }
        }

        // 盤面の描画
        private void drawBoard()
        {
            Graphics g = Graphics.FromImage(canvas);

            // 盤面の背景色 
            g.FillRectangle(Brushes.DarkTurquoise,
                0, 0, pictureBoard.Width - 1, pictureBoard.Height - 1);

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
                    // マスの左上座標と幅
                    int rx = x * BOX_WIDTH + 1;
                    int ry = y * BOX_WIDTH + 1;
                    int rw = BOX_WIDTH - 2;
                    // マークの左上座標
                    int mx = x * BOX_WIDTH + (BOX_WIDTH - MARK_SIZE) / 2;
                    int my = y * BOX_WIDTH + (BOX_WIDTH - MARK_SIZE) / 2;
                    // 石の左上座標
                    int sx = x * BOX_WIDTH + (BOX_WIDTH - STONE_SIZE) / 2;
                    int sy = y * BOX_WIDTH + (BOX_WIDTH - STONE_SIZE) / 2;

                    // プレイヤーの石が選択されている場合
                    if (selectedStone != Position.NONE)
                    {
                        // 選択中の石はハイライト
                        if (selectedStone.x == x && selectedStone.y == y)
                        {
                            g.FillRectangle(Brushes.Gold, rx, ry, rw, rw);
                        }
                        // 着手可能位置の升目には印を付ける
                        Position pos = new Position(x, y);
                        if (board.isAvailableMove(selectedStone, pos))
                        {
                            g.FillEllipse(Brushes.Gold, mx, my, MARK_SIZE, MARK_SIZE);
                        }
                    }
                    // 勝負がついている場合、勝者の石をハイライト
                    if (winner != Board.NONE && turnCnt == record.Count - 1)
                    {
                        if (board.getStone(x, y) == winner)
                        {
                            g.FillRectangle(Brushes.DeepPink, rx, ry, rw, rw);
                        }
                    }
                    // 大パンチ位置に印を付ける
                    else
                    {
                        int punch = daiPunch.getStone(x, y);
                        if (punch != Board.NONE)
                        {
                            Brush brush = (punch == Board.BLACK) ? Brushes.Black : Brushes.White;
                            g.FillEllipse(brush, mx, my, MARK_SIZE, MARK_SIZE);
                        }
                    }
                    // 直前の着手を矢印で示す
                    for (int i = Board.BLACK; i <= Board.WHITE; i++)
                    {
                        Move lastMove = board.getLastMove(i);
                        if (lastMove == KCSharp.Move.NONE)
                        {
                            continue;
                        }
                        if (lastMove.from.x == x &&
                            lastMove.from.y == y)
                        {
                            int x2 = lastMove.to.x;
                            int y2 = lastMove.to.y;

                            int ax1 = x * BOX_WIDTH + BOX_WIDTH / 2;
                            int ay1 = y * BOX_WIDTH + BOX_WIDTH / 2;
                            int ax2 = x2 * BOX_WIDTH + BOX_WIDTH / 2;
                            int ay2 = y2 * BOX_WIDTH + BOX_WIDTH / 2;
                            ax2 = (ax1 + ax2) / 2;
                            ay2 = (ay1 + ay2) / 2;

                            arrowPen[i].CustomEndCap = arrowCap;
                            g.DrawLine(arrowPen[i], ax1, ay1, ax2, ay2);
                        }
                    }

                    // 先手の石（黒）
                    if (board.getStone(x, y) == Board.BLACK)
                    {
                        g.FillEllipse(Brushes.Black, sx, sy, STONE_SIZE, STONE_SIZE);
                    }
                    // 後手の石（白）
                    else if (board.getStone(x, y) == Board.WHITE)
                    {
                        g.FillEllipse(Brushes.White, sx, sy, STONE_SIZE, STONE_SIZE);
                    }
                }
            }

            g.Dispose();
            pictureBoard.Image = canvas;

            // どちらが手番持ちかを表示
            switch (gameStatus)
            {
                case GameStatus.READY:
                    textTurn.Text = "対局前";
                    textTurn.ForeColor = Color.Green;
                    break;
                case GameStatus.PLAYING:
                    textTurn.Text = (board.turn == you) ? "あなた" : "CPU";
                    textTurn.ForeColor = (board.turn == you) ? Color.Red : Color.Blue;
                    break;
                case GameStatus.FINISHED:
                    textTurn.Text = "終了";
                    textTurn.ForeColor = Color.Green;
                    break;
                case GameStatus.RESIGNED:
                    textTurn.Text = "投了";
                    textTurn.ForeColor = Color.Green;
                    break;
            }

            // 何手目かを表示
            textTurnNum.Text = turnCnt.ToString();
            buttonUndo.Enabled = (turnCnt > 0);
            buttonRedo.Enabled = (turnCnt < record.Count - 1);
        }

        // ボタン等の表示変更
        private void updateControls()
        {
            // 対局中か？
            if (gameStatus == GameStatus.PLAYING)
            {
                buttonStart.Text = "投了する";
                comboMove.Enabled = false;
                comboLevel.Enabled = false;
                comboGameType.Enabled = false;
                textGameNumber.Enabled = false;
                buttonPP.Enabled = false;
                buttonP.Enabled = false;
                buttonN.Enabled = false;
                buttonNN.Enabled = false;
            }
            else
            {
                buttonStart.Text = "対局開始";
                comboMove.Enabled = true;
                comboLevel.Enabled = true;
                comboGameType.Enabled = true;
                textGameNumber.Enabled = true;
                buttonPP.Enabled = true;
                buttonP.Enabled = true;
                buttonN.Enabled = true;
                buttonNN.Enabled = true;
            }
        }

        // 初期配置
        private void setInitialPosition()
        {
            // 盤面のリセット
            switch (comboGameType.SelectedIndex)
            {
                // 96番勝負
                case 0:
                    Kifu black = initialPosition.black[initPosNo - 1];
                    Kifu white = initialPosition.white[initPosNo - 1];
                    board.reset(black.stones, white.stones);
                    break;
                // ランダム
                case 1:
                    board = randomPosition;
                    break;
            }

            gameStatus = GameStatus.READY;
            winner = Board.NONE;
            selectedStone = Position.NONE;
            turnCnt = 0;
            record.Clear();
            daiPunch.reset();

            drawBoard();
            updateControls();
        }

        // 着手と棋譜更新
        private void doMoveAndRecord(Move move)
        {
            // 着手
            board.doMove(move);
            turnCnt++;
            // 通常は棋譜に追加
            if (turnCnt == record.Count) {
                record.Add(board);
            }
            // やり直しの場合、以降の棋譜を削除
            else if (turnCnt < record.Count) {
                record[turnCnt] = board;
                int del_index = turnCnt + 1;
                if (del_index < record.Count) {
                    record.RemoveRange(del_index, record.Count - del_index);
                }
            }
            // 異常な場合
            else {
                MessageBox.Show("棋譜に予期しないエラーが発生しました。");
            }
        }
        #endregion

        #region タスク関数
        // CPUの手番タスク関数
        void taskCpuTurn()
        {
            // 次の着手を計算
            Move move = cpuEngine.getNextMove(board);
            // 中断判定(投了)
            if (move == KCSharp.Move.NONE) return;
            // 着手と棋譜更新
            doMoveAndRecord(move);
            // 大パンチ判定
            daiPunch.check(board);

            this.Invoke((Action)(() =>
            {
                // 盤面の描画
                drawBoard();

                // 勝利チェック
                if (board.isSquare(cpu))
                {
                    winner = cpu;
                    gameStatus = GameStatus.FINISHED;
                    drawBoard();
                    updateControls();
                    MessageBox.Show("あなたの負けです！");
                    return;
                }
                else if (isDebug)
                {
                    // あなたの代わりにデバッグ用CPU2の手番タスク
                    Task.Run(() =>
                    {
                        taskCpu2Turn();
                    });
                }
            }));
        }

        // あなたの代わりのCPU2の手番タスク関数 (デバッグ用)
        void taskCpu2Turn()
        {
            // 次の手を考える
            Move move = cpu2Engine.getNextMove(board);
            // 中断判定(投了)
            if (move == KCSharp.Move.NONE) return;
            // 着手と棋譜更新
            doMoveAndRecord(move);
            // 大パンチ判定
            daiPunch.check(board);

            this.Invoke((Action)(() =>
            {
                // 盤面の描画
                drawBoard();
                // 勝利チェック
                if (board.isSquare(you))
                {
                    winner = you;
                    gameStatus = GameStatus.FINISHED;
                    drawBoard();
                    updateControls();
                    MessageBox.Show("あなた(CPU2)の勝ちです！");
                    return;
                }
                else if (isDebug)
                {
                    // CPU1の手番タスク
                    Task.Run(() =>
                    {
                        taskCpuTurn();
                    });
                }
            }));
        }
        #endregion

        #region イベントハンドラ
        // 対局開始/投了ボタン
        private void buttonStart_Click(object sender, EventArgs e)
        {
            // 対局中 → 投了する
            if (gameStatus == GameStatus.PLAYING)
            {
                // CPUの手番だったら中断させる
                if (board.turn == cpu)
                {
                    cpuEngine.cancel();
                }
                gameStatus = GameStatus.RESIGNED;
                drawBoard();
                updateControls();
            }
            // 対局開始する
            else
            {
                // 先手/後手のチェック
                you = (comboMove.SelectedIndex == 0) ? Board.BLACK : Board.WHITE;
                cpu = (comboMove.SelectedIndex == 0) ? Board.WHITE : Board.BLACK;

                setInitialPosition();
                record.Add(board);

                gameStatus = GameStatus.PLAYING;
                drawBoard();
                updateControls();

                // CPUの思考エンジンを生成
                int cpuLevel = comboLevel.SelectedIndex + 1; // レベル1～7
                cpuEngine = new Engine_AB_SKR(cpuLevel, cpu);
                if (isDebug)
                {
                    // デバッグ用CPU2エンジン生成
                    cpuEngine = new Engine_AB_SKR(cpuLevel, cpu);
                    cpu2Engine = new Engine_AB_SKR(cpuLevel, you);
                }

                // CPUが先手の場合
                if (cpu == Board.BLACK)
                {
                    // CPUの手番タスク
                    Task.Run(() =>
                    {
                        taskCpuTurn();
                    });
                }
                else if (isDebug)
                {
                    // あなたの代わりにデバッグ用CPU2の手番タスク
                    Task.Run(() =>
                    {
                        taskCpu2Turn();
                    });
                }
            }
        }

        // 盤面のクリック（着手の操作）
        private void pictureBoard_MouseClick(object sender, MouseEventArgs e)
        {
            // 対局中かチェック
            if (gameStatus != GameStatus.PLAYING)
            {
                return;
            }
            // プレイヤーの手番かチェック
            if (board.turn == cpu)
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

            // そこにプレイヤーの石があるか判定
            if (board.isMyStone(pos))
            {
                selectedStone = pos;
                drawBoard();
            }
            // 石を選択後
            else if (selectedStone != Position.NONE)
            {
                // 有効な着手かチェック
                if (board.isAvailableMove(selectedStone, pos))
                {
                    Move move = new Move(selectedStone, pos);
                    // 着手と棋譜更新
                    doMoveAndRecord(move);
                    // 大パンチ判定
                    daiPunch.check(board);
                    // 選択中の石をクリア
                    selectedStone = Position.NONE;

                    // 盤面の描画
                    drawBoard();

                    // 勝利チェック
                    if (board.isSquare(you))
                    {
                        winner = you;
                        gameStatus = GameStatus.FINISHED;
                        drawBoard();
                        updateControls();
                        MessageBox.Show("あなたの勝ちです！");
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

        // 初期局面タイプの変更
        private void comboGameType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboGameType.SelectedIndex == 0)
            {
                // 96番勝負
                textGameNumber.Visible = true;
                buttonPP.Visible = true;
                buttonP.Visible = true;
                buttonN.Visible = true;
                buttonNN.Visible = true;
                buttonChange.Visible = false;
            }
            else
            {
                // ランダム
                textGameNumber.Visible = false;
                buttonPP.Visible = false;
                buttonP.Visible = false;
                buttonN.Visible = false;
                buttonNN.Visible = false;
                buttonChange.Visible = true;
            }
            setInitialPosition();
        }

        // 戻るボタン
        private void buttonUndo_Click(object sender, EventArgs e)
        {
            if (turnCnt == 0) return;
            turnCnt--;
            board = record[turnCnt];
            daiPunch.check(board);
            drawBoard();
        }

        // 進むボタン
        private void buttonRedo_Click(object sender, EventArgs e)
        {
            if (turnCnt >= record.Count - 1) return;
            turnCnt++;
            board = record[turnCnt];
            daiPunch.check(board);
            drawBoard();
        }

        // 初期局面番号 << ボタン
        private void buttonPP_Click(object sender, EventArgs e)
        {
            int no = initPosNo - 10;
            if (no < 1) return;
            initPosNo = no;
            textGameNumber.Text = no.ToString();
            setInitialPosition();
        }

        // 初期局面番号 < ボタン
        private void buttonP_Click(object sender, EventArgs e)
        {
            int no = initPosNo - 1;
            if (no < 1) return;
            initPosNo = no;
            textGameNumber.Text = no.ToString();
            setInitialPosition();
        }

        // 初期局面番号 > ボタン
        private void buttonN_Click(object sender, EventArgs e)
        {
            int no = initPosNo + 1;
            if (no > initPosMax) return;
            initPosNo = no;
            textGameNumber.Text = no.ToString();
            setInitialPosition();
        }

        // 初期局面番号 >> ボタン
        private void buttonNN_Click(object sender, EventArgs e)
        {
            int no = initPosNo + 10;
            if (no > initPosMax) return;
            initPosNo = no;
            textGameNumber.Text = no.ToString();
            setInitialPosition();
        }

        // ランダム盤面のチェンジボタン
        private void buttonChange_Click(object sender, EventArgs e)
        {
            generateRandomPosition();
            setInitialPosition();
        }
        #endregion
    }
}
