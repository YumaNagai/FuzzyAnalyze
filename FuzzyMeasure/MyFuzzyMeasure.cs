using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace FuzzyMeasure
{
    /// <summary>
    /// メインフォ－ム
    /// </summary>
    public class MyFuzzyMeasure : Form
    {
        #region インスタンスフィールド

        #region 入力データ郡

        //入力データファイル郡-------------------------------------------------------
        private const int samplingRate = 512;   //サンプリングレート
        private Label inputlabel;               //入力データのラベル
        private TextBox inputfileName;          //入力ファイル名
        private Button inputwaveButton;           //入力ファイルの読み込みボタン

        private Label inputsynchrolabel;             //入力用同期データのラベル
        private TextBox inputsynchrofileName;   //入力用同期ファイルの名前
        private Button inputsynchrodataButton;         //同期ファイルの読み込みボタン

        //入力データ郡(ユーザ走査入力)-----------------------------------------------
        private Label input_secondLabel;          //観測秒数ラベル
        private NumericUpDown input_second;       //計測秒数取得
        private int input_sampleSecond;           //計測秒数を示す変数

        private Label input_ageLabel;             //年齢ラベル
        private NumericUpDown input_age;          //年齢取得
        private int input_ageNumber;              //年齢を示す変数

        private Label input_genderLabel;          //性別ラベル
        private ComboBox input_genderType;        //性別選択
        private string input_gender;              //性別を示す変数

        private Label input_analyzeLabel;         //解析の種類
        private ComboBox input_analyzeType;      //解析の種類の取得
        private string input_analyze;             //解析の種類を示す変数

        #endregion

        #region 出力データファイル郡

        private Label outputlabel;              //出力データラベル
        private TextBox outputfileName;         //出力データ名テキスト
        private Button outputWrite;             //データ書き込み用ボタン

        private Label outputsynchroLabel;       //同期データ出力ラベル
        private TextBox outputsynchrofileName;  //同期出力データ名テキスト
        private Button outputsynchroWrite;      //同期データ書き込みボタン

        private bool isAnalyzed;                //解析完了したかどうかのフラグ true:解析完了 false:解析終了

        #endregion

        #region 解析関連

        private Button fuzzyAnalyze;            //ファジー解析用ボタン
        private Button dataClear;               //データクリアボタン
        private List<double> waveData;          //波形データ
        private MyFuzzyAnalyze analyzeObject;   //ファジー解析＋表示オブジェクト
        //private bool isenableAnalyze;           //解析できるかどうかのフラグ true:可能 

        #endregion

        #region タイミング同期のためのインスタンス

        private MyTimingSynchroForm timingform;             //タイミング同期表示用クラス
        private MyDataSynchronization synchroKeylogger;     //同期データ生成バックグランドフォーム
        private List<MyTimeStamp> synchroTimeStamp;           //同期データ本体
        private List<MySynchroDataSet> synchroData;           //同期データの構造を記した物
       
        #endregion

        #region 解析モード設定のためのインスタンス

        private MainMenu mainMenu;          //メインメニュー
        private MenuItem analyzeMode;       //アナライズモード
        private MenuItem fixanalyze;        //固定点解析
        private MenuItem freeAnalyze;       //自由点解析
        private bool isfreeAnalyze;         //自由解析かどうかのフラグ(true:自由解析 false:固定点解析)

        //固定点解析=================================================================
        private Label fixedLabel;
        private Label nofixLabel;           //yes固定点ラベル
        private ComboBox nofixNumber;       //yes固定点の個数のコンボボックス
        private Label yesfixLabel;          //yes固定点ラベル
        private ComboBox yesfixNumber;      //yes固定点の個数のコンボボックス

        //自由点解析=================================================================
        private int yessindex = 0;
        private int nosindex = 0;
        private Label freeSampleLabel;
        private Label yesSampleLabel;               //yes解析点ラベル
        private NumericUpDown yesAnalyzeSample;     //yes解析点を指定するアップダウン
        private Label noSampleLabel;                //no解析点ラベル
        private NumericUpDown noAnalyzeSample;      //no解析点を指定するアップダウン

        #endregion

        private static OpenFileDialog dialog = new OpenFileDialog(); //ダイアログ

        #endregion

        #region コンストラクタ&デストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private MyFuzzyMeasure()
        {
            Console.WriteLine("new MyFuzzyMeasure");
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~MyFuzzyMeasure()
        {
            Console.WriteLine("delete MyFuzzyMeasure");
        }


        #endregion

        #region 初期化&生成メソッド

        /// <summary>
        /// 初期化メソッド
        /// </summary>
        private void Initialize()
        {
            this.Text = "MyFuzzyForm";
            this.Size = new System.Drawing.Size(480, 480);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;     //ウィンドウサイズ固定
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new System.Drawing.Point(100, 100);

            #region 入力データコンポーネントの初期化

            inputlabel = new Label();
            inputlabel.Text = "入力データ名：";
            inputlabel.Top = 20;
            inputlabel.Left = 20;
            inputlabel.Width = 85;

            inputfileName = new TextBox();
            inputfileName.ReadOnly = true;
            inputfileName.Top = inputlabel.Top - 3;
            inputfileName.Left = inputlabel.Right;
            inputfileName.Width = 230;

            inputwaveButton = new Button();
            inputwaveButton.Text = "ReadData";
            inputwaveButton.Top = inputfileName.Top - 2;
            inputwaveButton.Left = inputfileName.Right + 20;
            inputwaveButton.Width = 70;
            inputwaveButton.Click += new EventHandler(ReadFileClickEvent);

            inputsynchrolabel = new Label();
            inputsynchrolabel.Text = "同期ファイル名：";
            inputsynchrolabel.Top = inputlabel.Top + 40;
            inputsynchrolabel.Left = inputlabel.Left;
            inputsynchrolabel.Width = 85;

            inputsynchrofileName = new TextBox();
            inputsynchrofileName.ReadOnly = true;
            inputsynchrofileName.Top = inputsynchrolabel.Top - 3;
            inputsynchrofileName.Left = inputsynchrolabel.Right;
            inputsynchrofileName.Width = 230;

            inputsynchrodataButton = new Button();
            inputsynchrodataButton.Text = "ReadData";
            inputsynchrodataButton.Top = inputsynchrofileName.Top - 3;
            inputsynchrodataButton.Left = inputsynchrofileName.Right + 20;
            inputsynchrodataButton.Width = 70;
            inputsynchrodataButton.Click += new EventHandler(ReadSynchroFileClickEvent);

            this.Controls.Add(inputlabel);
            this.Controls.Add(inputfileName);
            this.Controls.Add(inputwaveButton);

            this.Controls.Add(inputsynchrolabel);
            this.Controls.Add(inputsynchrofileName);
            this.Controls.Add(inputsynchrodataButton);

            #endregion

            #region 入力項目のコンポーネントの初期化

            //秒数----------------------------------------------
            input_secondLabel = new Label();
            input_secondLabel.Text = "解析秒数[s]：";
            input_secondLabel.Top = inputsynchrolabel.Top + 50;
            input_secondLabel.Left = inputlabel.Left;
            input_secondLabel.Width = 75;

            input_second = new NumericUpDown();
            input_second.TextAlign = HorizontalAlignment.Right;
            input_second.Minimum = 0;
            input_second.Maximum = 999;
            input_second.Value = 0;
            input_second.Top = 10;
            input_second.Left = input_secondLabel.Right;
            input_second.Top = input_secondLabel.Top - 4;
            input_second.Width = 45;
            input_second.ValueChanged += new EventHandler(SecondChangeUpDownEvent);
            input_second.Enabled = false;

            //年齢------------------------------------------------
            input_ageLabel = new Label();
            input_ageLabel.Text = "年齢：";
            input_ageLabel.Top = input_secondLabel.Top;
            input_ageLabel.Left = input_second.Right + 10;
            input_ageLabel.Width = 40;

            input_age = new NumericUpDown();
            input_age.Minimum = 0;
            input_age.Maximum = 99;
            input_age.Value = 0;
            input_age.Left = input_ageLabel.Right;
            input_age.Top = input_ageLabel.Top - 4;
            input_age.Width = 40;
            input_age.TextAlign = HorizontalAlignment.Right;
            input_age.ValueChanged += new EventHandler(AgeChangeUpDownEvent);
            input_age.Enabled = false;

            input_ageNumber = 0;

            //性別のタイプ-----------------------------------------
            input_genderLabel = new Label();
            input_genderLabel.Text = "性別：";
            input_genderLabel.Top = input_secondLabel.Top;
            input_genderLabel.Left = input_age.Right + 10;
            input_genderLabel.Width = 40;

            input_genderType = new ComboBox();
            input_genderType.Items.Add("男");
            input_genderType.Items.Add("女");
            input_genderType.DropDownStyle = ComboBoxStyle.DropDownList;
            input_genderType.Top = input_genderLabel.Top - 4;
            input_genderType.Left = input_genderLabel.Right;
            input_genderType.Width = 35;
            input_genderType.SelectedIndex = 0;
            input_genderType.SelectedIndexChanged += new EventHandler(GenderTypeChanged);
            input_genderType.Enabled = false;

            input_gender = "男";


            //実験の種類--------------------------------------------------
            input_analyzeLabel = new Label();
            input_analyzeLabel.Text = "解析：";
            input_analyzeLabel.Top = input_secondLabel.Top;
            input_analyzeLabel.Left = input_genderType.Right + 10;
            input_analyzeLabel.Width = 40;

            input_analyzeType = new ComboBox();
            input_analyzeType.Items.Add("No");
            input_analyzeType.Items.Add("Yes");
            input_analyzeType.DropDownStyle = ComboBoxStyle.DropDownList;
            input_analyzeType.Top = input_analyzeLabel.Top - 4;
            input_analyzeType.Left = input_analyzeLabel.Right;
            input_analyzeType.Width = 45;
            input_analyzeType.SelectedIndex = 0;
            input_analyzeType.SelectedIndexChanged += new EventHandler(AnalyzeTypeChanged);
            input_analyzeType.Enabled = false;

            input_analyze = "No";

            //追加-------------------------------------------------------
            this.Controls.Add(input_secondLabel);
            this.Controls.Add(input_second);
            this.Controls.Add(input_ageLabel);
            this.Controls.Add(input_age);
            this.Controls.Add(input_genderLabel);
            this.Controls.Add(input_genderType);
            this.Controls.Add(input_analyzeLabel);
            this.Controls.Add(input_analyzeType);

            #endregion

            #region 出力用用コンポーネントの初期化

            outputlabel = new Label();
            outputlabel.Text = "出力ファイル名(require only name)：";
            outputlabel.Left = input_secondLabel.Left;
            outputlabel.Top = input_secondLabel.Top + 50;
            outputlabel.Width = 180;

            outputfileName = new TextBox();
            outputfileName.Left = outputlabel.Right;
            outputfileName.Top = outputlabel.Top - 3;
            outputfileName.Width = 120;

            outputWrite = new Button();
            outputWrite.Text = "AnalyzeDataWrite";
            outputWrite.Top = outputfileName.Top - 2;
            outputWrite.Left = outputfileName.Right + 20;
            outputWrite.Width = 105;
            outputWrite.Enabled = false;
            outputWrite.Click += new EventHandler(OutputWriteDataEvent);

            outputsynchroLabel = new Label();
            outputsynchroLabel.Text = "出力ファイル名(require only name)：";
            outputsynchroLabel.Top = outputlabel.Top + 40;
            outputsynchroLabel.Left = outputlabel.Left;
            outputsynchroLabel.Width = 180;

            outputsynchrofileName = new TextBox();
            outputsynchrofileName.Left = outputsynchroLabel.Right;
            outputsynchrofileName.Top = outputsynchroLabel.Top - 3;
            outputsynchrofileName.Width = 120;

            outputsynchroWrite = new Button();
            outputsynchroWrite.Text = "SynchroDataWrite";
            outputsynchroWrite.Left = outputsynchrofileName.Right + 20;
            outputsynchroWrite.Top = outputsynchrofileName.Top - 2;
            outputsynchroWrite.Width = 105;
            outputsynchroWrite.Enabled = false;
            outputsynchroWrite.Click += new EventHandler(OutputWriteSynchroDataEvent);

            this.Controls.Add(outputlabel);
            this.Controls.Add(outputfileName);
            this.Controls.Add(outputWrite);
            this.Controls.Add(outputsynchroLabel);
            this.Controls.Add(outputsynchrofileName);
            this.Controls.Add(outputsynchroWrite);

            #endregion

            #region 固定点解析用コンポーネントの初期化

            fixedLabel = new Label();
            fixedLabel.Text = "固定点解析用パラメータ";
            fixedLabel.Left = outputsynchroLabel.Left;
            fixedLabel.Top  = outputsynchroLabel.Top + 40;
            fixedLabel.Width = 125;
            fixedLabel.Height = 15;

            //no--------------------------------------------
            nofixLabel = new Label();
            nofixLabel.Text = "処理No解析領域Index：";
            nofixLabel.Left = fixedLabel.Left;
            nofixLabel.Top = fixedLabel.Top + 25;
            nofixLabel.Width = 125;

            nofixNumber = new ComboBox();
            nofixNumber.Left = nofixLabel.Right;
            nofixNumber.Top = nofixLabel.Top - 3;
            nofixNumber.Width = 30;
            nofixNumber.DropDownStyle = ComboBoxStyle.DropDownList;
            nofixNumber.Enabled = false;

            //yes---------------------------------------------------
            yesfixLabel = new Label();
            yesfixLabel.Text = "処理Yes解析領域Index：";
            yesfixLabel.Left = nofixNumber.Right + 70;
            yesfixLabel.Top = fixedLabel.Top + 25;
            yesfixLabel.Width = 130;

            yesfixNumber = new ComboBox();
            yesfixNumber.Left = yesfixLabel.Right;
            yesfixNumber.Top = yesfixLabel.Top - 3;
            yesfixNumber.Width = 30;
            yesfixNumber.DropDownStyle = ComboBoxStyle.DropDownList;
            yesfixNumber.Enabled = false;

            this.Controls.Add(fixedLabel);
            this.Controls.Add(nofixLabel);
            this.Controls.Add(nofixNumber);
            this.Controls.Add(yesfixLabel);
            this.Controls.Add(yesfixNumber);

            #endregion

            #region 自由点解析コンポーネントの初期化

            freeSampleLabel = new Label();
            freeSampleLabel.Text = "自由点解析用パラメータ";
            freeSampleLabel.Top = fixedLabel.Top + 70;
            freeSampleLabel.Left = fixedLabel.Left;
            freeSampleLabel.Width = 125;
            freeSampleLabel.Height = 15;

            noSampleLabel = new Label();
            noSampleLabel.Text = "No解析点(Index)：";
            noSampleLabel.Left = freeSampleLabel.Left;
            noSampleLabel.Top =  freeSampleLabel.Top + 20;
            noSampleLabel.Width = 105;

            noAnalyzeSample = new NumericUpDown();
            noAnalyzeSample.Top = noSampleLabel.Top - 4;
            noAnalyzeSample.Left = noSampleLabel.Right;
            noAnalyzeSample.Width = 70;
            noAnalyzeSample.TextAlign = HorizontalAlignment.Right;
            noAnalyzeSample.Maximum = 9999999;
            noAnalyzeSample.Minimum = 0;
            noAnalyzeSample.Value = 0;
            noAnalyzeSample.Enabled = false;
            noAnalyzeSample.ValueChanged += new EventHandler(noSampleChangeEvent);

            yesSampleLabel = new Label();
            yesSampleLabel.Text = "Yes解析点(Index)：";
            yesSampleLabel.Left = noAnalyzeSample.Right + 40;
            yesSampleLabel.Top = noSampleLabel.Top;
            yesSampleLabel.Width = 105;

            yesAnalyzeSample = new NumericUpDown();
            yesAnalyzeSample.Top = yesSampleLabel.Top - 4;
            yesAnalyzeSample.Left = yesSampleLabel.Right;
            yesAnalyzeSample.Width = 70;
            yesAnalyzeSample.TextAlign = HorizontalAlignment.Right;
            yesAnalyzeSample.Maximum = 9999999;
            yesAnalyzeSample.Minimum = 0;
            yesAnalyzeSample.Value = 0;
            yesAnalyzeSample.Enabled = false;
            yesAnalyzeSample.ValueChanged += new EventHandler(yesSampleChangeEvent);

            this.Controls.Add(freeSampleLabel);
            this.Controls.Add(yesSampleLabel);
            this.Controls.Add(yesAnalyzeSample);
            this.Controls.Add(noSampleLabel);
            this.Controls.Add(noAnalyzeSample);

            #endregion

            #region 解析&データクリアボタンコンポーネント初期化


            fuzzyAnalyze = new Button();
            fuzzyAnalyze.Text = "FuzzyAnalyze";
            fuzzyAnalyze.Width = 100;
            fuzzyAnalyze.Left = this.Size.Width / 2 - fuzzyAnalyze.Width / 2 - 100;
            fuzzyAnalyze.Top = yesSampleLabel.Top + 50;
            fuzzyAnalyze.Click += new EventHandler(FuzzyAnalyzeEvent);
            fuzzyAnalyze.Enabled = false;

            dataClear = new Button();
            dataClear.Text = "DataAllClear";
            dataClear.Width = 100;
            dataClear.Left = this.Size.Width / 2 - dataClear.Width / 2 + 100;
            dataClear.Top = fuzzyAnalyze.Top;
            dataClear.Click += new EventHandler(DataAllClearEvent);

            //this.Controls.Add(revDataReset);
            this.Controls.Add(fuzzyAnalyze);
            this.Controls.Add(dataClear);

            #endregion

            #region タイミング同期用インスタントの初期化

            synchroKeylogger = MyDataSynchronization.Load();
            synchroTimeStamp = new List<MyTimeStamp>();
            synchroData = new List<MySynchroDataSet>();

            #endregion

            #region 解析モードメニューの作成

            isfreeAnalyze = false;                  //初期は固定点
            mainMenu = new MainMenu();
            analyzeMode = new MenuItem("解析モード");

            fixanalyze = new MenuItem("固定点解析");
            fixanalyze.Checked = true;
            fixanalyze.Click += new EventHandler(FixMenuEvent);

            freeAnalyze = new MenuItem("自由点解析");
            freeAnalyze.Checked = false;
            freeAnalyze.Click += new EventHandler(FreeMenuEvent);

            analyzeMode.MenuItems.Add(fixanalyze);
            analyzeMode.MenuItems.Add(freeAnalyze);

            mainMenu.MenuItems.Add(analyzeMode);

            this.Menu = mainMenu;

            #endregion


            //波データリストのオブジェクト生成
            waveData = new List<double>();

            //ダイアログ設定
            dialog.Filter = "読み込みファイル(*.csv)|*.csv;";
            dialog.Multiselect = false;

            this.Icon = new System.Drawing.Icon("fuzzyAnalyze.ico");


        }

        /// <summary>
        /// 生成メソッド
        /// </summary>
        /// <returns>生成されたMyFuzzyMeasureオブジェクト</returns>
        public static MyFuzzyMeasure Load()
        {
            MyFuzzyMeasure mfm = new MyFuzzyMeasure();
            mfm.Initialize();
            return mfm;
        }

        #endregion

        #region イベントメソッド

        #region 入力(波データ・同期データ・解析入力データ群のイベントメソッド)

        #region 入力データのイベント

        /// <summary>
        /// 性別の変更
        /// </summary>
        /// <param name="sender">イベント登録オブジェクト</param>
        /// <param name="e">イベントデータ</param>
        private void GenderTypeChanged(Object sender, EventArgs e)
        {
            switch (input_genderType.SelectedIndex)
            {
                //男
                case 0:
                    input_gender = "男";
                break;

                //女
                case 1:
                    input_gender = "女";
                break;

                default:

                break;

            }

        }

        /// <summary>
        /// 実験種類の変更
        /// </summary>
        /// <param name="sender">イベント登録オブジェクト</param>
        /// <param name="e">イベントデータ</param>
        private void AnalyzeTypeChanged(Object sender, EventArgs e)
        {
            switch (input_analyzeType.SelectedIndex)
            {

                #region Noの処理の時

                case 0:
                {
                    input_analyze = "No";

                    //固定点解析
                    if (isfreeAnalyze == false)
                    {
                        nofixNumber.Enabled = true;
                        yesfixNumber.Enabled = false;
                    }

                    //自由点解析
                    else
                    { 
                        noAnalyzeSample.Enabled = true;
                        yesAnalyzeSample.Enabled = false;
                    }

                }break;

                #endregion

                //Yes
                case 1:
                    input_analyze = "Yes";

                    //固定点解析
                    if (isfreeAnalyze == false)
                    {
                        nofixNumber.Enabled = false;
                        yesfixNumber.Enabled = true;
                    }

                    //自由点解析
                    else
                    {
                        noAnalyzeSample.Enabled = false;
                        yesAnalyzeSample.Enabled = true;
                    }
                break;

                default:

                    break;
            }
        }


        /// <summary>
        /// 解析時間変更時
        /// </summary>
        /// <param name="sender">登録するオブジェクト</param>
        /// <param name="e">イベントデータ</param>
        private void SecondChangeUpDownEvent(Object sender, EventArgs e)
        {
            input_sampleSecond = (int)input_second.Value;
        }

        /// <summary>
        /// 年齢変更時
        /// </summary>
        /// <param name="sender">登録するオブジェクト</param>
        /// <param name="e">イベントデータ</param>
        private void AgeChangeUpDownEvent(Object sender, EventArgs e)
        {
            input_ageNumber = (int)input_age.Value;
        }

        #endregion

        #region ファイル読み込みボタンのイベントメソッド

        /// <summary>
        /// ファイル読み込みボタンのイベント関数
        /// </summary>
        /// <param name="sender">登録するオブジェクトのアドレス</param>
        /// <param name="args">イベントデータ</param>
        private void ReadFileClickEvent(Object sender, EventArgs e)
        {
            //ダイアログ結果
            DialogResult result = dialog.ShowDialog();

            //ダイアログ結果がOKの時
            if (result == DialogResult.OK)
            {
                List<string> linebuf = new List<string>();            //データ作成用の文字列配列

                //テキスト読み出し
                using (StreamReader st = new StreamReader(dialog.OpenFile()))
                {
                    string pp;      //一時読み込み変数

                    //データ全読み込み
                    while ((pp = st.ReadLine()) != null) linebuf.Add(pp);
                }

                //最初行は文字列データだけであるため削除
                linebuf.RemoveAt(0);

                //データセットのデータを開放
                waveData.Clear();

                //構造体データに変換
                foreach (string str in linebuf)
                {
                    //[0]:timeData [1]:valueData
                    string[] param = str.Split(',');

                    //例外処理
                    if (param.Length != 2)
                    {
                        string ll = string.Format("RawWalueのデータ形式ではありません");
                        MessageBox.Show(ll, "Not WaveData", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    double tt = double.Parse(param[1]);

                    //データセット
                    waveData.Add(tt);
                }

                //読み込みがすべて成功したらファイル名表示
                inputfileName.Text = dialog.FileName;                 
                linebuf.Clear();

                //入力波形データが入っている場合,入力データをいじれるようにする
                bool enablecreate = (string.Equals(inputsynchrofileName.Text, "")) ? false : true;

                //各種データの設定や生成を行う
                if (enablecreate == true) SettingData();

            }
        }

        #endregion

        #region 同期ファイル読み込みボタンのイベントメソッド

        /// <summary>
        /// ファイル読み込みボタンのイベント関数
        /// </summary>
        /// <param name="sender">登録するオブジェクトのアドレス</param>
        /// <param name="args">イベントデータ</param>
        private void ReadSynchroFileClickEvent(Object sender, EventArgs e)
        {
            synchroTimeStamp = synchroKeylogger.getTimeStamp();

            #region 同期データがキーによって生成されている場合

            //同期データがキーによって生成されている場合はそちらを優先
            if (synchroTimeStamp.Count != 0)
            {
                inputsynchrofileName.Text = "KeySyncronazationData";
                outputsynchroWrite.Enabled = true;
            }

            #endregion

            #region そうでない場合

            //用意されているデータを読み込むのであればファイル解析
            else
            {
                //ダイアログ結果
                DialogResult result = dialog.ShowDialog();

                //ダイアログ結果がOKの時
                if (result == DialogResult.OK)
                {
                    
                    List<string> linebuf = new List<string>();              //データ作成用の文字列配列

                    //テキスト読み出し
                    using (StreamReader st = new StreamReader(dialog.OpenFile()))
                    {
                        string pp;      //一時読み込み変数

                        //データ全読み込み
                        while ((pp = st.ReadLine()) != null) linebuf.Add(pp);
                    }

                    synchroTimeStamp.Clear();     //一旦クリアする

                    //構造体データに変換
                    foreach (string str in linebuf)
                    {
                        //[0]:time(long) [1]:1 or 0(1 is keypress , 0 is keyrelease)
                        string[] param = str.Split(',');

                        //例外処理
                        if (param.Length != 2)
                        {
                            string ll = string.Format("Synchroのデータ形式ではありません");
                            MessageBox.Show(ll, "Not SynchroData", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        int kk;
                        long tt;        //構造体パラメータ[時間]
                        bool pp = false;        //構造体パラメータ[キー押下 1:押 0:離]
                        bool aa,bb;

                        aa = long.TryParse(param[0], out tt);
                        bb = int.TryParse(param[1], out kk);

                        //データの形式はあっているが，データの型が間違っている時
                        if(!(aa == true && bb == true))
                        {
                            string ll="";
                            if (aa == true && bb == true)  ll = "Both Data's not SynchroType";
                            if (aa == true && bb == false) ll = "First Data  not SynchroType";
                            if (aa == false && bb == true) ll = "Second Data not SynchroType";

                            MessageBox.Show(ll, "Not SynchroData", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        pp = (kk == 1) ? true : false;

                        MyTimeStamp tmp = new MyTimeStamp(tt, pp);

                        //データセット
                        synchroTimeStamp.Add(tmp);
                    }

                    //すべての中身が解析され終わった時
                    inputsynchrofileName.Text = dialog.FileName;           
                }
            }

            #endregion

            //入力波形データが入っている場合,入力データをいじれるようにする
            bool enablecreate = (string.Equals(inputfileName.Text, "")) ? false : true;

            //各種データの設定や生成を行う
            if (enablecreate == true) SettingData();
        }

        #endregion

        #region 各種生成処理

        /// <summary>
        /// 各種データの設定＋タイミングチャートフォームの生成
        /// </summary>
        private void SettingData()
        {
            //同期データ＆タイミングチャート生成
            this.createSynchroIndex();
            timingform = MyTimingSynchroForm.Load(waveData, synchroData);
    
            //波データ＋同期データの読み込みを不可とする
            inputwaveButton.Enabled = false;
            inputsynchrodataButton.Enabled = false;

            //解析用入力データの入力を可能にする
            input_second.Enabled = true;
            input_age.Enabled = true;
            input_genderType.Enabled = true;
            input_analyzeType.Enabled = true;
            
            //解析ボタンを使用可能に
            fuzzyAnalyze.Enabled = true;
            
            //固定点解析(解析用アップダウンは使用できない)
            if (isfreeAnalyze == false)
            {
                List<int> nsindex = new List<int>();
                List<int> ysindex = new List<int>();

                for (int ii = 0; ii < synchroData.Count; ii++)
                {
                    if (string.Equals(synchroData[ii].type, "No"))
                    {
                        nsindex.Add(synchroData[ii].startindex);
                    }

                    else
                    {
                        ysindex.Add(synchroData[ii].startindex);
                    }
                }
                timingform.defaultInformation(ysindex, nsindex);     //フォームに情報を表示

                //this.createynindex();                                //それぞれ1番目のyes,noを生成
                yesAnalyzeSample.Enabled = false;                       
                noAnalyzeSample.Enabled = false;
                CreateFixedPointNumber();
                
                if(string.Equals(input_analyze,"No")) nofixNumber.Enabled = true;
                
                else yesfixNumber.Enabled = true; 

            }

            //自由点解析(解析用アップダウンを使用できる)
            else
            {
                string tt ="";
                int kk = 0;

                //No選択時
                if(string.Equals(input_analyze, "No")==true)
                {  
                    tt = "    SelectMode:Yes";
                    kk = (int)noAnalyzeSample.Value;
                    noAnalyzeSample.Enabled = true;
                    
                }

                //Yes選択時
                else
                {
                    tt = "    SelectMode:No";
                    kk = (int)yesAnalyzeSample.Value;
                    yesAnalyzeSample.Enabled = true;
                }

                //フォームに情報を表示
                timingform.displayCurrentInformation(kk,tt);
            }

            //解析モードを変えられないようにする
            analyzeMode.Enabled = false;
        }

        #endregion

        #endregion

        #region ファジー解析実行のイベントボタン

        /// <summary>
        /// ファジー解析イベント関数
        /// </summary>
        /// <param name="sender">登録するオブジェクトのアドレス</param>
        /// <param name="args">イベントデータ</param>
        private void FuzzyAnalyzeEvent(Object sender, EventArgs e)
        {
            bool aa = false;
            int tsample = 0;
            int ii = 0;
            int count = synchroData.Count;
            MySynchroDataSet data = new MySynchroDataSet();

            //固定点解析
            if (isfreeAnalyze == false)
            {
                int selectindex=0;
                
                //解析タイプがNoのとき
                if (string.Equals(input_analyze, "No"))
                {
                    selectindex = int.Parse((string)nofixNumber.SelectedItem);
                }

                //解析タイプがYesのとき
                else
                {
                    selectindex = int.Parse((string)yesfixNumber.SelectedItem);
                }
                

                aa = FixedCheckAnalyzeDataIntegrity(selectindex);   //整合性をチェック
                if (aa == false) return;                          //問題があったら処理を抜ける  

                data = synchroData[selectindex];
                
            }

            //自由点解析
            else
            {
                aa = FreeCheckAnalyzeDataIntegrity();       //整合性をチェック
                if (aa == false) return;                    //問題があったら処理を抜ける  

                //Yesの場合
                if (string.Equals(input_analyze, "Yes")) tsample = (int)yesAnalyzeSample.Value;

                //Noの場合
                else tsample = (int)noAnalyzeSample.Value;

                while (ii<count)
                {
                    bool pp = (string.Equals(synchroData[ii].type, input_analyze)) &&
                              (synchroData[ii].startindex >= tsample && synchroData[ii].lastindex <= tsample);

                    //解析タイプと同期データ構造体のタイプが一致した時
                    if (pp == true)
                    {
                        data = synchroData[ii];
                        break;
                    }
                    ii++;
                }
            }

            double[] wave = createInputData((input_sampleSecond * samplingRate), data, tsample);  //解析データ生成
            analyzeObject = MyFuzzyAnalyze.Load(wave);                                      //解析オブジェクト生成
            isAnalyzed = true;                                                              //解析終了
            outputWrite.Enabled = isAnalyzed;        
        }


        #endregion

        #region データクリアのイベントメソッド

        /// <summary>
        /// ファイル読み込みボタンのイベント関数
        /// </summary>
        /// <param name="sender">登録するオブジェクトのアドレス</param>
        /// <param name="args">イベントデータ</param>
        private void DataAllClearEvent(Object sender, EventArgs e)
        {
            Console.WriteLine("ClearAll");

            #region 入力ファイルデータ(波、同期)+入力データ初期化

            //入力関連データの初期化-----------------------------------------------------
            inputfileName.Text = "";                    //入力ファイル
            inputsynchrofileName.Text = "";             //入力同期ファイル
            inputwaveButton.Enabled = true;             //入力ファイルボタン読み込みボタン
            inputsynchrodataButton.Enabled = true;      //入力同期ファイル読み込みボタン
            input_second.Enabled = false;
            input_age.Enabled = false;
            input_genderType.Enabled = false;
            input_analyzeType.Enabled = false;
            waveData.Clear();                           //波データの開放
       

            #endregion

            #region 同期関連データ初期化
            
            //固定点解析============================
            nofixNumber.Items.Clear();
            nofixNumber.Enabled = false;

            yesfixNumber.Items.Clear();
            yesfixNumber.Enabled = false;
            //=======================================


            //同期関連データの初期化-----------------------
            if (timingform != null) timingform.Close();        //タイミングチャートをクローズ
            synchroKeylogger.AllResetData();                //キーログ情報をリセット
            synchroData.Clear();                            //同期データクリア
            synchroTimeStamp.Clear();                       //同期タイムスタンプクリア

            yesAnalyzeSample.Enabled = false;
            noAnalyzeSample.Enabled = false;
            analyzeMode.Enabled = true;          

            #endregion

            #region 出力データ郡+データ出力ボタン

            //出力データ郡の初期化
            //outputfileName.Text = "";
            outputWrite.Enabled = false;
            //outputsynchrofileName.Text = "";
            outputsynchroWrite.Enabled = false;

            //(入力データが読み込まれていない状態なので)ファジー解析ボタン無効
            fuzzyAnalyze.Enabled = false;
            isAnalyzed = false;
            //iswrite = false; 

            //解析オブジェクトがある場合
            if (analyzeObject != null)
            {
                analyzeObject.ReleaseComponent();
                analyzeObject = null;
            }

            #endregion

        }


        #endregion

        #region 出力系イベントメソッド

        #region 解析データ出力のイベントメソッド

        /// <summary>
        /// データ出力のイベントメソッド(現在ウィンドウに出ている解析データ)
        /// </summary>
        /// <param name="sender">登録するオブジェクトのアドレス</param>
        /// <param name="args">イベントデータ</param>
        private void OutputWriteDataEvent(Object sender, EventArgs e)
        {
            //入力データが明らかにおかしい時
            if (input_sampleSecond == 0 || input_ageNumber == 0)
            {
                string tt = "";

                if ((input_sampleSecond == 0) && (input_ageNumber == 0)) tt = "秒数,年齢ともに間違っています";
                else if ((input_sampleSecond == 0)) tt = "0秒では解析出来ません";
                else if ((input_ageNumber == 0)) tt = "0歳は対象にしていません";
                MessageBox.Show(tt, "InputData Wrong", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //出力ファイル名が無いとき
            if (string.Equals(outputfileName.Text, "") == true)
            {
                string tt = "ファイル名がありません";
                MessageBox.Show(tt, "Empty FileName", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string[] inputdata = new string[5];
            inputdata[0] = string.Format("{0:D}", input_sampleSecond);
            inputdata[1] = string.Format("{0:D}", input_ageNumber);
            inputdata[2] = input_gender;
            inputdata[3] = input_analyze;
            inputdata[4] = inputfileName.Text;      //フルパスファイル名
            analyzeObject.WriteAnalyzeData(outputfileName.Text, inputdata);
            //iswrite = true;
            //revDataReset.Enabled = true;
            //outputWrite.Enabled = false;
        }

        #endregion

        #region 同期用データ出力のイベントメソッド

        /// <summary>
        /// 同期用データ出力のイベントメソッド(現在読み込んでいる同期データ)
        /// </summary>
        /// <param name="sender">登録するオブジェクトのアドレス</param>
        /// <param name="args">イベントデータ</param>
        private void OutputWriteSynchroDataEvent(Object sender, EventArgs e)
        {
            //入力データが明らかにおかしい時
            if (string.Equals(outputsynchrofileName.Text, ""))
            {
                string tt = "ファイル名がありません";
                MessageBox.Show(tt, "Empty FileName", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string outputName = outputsynchrofileName.Text + "Synchro" + ".csv";

            //ファイルの書き出し
            using (StreamWriter sw = new StreamWriter(outputName, false, Encoding.GetEncoding("Shift_JIS")))
            {
                foreach (MyTimeStamp kk in synchroTimeStamp)
                {
                    string writeData = "";
                    int pp = (kk.iskeypress == true) ? 1 : 0;
                    writeData = string.Format("{0:D},{1:D}", kk.time, pp);
                    sw.WriteLine(writeData);
                }

                string tt = outputName + "に書き込みました";
                MessageBox.Show(tt, "SuccessWriteData", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
        }

        #endregion
 
        #endregion
        
        #region 解析モードのメニューイベント

        /// <summary>
        /// 固定解析モード
        /// </summary>
        /// <param name="sender">登録オブジェクト</param>
        /// <param name="e">イベントメソッド</param>
        private void FixMenuEvent(Object sender, EventArgs e)
        {
            foreach (MenuItem item in analyzeMode.MenuItems)
            {
                //登録されているオブジェクトと一致している時
                if (object.ReferenceEquals(item, sender))
                {
                    item.Checked = true;
                }

                else
                {
                    item.Checked = false;
                }
            }
            isfreeAnalyze = false;
            yesAnalyzeSample.Enabled = false;
            noAnalyzeSample.Enabled = false;

        }


        /// <summary>
        /// 自由解析モード
        /// </summary>
        /// <param name="sender">登録オブジェクト</param>
        /// <param name="e">イベントメソッド</param>
        private void FreeMenuEvent(Object sender, EventArgs e)
        {
            foreach (MenuItem item in analyzeMode.MenuItems)
            {
                //登録されているオブジェクトと一致している時
                if (object.ReferenceEquals(item, sender))
                {
                    item.Checked = true;
                }

                else
                {
                    item.Checked = false;
                }
            }
            isfreeAnalyze = true;

            if (string.Equals(input_analyze, "Yes") == true) yesAnalyzeSample.Enabled = true;
            else noAnalyzeSample.Enabled = false;



        }

        #endregion

        #endregion

        #region 入力データと同期データからそれぞれのデータの開始番号を作るメソッド

        /// <summary>
        /// 入力データと同期データからそれぞれのデータの開始番号を作るメソッド
        /// </summary>
        private void createSynchroIndex()
        {
            /*------------------------------------------------------------------*
             * 使用機器(Ba3Band)のサンプリングレートが512Hzであるため           *
             * 1/512 = 0.001953125≒0.002[s]=2[ms]がサンプリング時間であるため  *
             * 2で割るとその地点でのサンプル番号(インデクッス)が算出される      *
             *------------------------------------------------------------------*/

            int jj = (int)(synchroTimeStamp[0].time / 2);       //スタートインデクス(波の)
            bool nsync = false, sync = false;                   //非同期が連続しているかのフラグ
            
            //一時保存用
            List<int> nsindex, nlindex, ysindex, ylindex;
            nsindex = new List<int>();
            nlindex = new List<int>();
            ysindex = new List<int>();
            ylindex = new List<int>();

            //タイムスタンプの個数
            int count = synchroTimeStamp.Count;

            //No
            while (jj < count)
            {
                //非同期(No)部分の検出
                if (synchroTimeStamp[jj].iskeypress == false && nsync == false)
                {
                    nsindex.Add((int)(synchroTimeStamp[jj].time / 2));
                    nsync = true;
                }

                //Noの終端部分を検出
                else if (synchroTimeStamp[jj].iskeypress == true && nsync == true)
                {
                    nlindex.Add((int)(synchroTimeStamp[jj - 1].time / 2));
                    nsync = false;
                }
                jj++;
            }

            //一致しない時
            if (nsindex.Count != nlindex.Count) nlindex.Add((int)(synchroTimeStamp[synchroTimeStamp.Count - 1].time / 2));
            
            
            jj = (int)(synchroTimeStamp[0].time / 2);

            //Yes
            while (jj < count)
            {
                //同期(Yes)部分の検出
                if (synchroTimeStamp[jj].iskeypress == true && sync == false)
                {
                    ysindex.Add((int)(synchroTimeStamp[jj].time / 2));
                    sync = true;
                }

                else if (synchroTimeStamp[jj].iskeypress == false && sync == true)
                {
                    ylindex.Add((int)(synchroTimeStamp[jj - 1].time / 2));
                    sync = false;
                }

                jj++;
            }

            //一致しない時
            if (ysindex.Count != ylindex.Count) ylindex.Add((int)(synchroTimeStamp[synchroTimeStamp.Count - 1].time / 2));

            //1個以上あるならば
            if (ysindex.Count > 0 && nsindex.Count > 0)
            {
                for (int kk = 0, ll = 0; ; kk++, ll++)
                {
                    //No
                    if (ll < nsindex.Count)
                    {
                        MySynchroDataSet nn = new MySynchroDataSet(nsindex[ll], nlindex[ll], "No");
                        synchroData.Add(nn);
                    }

                    //Yes
                    if (kk < ysindex.Count)
                    {
                        MySynchroDataSet yy = new MySynchroDataSet(ysindex[kk], ylindex[kk], "Yes");
                        synchroData.Add(yy);
                    }
                    if (ll >= (nsindex.Count - 1) && kk >= (ysindex.Count - 1)) break;
                }
            }

            nsindex.Clear();
            nlindex.Clear();
            ysindex.Clear();
            ylindex.Clear();

            //昇順ソート
            synchroData.Sort(delegate(MySynchroDataSet a, MySynchroDataSet b) { return a.startindex - b.startindex; });
        }


        #endregion

        #region 同期データから入力データを生成するメソッド

        /// <summary>
        /// 同期を加味した脳波データ
        /// </summary>
        /// <param name="createcount">生成するデータ数</param>
        /// <param name="data">同期用データセット</param>
        /// <param name="offset">オフセット値(開始インデックス)</param>
        /// <returns>同期した脳波データ</returns>
        private double[] createInputData(int createcount, MySynchroDataSet data,int offset=0)
        {
            double[] wave = new double[createcount];
            int ii = (data.startindex < offset) ? offset : data.startindex;
            int last = ii + createcount;

            for (int jj = 0; ii < last; jj++, ii++)
            {
                wave[jj] = waveData[ii];
            }
            return wave;
        }


        #endregion

        #region 固定点解析用メソッド

        #region 解析秒と同期データ、波形データとの整合性をチェックするメソッド

        /// <summary>
        /// 解析秒と同期データ、波形データとの整合性をチェックするメソッド
        /// </summary>
        /// <returns>true:解析可能　false:解析不可能</returns>
        private bool FixedCheckAnalyzeDataIntegrity(int selectindex)
        {
            //秒数が0の時
            if (input_sampleSecond == 0)
            {
                string tt = string.Format("0秒では解析出来ません");
                MessageBox.Show(tt, "Don't Analyze", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            //波データが解析秒数分ないとき
            if ((input_sampleSecond * samplingRate) > waveData.Count)
            {
                string tt = "波データが解析秒数分無いため解析できません。";
                MessageBox.Show(tt, "Shortage WaveData", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            //選択領域の秒数が解析秒数あるか
            int kk =  synchroData[selectindex].lastindex - synchroData[selectindex].startindex;
            if ((input_sampleSecond * samplingRate) > kk)
            {
                string tt = string.Format("{0:D}で解析秒数分の領域がありません",selectindex);
                MessageBox.Show(tt, "Shortage WaveData", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            
            return true;

        }



        #endregion

        #region 一番最初のYes、Noの部分の開始点を生成するメソッド

        /// <summary>
        /// 一番最初のYes、Noの部分の開始点を生成するメソッド
        /// </summary>
        private void createynindex()
        {
            bool checky = false, checkn = false;        //最初のYes、Noをチェックしたかのフラグ

            for (int ii = 0; ii < synchroData.Count; ii++)
            {
                //最初のYes
                if (checky == false && string.Equals(synchroData[ii].type, "Yes"))
                {
                    yessindex = synchroData[ii].startindex;
                    checky = true;
                }

                //最初のNo
                else if (checkn == false && string.Equals(synchroData[ii].type, "No"))
                {
                    nosindex = synchroData[ii].startindex;
                    checkn = true;
                }

                //チェック終了
                if (checky == true && checkn == true) break;
            }
        }


        #endregion

        #region 生成された固定点から番号を得るメソッド

        /// <summary>
        /// 生成された固定点から解析領域のインデクスを得るメソッド
        /// </summary>
        private void CreateFixedPointNumber()
        {
            for (int ii = 0; ii < synchroData.Count; ii++)
            {
                if (string.Equals(synchroData[ii].type, "No"))
                {
                    nofixNumber.Items.Add(string.Format("{0:D}", ii));
                }

                else
                {
                    yesfixNumber.Items.Add(string.Format("{0:D}", ii));
                }
            }
        }

        #endregion

        #endregion

        #region 自由点解析用メソッド

        #region 自由点の解析ができるかどうかの整合性をチェックするメソッド

        /// <summary>
        /// 自由点の解析ができるかどうかの整合性をチェックするメソッド
        /// </summary>
        /// <returns>true:解析可 false:解析不可</returns>
        private bool FreeCheckAnalyzeDataIntegrity()
        {
            int count   = synchroData.Count;
            int tsample = 0; 
            MySynchroDataSet data = new MySynchroDataSet();
            bool pickup = false;

            //Yesの場合
            if(string.Equals(input_analyze, "Yes")) tsample = (int)yesAnalyzeSample.Value;
            
            //Noの場合
            else tsample = (int)noAnalyzeSample.Value;

            //該当する構造体を抜き出す
            for (int ii = 0; ii < count; ii++)
            {
                bool kk = (tsample >= synchroData[ii].startindex && tsample <= synchroData[ii].lastindex); 
                bool ss = (string.Equals(synchroData[ii].type,input_analyze));

                //該当がある場合
                if (kk == true && ss == true)
                {
                    data = synchroData[ii];
                    pickup = true;
                    break;
                }

                //番号は該当するが解析領域が違うとき
                else if (kk == true && ss == false)
                {
                    string tt = "選択している解析と該当する領域の種類が違います";
                    MessageBox.Show(tt, "MismatchAnalyzeRegion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            //該当する構造体がなかった時
            if (pickup == false)
            {
                string tt = "該当サンプルがありません";
                MessageBox.Show(tt, "OverSample", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            //該当する構造体がなかった時
            if (input_sampleSecond == 0)
            {
                string tt = string.Format("0秒では解析出来ません");
                MessageBox.Show(tt, "Don't Analyze", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            //その解析点から解析秒数分のサンプルがあるかどうかをチェック
            bool isserve = ((tsample+(input_sampleSecond * samplingRate)) <= data.lastindex);

            //解析秒分足りない
            if (isserve == false)
            {
                string tt = string.Format("解析点{0:D}から{1:D}秒分のデータが取れません",tsample,input_sampleSecond);
                MessageBox.Show(tt, "ShortageSample", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return isserve;
        }

        #endregion

        #region タイミング同期コンポーネントのイベントメソッド

        /// <summary>
        /// Yes数値指定用アップダウンの値が変更された時
        /// </summary>
        /// <param name="sender">登録するオブジェクト</param>
        /// <param name="e">イベントデータ</param>
        private void yesSampleChangeEvent(Object sender, EventArgs e)
        {
            //this.FreeCheckAnalyzeDataIntegrity();
            int tmp = (int)yesAnalyzeSample.Value;
            timingform.displayCurrentInformation(tmp,"    SelectMode:Yes");
        }

        /// <summary>
        /// No数値指定用アップダウンの値が変更された時
        /// </summary>
        /// <param name="sender">登録するオブジェクト</param>
        /// <param name="e">イベントデータ</param>
        private void noSampleChangeEvent(Object sender, EventArgs e)
        {
            //this.FreeCheckAnalyzeDataIntegrity();
            int tmp = (int)noAnalyzeSample.Value;
            timingform.displayCurrentInformation(tmp,"    SelectMode:No");
        }

        #endregion

        #endregion

    }
}