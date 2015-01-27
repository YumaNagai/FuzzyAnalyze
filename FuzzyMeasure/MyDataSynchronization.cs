using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;


namespace FuzzyMeasure
{
    #region 同期を記録するための構造体

    /// <summary>
    /// タイムスタンプ
    /// </summary>
    public struct MyTimeStamp
    {
        public long time;              //記録した時刻
        public bool iskeypress;        //true:キーを押していた　false:キーを押していない

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="time">時間[ms]</param>
        /// <param name="ikp">キーを押しているかどうかのフラグ(true:押している false:押していない)</param>
        public MyTimeStamp(long time, bool ikp)
        {
            this.time = time;
            this.iskeypress = ikp;
        }
    }

    public struct MySynchroDataSet
    {
        public int startindex;      //開始インデクス
        public int lastindex;       //終了インデクス
        public string type;         //解析の種類

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="st">開始インデクス</param>
        /// <param name="lt">終了インデクス</param>
        /// <param name="ty">解析の種類</param>
        public MySynchroDataSet(int st, int lt, string ty)
        {
            this.startindex = st;
            this.lastindex = lt;
            this.type = ty;
        }
    }

    #endregion

    #region キーの状態取得をバックグランドで行うフォームクラス

    /// <summary>
    /// キーの状態取得をバックグランドで行うフォームクラス
    /// </summary>
    public class MyDataSynchronization : Form
    {
        #region キーロギングに関するインスタンス郡

        /// <summary>
        /// バックグラウンドでキー入力を取得するメソッド(外部API)
        /// </summary>
        /// <param name="nVirtKey">キーコード</param>
        /// <returns>0以外:KeyPress 0:KeyRelease</returns>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetAsyncKeyState(int nVirtKey);
        
        /// <summary>
        /// キーボードログのためのデリゲード
        /// </summary>
        private delegate void KeyStateLoggingDelegate();
        
        /// <summary>
        /// キー監視スレッド(メインの邪魔にならないようにバックグラウンドで処理)
        /// </summary>
        private Thread routine;

        #endregion

        #region インスタンスフィールド

        private TextBox keylogText;             //ログテキスト
        private bool isRecoding;                //計測中かどうかのフラグ(true:計測中 false:計測中でない) 
        //private bool isMeasureFinish ;          //計測終了かどうかのフラグ(true:計測終了 false:終了していない) 
        private long previous;                  //一つ前に記録した時刻
        private Stopwatch ProgramTimeStamp;     //サンプリング用タイムスタンプ
        private List<MyTimeStamp> stamplog;     //スタンプログ
        private int threadsleep = 0;            //スレッド停止時間[ms]→サンプリングの間隔はこれで調整する

        //private Stopwatch ProgramTimeStamp;     //計測用
        //private bool isdatalog  = false;        //計測可能 or 計測終了可能かどうかのフラグ(true:可能 false:不可能)
        
        #endregion

        #region イニシャライザ系メソッド

