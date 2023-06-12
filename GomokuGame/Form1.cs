using GomokuGame;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using static GomokuGame.SocketData;

namespace GomokuGame
{
    public partial class Form1 : Form
    {
        #region Properties
        ChessBoardManager ChessBoard;
        SocketManager socket;
        string PlayerName;
        #endregion
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;

            ChessBoard = new ChessBoardManager(pnlChessBoard, txbPlayerName , pctbMark);
            ChessBoard.EndedGame += ChessBoard_EndedGame;
            ChessBoard.PlayerMarked += ChessBoard_PlayerMarked;

            prcbCoolDown.Step = Cons.COOL_DOWN_STEP;
            prcbCoolDown.Maximum = Cons.COOL_DOWN_TIME;
            prcbCoolDown.Value = 0;

            tmCoolDown.Interval = Cons.COOL_DOWN_INTERVAL;
            socket = new SocketManager();
            //ChessBoard.DrawChessBoard();
            NewGame();
        }

        void EndGame()
        {
            undoToolStripMenuItem.Enabled = false;
            tmCoolDown.Stop();
            pnlChessBoard.Enabled = false;
            //MessageBox.Show("Kết thúc","Thông Báo");
        }

        void NewGame()
        {
            prcbCoolDown.Value = 0;
            tmCoolDown.Stop();
            undoToolStripMenuItem.Enabled = true;

            ChessBoard.DrawChessBoard();
        }

        void Quit()
        {
            Application.Exit();
        }

        void Undo()
        {
            ChessBoard.Undo();
        }
        void SaveGame()
        {
            ChessBoard.SaveFile();
        }
        void LoadGame()
        {
            ChessBoard.LoadFile( );
        }
        

        void ChessBoard_PlayerMarked(object sender, BtnClickEvent e)
        {
            tmCoolDown.Start();
            prcbCoolDown.Value = 0;
            if (ChessBoard.PlayMode == 1)
            {
                try
                {
                    pnlChessBoard.Enabled = false;
                    socket.Send(new SocketData((int)SocketCommand.SEND_POINT, "", e.ClickedPoint));

                    undoToolStripMenuItem.Enabled = false;
                    

                    Listen();
                }
                catch
                {
                    EndGame();
                    MessageBox.Show("Không có kết nối nào tới máy đối thủ", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        void ChessBoard_EndedGame(object sender, EventArgs e)
        {
            EndGame();
            if (ChessBoard.PlayMode == 1)
                socket.Send(new SocketData((int)SocketCommand.END_GAME, "", new Point()));
        }

        private void tmCoolDown_Tick(object sender, EventArgs e)
        {
            prcbCoolDown.PerformStep();

            if (prcbCoolDown.Value >= prcbCoolDown.Maximum)
            {
                EndGame();
                if (ChessBoard.PlayMode == 1)
                    socket.Send(new SocketData((int)SocketCommand.TIME_OUT, "", new Point()));
            }
        }

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewGame();
            if (ChessBoard.PlayMode == 1)
            {
                try
                {
                    socket.Send(new SocketData((int)SocketCommand.NEW_GAME, "", new Point()));
                }
                catch { }
            }

            pnlChessBoard.Enabled = true;
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            prcbCoolDown.Value = 0;
            
            Undo();
            if (ChessBoard.PlayMode == 1)
                socket.Send(new SocketData((int)SocketCommand.UNDO, "", new Point()));
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Quit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn thoát", "Thông báo", MessageBoxButtons.OKCancel) != System.Windows.Forms.DialogResult.OK)
                e.Cancel = true;
            else
            {
                try
                {
                    socket.Send(new SocketData((int)SocketCommand.QUIT, "", new Point()));
                }
                catch { }
            }
        }

        private void ruleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Chào bạn! \n" +
                "     -----------------------------------------------  \n" +
                "Có lẽ bạn đã quá quen thuộc với trò chơi CARO rồi.\n" + 
                "Nhưng bạn nên chú ý một số vấn đề sau:\n\n" + 
                "~**************************************~ \n" + 
                "+ Bất kỳ người chơi nào được 5 quân liên tiếp là thắng cho dù là bị chặn 2 đầu. \n\n" + 
                "+ Lúc mới bắt đầu chơi nếu bạn không thiết lập thuộc tính cho mình thì máy sẽ mặc định sẵn cho bạn.\n\n" + 
                "+ Bạn chọn Save Game nếu muốn tạm thời dừng cuộc chơi.\n\n" + 
                "+ Bạn chon Load Game nếu bạn đã Save Game ít nhất một lần và bạn sẽ chơi lại ván mà bạn đã Save lần cuối cùng. \n\n" + 
                "                                  **** Chúc bạn vui vẻ**** \n\n" + 
                "SV:20110710 - Nguyễn Thanh Sang\n" +
                "     20110723 - Võ Văn Thạnh","Luật chơi");
        }

        private void loadGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewGame();
            LoadGame();
        }

