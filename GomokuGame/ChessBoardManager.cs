using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace GomokuGame 
{
    public class ChessBoardManager
    {

        Image image_O = Image.FromFile(Application.StartupPath + "\\Resources\\o.png");
        Image image_X = Image.FromFile(Application.StartupPath + "\\Resources\\x.png");
        #region Properties
        protected string output;
        protected string readFile;
        protected int checkUndo = 0;

        // chưa hoàn thiện chọn chọn quân cờ
        protected int chooseChess=0;
        

        private Panel chessBoard;
        private Stack<PlayInfo> stkUndoStep;
        public Stack<PlayInfo> StkUndoStep
        {
            get { return stkUndoStep; }
            set { stkUndoStep = value; }
        }
        public Panel ChessBoard
        {
            get { return chessBoard; }
            set { chessBoard = value; }
        }

        private List<Player> player;
        private List<Player> playerVsPC;

        public List<Player> Player
        {
            get { return player; }
            set { player = value; }
        }
        public List<Player> PlayerVsPC
        {
            get { return playerVsPC; }
            set { playerVsPC = value; }
        }

        private int currentPlayer;

        public int CurrentPlayer
        {
            get { return currentPlayer; }
            set { currentPlayer = value; }
        }

        private TextBox playerName;

        public TextBox PlayerName
        {
            get { return playerName; }
            set { playerName = value; }
        }

        private PictureBox playerMark;

        public PictureBox PlayerMark
        {
            get { return playerMark; }
            set { playerMark = value; }
        }

        private List<List<Button>> matrix;

        public List<List<Button>> Matrix
        {
            get { return matrix; }
            set { matrix = value; }
        }

        private event EventHandler<BtnClickEvent> playerMarked;
        public event EventHandler<BtnClickEvent> PlayerMarked
        {
            add
            {
                playerMarked += value;
            }
            remove
            {
                playerMarked -= value;
            }
        }

        private event EventHandler endedGame;
        public event EventHandler EndedGame
        {
            add
            {
                endedGame += value;
            }
            remove
            {
                endedGame -= value;
            }
        }

        private Stack<PlayInfo> playTimeLine;

        public Stack<PlayInfo> PlayTimeLine
        {
            get { return playTimeLine; }
            set { playTimeLine = value; }
        }
        // playmode==3 la pVsPC
        private int playMode = 0;
        public int PlayMode
        {
            get { return playMode; }
            set { playMode = value; }
        }
        private bool IsAI = false;
        #endregion

        #region Initialize
        public ChessBoardManager(Panel chessBoard, TextBox playerName, PictureBox mark)
        {
            this.ChessBoard = chessBoard;
            this.PlayerName = playerName;
            this.PlayerMark = mark;
            // play vs PC
                this.playerVsPC = new List<Player>()
                {
                    new Player("Computer", image_X),
                    
                    new Player("You",image_O)
                 };
            // player vs player
                this.Player = new List<Player>()
                {
                    new Player("Player_1", image_X),
                    new Player("Player_2", image_O)
                 };
            

        }
        #endregion

        #region Methods        
        public void DrawChessBoard()
        {
            ChessBoard.Enabled = true;
            ChessBoard.Controls.Clear();

            PlayTimeLine = new Stack<PlayInfo>();
            StkUndoStep = new Stack<PlayInfo>();
            this.CurrentPlayer = 0;

            if(playMode==3)
            {
                PlayerName.Text = PlayerVsPC[1].Name;
                PlayerMark.Image = PlayerVsPC[1].Mark;
            }
            else
                ChangePlayer();

            Matrix = new List<List<Button>>();

            Button oldButton = new Button() { Width = 0, Location = new Point(0, 0) };
            for (int i = 0; i < Cons.CHESS_BOARD_HEIGHT; i++)
            {
                Matrix.Add(new List<Button>());

                for (int j = 0; j < Cons.CHESS_BOARD_WIDTH; j++)
                {
                    Button btn = new Button()
                    {
                        Width = Cons.CHESS_WIDTH,
                        Height = Cons.CHESS_HEIGHT,
                        Location = new Point(oldButton.Location.X + oldButton.Width, oldButton.Location.Y),
                        BackgroundImageLayout = ImageLayout.Stretch,
                        Tag = i.ToString()
                    };

                    btn.Click += btn_Click;

                    ChessBoard.Controls.Add(btn);

                    Matrix[i].Add(btn);

                    oldButton = btn;
                }
                oldButton.Location = new Point(0, oldButton.Location.Y + Cons.CHESS_HEIGHT);
                oldButton.Width = 0;
                oldButton.Height = 0;
                output = string.Empty;
            }
        }
        #region chooseChess
        public void Choose_X(PictureBox mark)
        {
            chooseChess = 1;
            mark.Image = image_X;
            this.playerMark.Image = image_X;
            
        }
        public void Choose_O()
        {
            chooseChess = 0;
        }
        #endregion
       
        public bool Undo()
        {
            if (PlayTimeLine.Count <= 1)
                return false;

            PlayInfo OldPos = playTimeLine.Peek();
            CurrentPlayer = OldPos.CurrentPlayer == 1 ? 0 : 1;

            if (PlayMode == 3)

            {
                bool IsUndo1 = UndoAStep();
                return IsUndo1;
            }

            else
            {
                bool IsUndo1 = UndoAStep();
                bool IsUndo2 = UndoAStep();

                return IsUndo1 && IsUndo2;
            }
        }
        private bool UndoAStep()
        {
            if (PlayTimeLine.Count <= 0)
                return false;

            PlayInfo OldPos = PlayTimeLine.Pop();

            Button btn = Matrix[OldPos.Point.Y][OldPos.Point.X];
            btn.BackgroundImage = null;

            if (PlayTimeLine.Count <= 0)
                CurrentPlayer = 0;
            else
                OldPos = PlayTimeLine.Peek();
            if(PlayMode==3)
            {
                PlayerName.Text = PlayerVsPC[1].Name;

                PlayerMark.Image = PlayerVsPC[1].Mark;
            }    
            else
                ChangePlayer();
            output = output.Remove(output.Length - 3, 3);
            return true;
        }
        public void Save(Button btn)
        {
            Point point = GetChessPoint(btn);

            output +=(point.Y).ToString() +(point.X).ToString();
            if (Matrix[point.Y][point.X].BackgroundImage == image_O)
            {
                output += "O";
            }
            else
                output += "X";
  
        }
        
        public void SaveFile()
        {
            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {

                    using (var writer = new StreamWriter(myStream))
                    {
                        writer.WriteLine(output);
                    }
                    myStream.Close();
                }
                output = string.Empty;
            }
            MessageBox.Show("Đã Save", "Thông báo", MessageBoxButtons.OK);
        }
        public void check_file()
        {
            int dem = 0;
            for (int i = 0; i < readFile.Length; i++)
            {
                if (readFile[i] >= 48 && readFile[i] <= 57)
                    dem++;
                if (readFile[i] == 'X' || readFile[i] == 'O')
                    dem = 0;
                if (dem == 3)
                {
                    readFile = readFile.Remove(i, 1);
                    i = 0;
                }
            }
        }
        public void Load()
        {
            Point point = new Point(0,0);
            int i = 0;
            readFile = readFile.Trim();
            check_file();
            while(i<readFile.Length)
            {
                point.X = int.Parse(readFile[i].ToString());
                i++;
                point.Y = int.Parse(readFile[i].ToString());
                i++;
                //Point point=new Point(readFile[i ].ToString(), readFile[i + 1].ToString())
                if (readFile[i] == 'X')
                {
                    Matrix[point.X][point.Y].BackgroundImage = image_X;
                    i++;
                }
                else
                { 
                    Matrix[point.X][point.Y].BackgroundImage = image_O;
                    i++; 
                }
            }
            // chua hoan thien
            if(readFile[readFile.Length-1]=='X')
            {

                currentPlayer = currentPlayer == 1 ? 0 : 1;
                ChangePlayer();
            }
            
        }
        public void LoadFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                StreamReader r = new StreamReader(dlg.FileName);
                readFile = r.ReadToEnd();
                r.Dispose();
                r.Close();
                Load();
                output = readFile;
                MessageBox.Show("Đã Load game", "Thông Báo", MessageBoxButtons.OK
               , MessageBoxIcon.Information);

            }
            
        }
        public override string ToString()
        {
            return output.ToString();
        }
        
        private bool isEndGame(Button btn)
        {
            return isEndHorizontal(btn) || isEndVertical(btn) || isEndPrimary(btn) || isEndSub(btn);
        }

        private Point GetChessPoint(Button btn)
        {
            int vertical = Convert.ToInt32(btn.Tag);
            int horizontal = Matrix[vertical].IndexOf(btn);

            Point point = new Point(horizontal, vertical);

            return point;
        }
        #region Handling winning and losing
        private bool isEndHorizontal(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countLeft = 0;
            for (int i = point.X; i >= 0; i--)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    countLeft++;
                }
                else
                    break;
            }

            int countRight = 0;
            for (int i = point.X + 1; i < Cons.CHESS_BOARD_WIDTH; i++)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    countRight++;
                }
                else
                    break;
            }

            return countLeft + countRight == 5;
        }
        private bool isEndVertical(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countTop = 0;
            for (int i = point.Y; i >= 0; i--)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else
                    break;
            }

            int countBottom = 0;
            for (int i = point.Y + 1; i < Cons.CHESS_BOARD_HEIGHT; i++)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else
                    break;
            }

            return countTop + countBottom == 5;
        }
        private bool isEndPrimary(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countTop = 0;
            for (int i = 0; i <= point.X; i++)
            {
                if (point.X - i < 0 || point.Y - i < 0)
                    break;

                if (Matrix[point.Y - i][point.X - i].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else
                    break;
            }

            int countBottom = 0;
            for (int i = 1; i <= Cons.CHESS_BOARD_WIDTH - point.X; i++)
            {
                if (point.Y + i >= Cons.CHESS_BOARD_HEIGHT || point.X + i >= Cons.CHESS_BOARD_WIDTH)
                    break;

                if (Matrix[point.Y + i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else
                    break;
            }

            return countTop + countBottom == 5;
        }
        private bool isEndSub(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countTop = 0;
            for (int i = 0; i <= point.X; i++)
            {
                if (point.X + i > Cons.CHESS_BOARD_WIDTH || point.Y - i < 0)
                    break;

                if (Matrix[point.Y - i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else
                    break;
            }

            int countBottom = 0;
            for (int i = 1; i <= Cons.CHESS_BOARD_WIDTH - point.X; i++)
            {
                if (point.Y + i >= Cons.CHESS_BOARD_HEIGHT || point.X - i < 0)
                    break;

                if (Matrix[point.Y + i][point.X - i].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else
                    break;
            }

            return countTop + countBottom == 5;
        }

        private void Mark(Button btn)
        {
            btn.BackgroundImage = Player[CurrentPlayer].Mark;
        }


        #endregion
        #region 1 player
        private int level = 0;
        public void Easy()
        {
            level = 1;
        }
        public void Normal()
        {
            level = 2;
        }
        public void Hard()
        {
            level = 3;
        }

        private long[] ArrAttackScore= new long[7] { 0, 64, 4096, 262144, 16777216, 1073741824, 68719476736 };
        private long[] ArrDefenseScore = new long[7] { 0, 8, 512, 32768, 2097152, 134217728, 8589934592 };

        #region Calculate attack score
        private long AttackHorizontal(int CurrRow, int CurrCol)
        {
            long TotalScore = 0;
            int ComCells = 0;
            int ManCells = 0;

            // Duyệt từ trên xuống
            for (int Count = 1; Count < 6 && CurrRow + Count < Cons.CHESS_BOARD_HEIGHT; Count++)
            {
                if (Matrix[CurrRow + Count][CurrCol].BackgroundImage == player[0].Mark)
                    ComCells += 1;
                else if (Matrix[CurrRow + Count][CurrCol].BackgroundImage == player[1].Mark)
                {
                    ManCells += 1;
                    break;
                }
                else
                    break;
            }

            // Duyệt từ dưới lên
            for (int Count = 1; Count < 6 && CurrRow - Count >= 0; Count++)
            {
                if (Matrix[CurrRow - Count][CurrCol].BackgroundImage == player[0].Mark)
                    ComCells += 1;
                else if (Matrix[CurrRow - Count][CurrCol].BackgroundImage == player[1].Mark)
                {
                    ManCells += 1;
                    break;
                }
                else
                    break;
            }

            if (ManCells == 2)
                return 0;

            /* Nếu ManCells == 1 => bị chặn 1 đầu => lấy điểm phòng ngự tại vị trí này nhưng 
            nên cộng thêm 1 để tăng phòng ngự cho máy cảnh giác hơn vì đã bị chặn 1 đầu */

            TotalScore -= ArrDefenseScore[ManCells + 1];
            TotalScore += ArrAttackScore[ComCells];

            return TotalScore;
        }

        private long AttackVertical(int CurrRow, int CurrCol)
        {
            long TotalScore = 0;
            int ComCells = 0;
            int ManCells = 0;

            // Duyệt từ trái sang phải
            for (int Count = 1; Count < 6 && CurrCol + Count < Cons.CHESS_BOARD_WIDTH; Count++)
            {
                if (Matrix[CurrRow][CurrCol + Count].BackgroundImage == player[0].Mark)
                    ComCells += 1;
                else if (Matrix[CurrRow][CurrCol + Count].BackgroundImage == player[1].Mark)
                {
                    ManCells += 1;
                    break;
                }
                else
                    break;
            }

            // Duyệt từ phải sang trái
            for (int Count = 1; Count < 6 && CurrCol - Count >= 0; Count++)
            {
                if (Matrix[CurrRow][CurrCol - Count].BackgroundImage == player[0].Mark)
                    ComCells += 1;
                else if (Matrix[CurrRow][CurrCol - Count].BackgroundImage == player[1].Mark)
                {
                    ManCells += 1;
                    break;
                }
                else
                    break;
            }

            if (ManCells == 2)
                return 0;

            /* Nếu ManCells == 1 => bị chặn 1 đầu => lấy điểm phòng ngự tại vị trí này nhưng 
            nên cộng thêm 1 để tăng phòng ngự cho máy cảnh giác hơn vì đã bị chặn 1 đầu */

            TotalScore -= ArrDefenseScore[ManCells + 1];
            TotalScore += ArrAttackScore[ComCells];

            return TotalScore;
        }

        private long AttackMainDiag(int CurrRow, int CurrCol)
        {
            long TotalScore = 0;
            int ComCells = 0;
            int ManCells = 0;

            // Duyệt trái trên
            for (int Count = 1; Count < 6 && CurrCol + Count < Cons.CHESS_BOARD_WIDTH && CurrRow + Count < Cons.CHESS_BOARD_HEIGHT; Count++)
            {
                if (Matrix[CurrRow + Count][CurrCol + Count].BackgroundImage == player[0].Mark)
                    ComCells += 1;
                else if (Matrix[CurrRow + Count][CurrCol + Count].BackgroundImage == player[1].Mark)
                {
                    ManCells += 1;
                    break;
                }
                else
                    break;
            }

            // Duyệt phải dưới
            for (int Count = 1; Count < 6 && CurrCol - Count >= 0 && CurrRow - Count >= 0; Count++)
            {
                if (Matrix[CurrRow - Count][CurrCol - Count].BackgroundImage == player[0].Mark)
                    ComCells += 1;
                else if (Matrix[CurrRow - Count][CurrCol - Count].BackgroundImage == player[1].Mark)
                {
                    ManCells += 1;
                    break;
                }
                else
                    break;
            }

            if (ManCells == 2)
                return 0;

            /* Nếu ManCells == 1 => bị chặn 1 đầu => lấy điểm phòng ngự tại vị trí này nhưng 
            nên cộng thêm 1 để tăng phòng ngự cho máy cảnh giác hơn vì đã bị chặn 1 đầu */

            TotalScore -= ArrDefenseScore[ManCells + 1];
            TotalScore += ArrAttackScore[ComCells];

            return TotalScore;
        }

        private long AttackExtraDiag(int CurrRow, int CurrCol)
        {
            long TotalScore = 0;
            int ComCells = 0;
            int ManCells = 0;

            // Duyệt phải trên
            for (int Count = 1; Count < 6 && CurrCol + Count < Cons.CHESS_BOARD_WIDTH && CurrRow - Count >= 0; Count++)
            {
                if (Matrix[CurrRow - Count][CurrCol + Count].BackgroundImage == player[0].Mark)
                    ComCells += 1;
                else if (Matrix[CurrRow - Count][CurrCol + Count].BackgroundImage == player[1].Mark)
                {
                    ManCells += 1;
                    break;
                }
                else
                    break;
            }

            // Duyệt trái dưới
            for (int Count = 1; Count < 6 && CurrCol - Count >= 0 && CurrRow + Count < Cons.CHESS_BOARD_HEIGHT; Count++)
            {
                if (Matrix[CurrRow + Count][CurrCol - Count].BackgroundImage == player[0].Mark)
                    ComCells += 1;
                else if (Matrix[CurrRow + Count][CurrCol - Count].BackgroundImage == player[1].Mark)
                {
                    ManCells += 1;
                    break;
                }
                else
                    break;
            }

            if (ManCells == 2)
                return 0;

            /* Nếu ManCells == 1 => bị chặn 1 đầu => lấy điểm phòng ngự tại vị trí này nhưng 
            nên cộng thêm 1 để tăng phòng ngự cho máy cảnh giác hơn vì đã bị chặn 1 đầu */

            TotalScore -= ArrDefenseScore[ManCells + 1];
            TotalScore += ArrAttackScore[ComCells];

            return TotalScore;
        }
        #endregion

        #region Calculate defense score
        private long DefenseHorizontal(int CurrRow, int CurrCol)
        {
            long TotalScore = 0;
            int ComCells = 0;
            int ManCells = 0;

            // Duyệt từ trên xuống
            for (int Count = 1; Count < 6 && CurrRow + Count < Cons.CHESS_BOARD_HEIGHT; Count++)
            {
                if (Matrix[CurrRow + Count][CurrCol].BackgroundImage == player[0].Mark)
                {
                    ComCells += 1;
                    break;
                }
                else if (Matrix[CurrRow + Count][CurrCol].BackgroundImage == player[1].Mark)
                    ManCells += 1;
                else
                    break;
            }

            // Duyệt từ dưới lên
            for (int Count = 1; Count < 6 && CurrRow - Count >= 0; Count++)
            {
                if (Matrix[CurrRow - Count][CurrCol].BackgroundImage == player[0].Mark)
                {
                    ComCells += 1;
                    break;
                }
                else if (Matrix[CurrRow - Count][CurrCol].BackgroundImage == player[1].Mark)
                    ManCells += 1;
                else
                    break;
            }

            if (ComCells == 2)
                return 0;

            TotalScore += ArrDefenseScore[ManCells];

            return TotalScore;
        }

        private long DefenseVertical(int CurrRow, int CurrCol)
        {
            long TotalScore = 0;
            int ComCells = 0;
            int ManCells = 0;

            // Duyệt từ trái sang phải
            for (int Count = 1; Count < 6 && CurrCol + Count < Cons.CHESS_BOARD_WIDTH; Count++)
            {
                if (Matrix[CurrRow][CurrCol + Count].BackgroundImage == player[0].Mark)
                {
                    ComCells += 1;
                    break;
                }
                else if (Matrix[CurrRow][CurrCol + Count].BackgroundImage == player[1].Mark)
                    ManCells += 1;
                else
                    break;
            }

            // Duyệt từ phải sang trái
            for (int Count = 1; Count < 6 && CurrCol - Count >= 0; Count++)
            {
                if (Matrix[CurrRow][CurrCol - Count].BackgroundImage == player[0].Mark)
                {
                    ComCells += 1;
                    break;
                }
                else if (Matrix[CurrRow][CurrCol - Count].BackgroundImage == player[1].Mark)
                    ManCells += 1;
                else
                    break;
            }

            if (ComCells == 2)
                return 0;

            TotalScore += ArrDefenseScore[ManCells];

            return TotalScore;
        }

        private long DefenseMainDiag(int CurrRow, int CurrCol)
        {
            long TotalScore = 0;
            int ComCells = 0;
            int ManCells = 0;

            // Duyệt trái trên
            for (int Count = 1; Count < 6 && CurrCol + Count < Cons.CHESS_BOARD_WIDTH && CurrRow + Count < Cons.CHESS_BOARD_HEIGHT; Count++)
            {
                if (Matrix[CurrRow + Count][CurrCol + Count].BackgroundImage == player[0].Mark)
                {
                    ComCells += 1;
                    break;
                }
                else if (Matrix[CurrRow + Count][CurrCol + Count].BackgroundImage == player[1].Mark)
                    ManCells += 1;
                else
                    break;
            }

            // Duyệt phải dưới
            for (int Count = 1; Count < 6 && CurrCol - Count >= 0 && CurrRow - Count >= 0; Count++)
            {
                if (Matrix[CurrRow - Count][CurrCol - Count].BackgroundImage == player[0].Mark)
                {
                    ComCells += 1;
                    break;
                }
                else if (Matrix[CurrRow - Count][CurrCol - Count].BackgroundImage == player[1].Mark)
                    ManCells += 1;
                else
                    break;
            }

            if (ComCells == 2)
                return 0;

            TotalScore += ArrDefenseScore[ManCells];

            return TotalScore;
        }

        private long DefenseExtraDiag(int CurrRow, int CurrCol)
        {
            long TotalScore = 0;
            int ComCells = 0;
            int ManCells = 0;

            // Duyệt phải trên
            for (int Count = 1; Count < 6 && CurrCol + Count < Cons.CHESS_BOARD_WIDTH && CurrRow - Count >= 0; Count++)
            {
                if (Matrix[CurrRow - Count][CurrCol + Count].BackgroundImage == player[0].Mark)
                {
                    ComCells += 1;
                    break;
                }
                else if (Matrix[CurrRow - Count][CurrCol + Count].BackgroundImage == player[1].Mark)
                    ManCells += 1;
                else
                    break;
            }

            // Duyệt trái dưới
            for (int Count = 1; Count < 6 && CurrCol - Count >= 0 && CurrRow + Count < Cons.CHESS_BOARD_HEIGHT; Count++)
            {
                if (Matrix[CurrRow + Count][CurrCol - Count].BackgroundImage == player[0].Mark)
                {
                    ComCells += 1;
                    break;
                }
                else if (Matrix[CurrRow + Count][CurrCol - Count].BackgroundImage == player[01].Mark)
                    ManCells += 1;
                else
                    break;
            }

            if (ComCells == 2)
                return 0;

            TotalScore += ArrDefenseScore[ManCells];

            return TotalScore;
        }
        #endregion
        private Point FindAiPos()
        {
            Point AiPos = new Point();
            long MaxScore = 0;
            
            for (int i = 0; i < Cons.CHESS_BOARD_HEIGHT; i++)
            {
                for (int j = 0; j < Cons.CHESS_BOARD_WIDTH; j++)
                {
                    if (Matrix[i][j].BackgroundImage == null)
                    {
                        if (level == 1)
                        {
                            long AttackScore = AttackHorizontal(i, j)  + AttackMainDiag(i, j) ;
                            long DefenseScore = DefenseHorizontal(i, j) + DefenseVertical(i, j)  + DefenseExtraDiag(i, j);

                            long TempScore = AttackScore > DefenseScore ? AttackScore : DefenseScore;

                            if (MaxScore < TempScore)
                            {
                                MaxScore = TempScore;
                                AiPos = new Point(i, j);
                            }
                        }
                        else if (level == 2)
                        {
                            long AttackScore = AttackHorizontal(i, j) + AttackVertical(i, j) + AttackMainDiag(i, j) ;
                            long DefenseScore = DefenseHorizontal(i, j) + DefenseVertical(i, j) + DefenseMainDiag(i, j) + DefenseExtraDiag(i, j);

                            long TempScore = AttackScore > DefenseScore ? AttackScore : DefenseScore;

                            if (MaxScore < TempScore)
                            {
                                MaxScore = TempScore;
                                AiPos = new Point(i, j);
                            }
                        }
                        else
                        {
                            long AttackScore = AttackHorizontal(i, j) + AttackVertical(i, j) + AttackMainDiag(i, j) + AttackExtraDiag(i, j);
                            long DefenseScore = DefenseHorizontal(i, j) + DefenseVertical(i, j) + DefenseMainDiag(i, j) + DefenseExtraDiag(i, j);

                            long TempScore = AttackScore > DefenseScore ? AttackScore : DefenseScore;

                            if (MaxScore < TempScore)
                            {
                                MaxScore = TempScore;
                                AiPos = new Point(i, j);
                            }
                        }
                    }
                }
            }

            return AiPos;
        }

        public void StartAI()
        {
            IsAI = true;
            Button btn = new Button();
            if (PlayTimeLine.Count == 0) // mới bắt đầu thì cho đánh giữa bàn cờ

            {
                btn = Matrix[Cons.CHESS_BOARD_HEIGHT / 4][Cons.CHESS_BOARD_WIDTH / 4];
                Save(btn);
                Matrix[Cons.CHESS_BOARD_HEIGHT / 4][Cons.CHESS_BOARD_WIDTH / 4].BackgroundImage= PlayerVsPC[0].Mark;
                Matrix[Cons.CHESS_BOARD_HEIGHT / 4][Cons.CHESS_BOARD_WIDTH / 4].PerformClick();
            }
            else
            {
                Point AiPos = FindAiPos();
                btn = Matrix[AiPos.X][AiPos.Y];
                Save(btn);
                Matrix[AiPos.X][AiPos.Y].BackgroundImage = PlayerVsPC[0].Mark;
                PlayerName.Text = PlayerVsPC[1].Name;
                PlayerMark.Image = PlayerVsPC[1].Mark;
                Matrix[AiPos.X][AiPos.Y].PerformClick();
                if (isEndGame(Matrix[AiPos.X][AiPos.Y]))
                {
                    EndGame();
                    // in ra người chơi thắn
                    MessageBox.Show(PlayerVsPC[0].Name + " win");
                }
            }
            
        }
        #endregion
        #endregion
        #region 2 players
        public void EndGame()
        {
            if (endedGame != null)
                endedGame(this, new EventArgs());
        }
        private void ChangePlayer()
        {
            PlayerName.Text = Player[CurrentPlayer].Name;

            PlayerMark.Image = Player[CurrentPlayer].Mark;
        }

        void btn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (PlayMode != 3)
            {
                if (btn.BackgroundImage != null)
                    return;

                Mark(btn);

                Save(btn);

                PlayTimeLine.Push(new PlayInfo(GetChessPoint(btn), CurrentPlayer));

                CurrentPlayer = CurrentPlayer == 1 ? 0 : 1;

                ChangePlayer();

                if (playerMarked != null)
                    playerMarked(this, new BtnClickEvent(GetChessPoint(btn)));

                if (isEndGame(btn))
                {
                    EndGame();
                    //MessageBox.Show("Người chơi " + Player[currentPlayer == 1 ? 0 : 1].Name + " win");

                }
                //if (!(IsAI) && playMode == 3)
                //    StartAI();

                //IsAI = false;
            }
            if(PlayMode==3)
            {
                if (btn.BackgroundImage == null)
                {
                    btn.BackgroundImage = PlayerVsPC[1].Mark;
                    Save(btn);
                    playTimeLine.Push(new PlayInfo(GetChessPoint(btn), currentPlayer));
                    if (!isEndGame(btn))
                    {
                        PlayerName.Text = PlayerVsPC[1].Name;
                        PlayerMark.Image = PlayerVsPC[1].Mark;
                        if (!(IsAI) && playMode == 3)
                            StartAI();
                        
                        //IsAI = false;
                        if (playerMarked != null)
                        {
                            playerMarked(this, new BtnClickEvent(GetChessPoint(btn)));
                        }
                    }
                    else
                    {
                        EndGame();
                        MessageBox.Show(PlayerVsPC[1].Name + " win");
                    }
                }
            }
            IsAI = false;
        }

        public void OtherPlayerClicked(Point point)
        {
            Button btn = Matrix[point.Y][point.X];

            if (btn.BackgroundImage != null)
                return; // Nếu ô đã được đánh thì ko cho đánh lại

            Mark(btn);
            Save(btn);

            PlayTimeLine.Push(new PlayInfo(GetChessPoint(btn), CurrentPlayer));

            CurrentPlayer = CurrentPlayer == 1 ? 0 : 1;
            ChangePlayer();

            if (isEndGame(btn))
                EndGame();
        }
        #endregion
        

    }
    //public class ButtonClickEvent : EventArgs
    //{
    //    private Point clickedPoint;

    //    public Point ClickedPoint { get => clickedPoint; set => clickedPoint = value; }

    //    public ButtonClickEvent(Point point)
    //    {
    //        this.ClickedPoint = point;
    //    }
    //}
}