        #region コンストラクタ、デストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private MyDataSynchronization()
        {
            Console.WriteLine("new MyKeylogger");
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~MyDataSynchronization() 
        {
            Console.WriteLine("delete MyKeylogger");
        }


        #endregion

        #region 初期化メソッド

        /// <summary>
        /// 初期化メソッド
        /// </summary>
        private void Initialize()
        {
            //フォーム自体の設定
            //フォームの閉じるボタン無効、フォームの幅・高さの可変なし
            this.FormClosing += new FormClosingEventHandler(FormClosingEvent);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Text = "keyEventLog";
            
            //ログテキストの設定
            keylogText = new TextBox();
            keylogText.Multiline = true;
            keylogText.Dock = DockStyle.Fill;
            keylogText.ReadOnly = true;
            this.Controls.Add(keylogText);
            this.Show();

            //タイムスタンプの生成+フラグ設定
            previous = 0;
            stamplog = new List<MyTimeStamp>();   
            isRecoding = false;                 //未計測
            //isMeasureFinish = false;            //終了していない

            ProgramTimeStamp = new Stopwatch();
            ProgramTimeStamp.Start();

            //キーボード監視スレッド生成&開始
            threadsleep = 1;
            this.routine = new Thread(new ThreadStart(MainRoutine));
            routine.IsBackground = true;
            routine.Start();           
        }


        #endregion

        #region 生成メソッド

        /// <summary>
        /// 生成メソッド
        /// </summary>
        /// <returns>初期化生成されたKeyloggerクラス</returns>
        public static MyDataSynchronization Load()
        {
            MyDataSynchronization logger = new MyDataSynchronization();
            logger.Initialize();
            return logger;
        }

        #endregion

        #endregion

        #region キーボードの状態を出力するメソッド

        #region メイン処理

        /// <summary>
        /// スレッドで動かすメインルーチン
        /// </summary>
        private void MainRoutine()
        {
            while (true)
            {
                //キーログ中は他の機能を使うことは出来ないため(人間工学的に)
                //他のスレッドの処理を中断しキーログの処理を優先させる
                System.Threading.Thread.Sleep(threadsleep);
                this.BeginInvoke(new KeyStateLoggingDelegate(Keylogging), null);
            }
        }

        #endregion

        #region キーログメソッド

        /// <summary>
        /// キーの挙動を確認し状態を変更するメソッド
        /// </summary>
        private void Keylogging()
        {
            //(1/512)≒2ミリ秒でサンプリングする
            if (ProgramTimeStamp.ElapsedMilliseconds - previous >= 2)
            {
                #region R(計測開始ボタン)

                //計測準備ができているならば計測
                //NeuroViewではAlt →　R で記録開始
                if (GetAsyncKeyState((int)Keys.R) != 0 && isRecoding == false)
                {
                    threadsleep = 1;
                    isRecoding = true;
                    ProgramTimeStamp.Stop();        //計測終了
                    ProgramTimeStamp.Reset();       //タイムスタンプリセット
                    ProgramTimeStamp.Start();
                    previous = ProgramTimeStamp.ElapsedMilliseconds;   //一つ前の時刻を記録 
                    keylogText.AppendText("R Button Press StartMeasureMent" + System.Environment.NewLine);
                }

                #endregion

                #region S(計測終了ボタン)

                //計測中ならば計測終了
                //NeuroViewではAlt →　S で記録終了
                if (GetAsyncKeyState((int)Keys.S) != 0 && isRecoding == true)
                {
                    threadsleep = 1000;
                    //isMeasureFinish = true;         //計測終了
                    isRecoding = false;             //記録中ではないので状態変更
                    previous = 0;                   //初期値リセット
                    keylogText.AppendText("S Button Press RecordingFinish" + System.Environment.NewLine);
                    
                    //プログラム時間リセット
                    ProgramTimeStamp.Stop();        //計測終了
                    ProgramTimeStamp.Reset();       //タイムスタンプリセット
                    ProgramTimeStamp.Start();       //タイムスタンプリセット
                }

                #endregion

                #region M(同期ボタン)

                //これが押されている間は被験者がYesと考えている
                if (GetAsyncKeyState((int)Keys.M) != 0 && isRecoding == true)
                {
                    MyTimeStamp tt = new MyTimeStamp(ProgramTimeStamp.ElapsedMilliseconds, true);
                    stamplog.Add(tt);
                    previous = ProgramTimeStamp.ElapsedMilliseconds;
                    keylogText.AppendText("Time:" + ProgramTimeStamp.ElapsedMilliseconds + " keypress" + System.Environment.NewLine);
                }

                //押されていない時は、何もしていない
                else if (GetAsyncKeyState((int)Keys.M) == 0 && isRecoding == true)
                {
                    MyTimeStamp tt = new MyTimeStamp(ProgramTimeStamp.ElapsedMilliseconds, false);
                    stamplog.Add(tt);
                    previous = ProgramTimeStamp.ElapsedMilliseconds;
                    keylogText.AppendText("Time:" + ProgramTimeStamp.ElapsedMilliseconds + " keyrelease" + System.Environment.NewLine);
                }


                #endregion

                #region デバッグコマンド

                //計測終了状態にする
                if (GetAsyncKeyState((int)Keys.F12) != 0 )
                {
                    threadsleep = 1000;
                    isRecoding = false;             //記録中ではないので状態変更
                    previous = 0;                   //初期値リセット
                    keylogText.AppendText("F12 Button Press ForceStateChange" + System.Environment.NewLine);

                    //プログラム時間リセット
                    ProgramTimeStamp.Stop();        //計測終了
                    ProgramTimeStamp.Reset();       //タイムスタンプリセット
                }

                #endregion
            }
        }

        #endregion

        #endregion

        #region 状態を最初にリセットするメソッド

        /// <summary>
        /// 状態を最初にリセットするメソッド
        /// </summary>
        public void AllResetData()
        {
            threadsleep = 10;
            previous = 0;
            //isMeasureFinish = false;            //計測終了
            isRecoding = false;                 //記録中ではないので状態変更
            stamplog.Clear();                   //スタンプログのデータ開放
            ProgramTimeStamp.Reset();           //タイムスタンプリセット
            ProgramTimeStamp.Start();           //再スタート           
            keylogText.Text = "";
        }


        #endregion

        #region スタンプログを取得するメソッド

        /// <summary>
        /// スタンプログを取得するメソッド(外部では絶対にいじれないようにする)
        /// </summary>
        /// <returns>スタンプログのアドレス</returns>
        public List<MyTimeStamp> getTimeStamp()
        {
            return this.stamplog;
        }

        #endregion

        #region フォームを閉じる時に呼ばれるイベント

        /// <summary>
        /// フォームを閉じる時に呼ばれるイベント
        /// </summary>
        /// <param name="sender">登録するイベント</param>
        /// <param name="e">イベントオブジェクト</param>
        private void FormClosingEvent(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        #endregion

        #region ログをファイル出力するメソッド

        ///// <summary>
        ///// 同期ログを吐き出すメソッド
        ///// </summary>
        ///// <param name="fileName">パスを含むファイル名(拡張子はいらない)</param>
        //public void WriteStampLog(string fileName)
        //{ 
        //    string outputName = fileName + ".csv";

        //    //ファイルの書き出し
        //    using (StreamWriter sw = new StreamWriter(outputName, true, Encoding.GetEncoding("Shift_JIS")))
        //    {
        //        foreach (TimeStamp kk in stamplog)
        //        {
        //            string writeData = "";
        //            int pp = (kk.iskeypress == true) ? 1 : 0;
        //            writeData = string.Format("{0:D},{1:D}", kk.time, pp);
        //        }

        //        string tt = fileName + ".csv" + "に書き込みました";
        //        MessageBox.Show(tt, "SuccessWriteData", MessageBoxButtons.OK, MessageBoxIcon.None);
        //    }
        //}

        #endregion
    }

    #endregion
}