        private void saveGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tmCoolDown.Stop();
            pnlChessBoard.Enabled = false;
            SaveGame();

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void playVsComputerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ChessBoard.PlayMode == 1)
            {
                if (ChessBoard.PlayMode == 1)
                {
                    try
                    {
                        socket.Send(new SocketData((int)SocketCommand.QUIT, "", new Point()));
                    }
                    catch { }

                    socket.CloseConnect();
                    MessageBox.Show("Đã ngắt kết nối mạng LAN", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            ChessBoard.PlayMode = 3;
            NewGame();
            ChessBoard.StartAI();
        }

        private void Btn_AI_Click_Click(object sender, EventArgs e)
        {
            playVsComputerToolStripMenuItem_Click(sender, e);
        }
        
        private void btn_LAN_Click(object sender, EventArgs e)
        {
            playViaLanToolStripMenuItem_Click(sender, e);
        }
        #region LAN settings
        private void GameCaro_Shown(object sender, EventArgs e)
        {
            txt_IP.Text = socket.GetLocalIPv4(NetworkInterfaceType.Wireless80211);

            if (string.IsNullOrEmpty(txt_IP.Text))
                txt_IP.Text = socket.GetLocalIPv4(NetworkInterfaceType.Ethernet);

        }

        private void Listen()
        {
            Thread ListenThread = new Thread(() =>
            {
                try
                {
                    SocketData data = (SocketData)socket.Receive();
                    ProcessData(data);
                }
                catch { }
            });

            ListenThread.IsBackground = true;
            ListenThread.Start();
        }

        private void ProcessData(SocketData data)
        {
            PlayerName = ChessBoard.Player[ChessBoard.CurrentPlayer == 1 ? 0 : 1].Name;

            switch (data.Command)
            {
                case (int)SocketCommand.SEND_POINT:
                    // Có thay đổi giao diện muốn chạy ngọt phải để trong đây
                    this.Invoke((MethodInvoker)(() =>
                    {
                        ChessBoard.OtherPlayerClicked(data.Point);
                        pnlChessBoard.Enabled = true;

                        prcbCoolDown.Value = 0;
                        tmCoolDown.Start();

                        undoToolStripMenuItem.Enabled = true;
                    }));
                    break;

                case (int)SocketCommand.NEW_GAME:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        NewGame();
                        pnlChessBoard.Enabled = false;
                    }));
                    break;

                case (int)SocketCommand.UNDO:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        prcbCoolDown.Value = 0;
                        ChessBoard.Undo();
                    }));
                    break;

                

                case (int)SocketCommand.END_GAME:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        EndGame();
                        MessageBox.Show(PlayerName + " đã chiến thắng ♥ !!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }));
                    break;

                case (int)SocketCommand.TIME_OUT:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        EndGame();
                        MessageBox.Show("Hết giờ rồi !!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }));
                    break;

                case (int)SocketCommand.QUIT:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        tmCoolDown.Stop();
                        EndGame();

                        ChessBoard.PlayMode = 2;
                        socket.CloseConnect();

                        MessageBox.Show("Đối thủ đã chạy mất dép", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }));
                    break;

                default:
                    break;
            }

            Listen();
        }
        #endregion

        private void playViaLanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChessBoard.PlayMode = 1;
            NewGame();

            socket.IP = txt_IP.Text;

            if (!socket.ConnectServer())
            {
                socket.IsServer = true;
                pnlChessBoard.Enabled = true;
                socket.CreateServer();
                MessageBox.Show("Bạn đang là Server", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                socket.IsServer = false;
                pnlChessBoard.Enabled = false;
                Listen();
                MessageBox.Show("Kết nối thành công !!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 Music = new Form2();
            Music.Show();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if(radioButton1.Checked==true)
            {
                ChessBoard.Easy();
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {
                ChessBoard.Normal();
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked == true)
            {
                ChessBoard.Hard();
            }
        }

        private void easyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
            radioButton1_CheckedChanged(sender, e);
            playVsComputerToolStripMenuItem_Click(sender, e);
            
        }

        private void normalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            radioButton2.Checked = true;
            radioButton2_CheckedChanged(sender, e);
            playVsComputerToolStripMenuItem_Click(sender, e);
        }

        private void hardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            radioButton3.Checked = true;
            radioButton3_CheckedChanged(sender, e);
            playVsComputerToolStripMenuItem_Click(sender, e);
        }
    }


}
