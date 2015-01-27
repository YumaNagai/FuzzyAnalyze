using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Windows.Forms;
using System.IO;

//ExcelApplicationとFormを動かすためのstaticクラスのApplicationクラスの名前が衝突
//するためあえてこのように記述する
using Excel = Microsoft.Office.Interop.Excel;
using chartControl = System.Windows.Forms.DataVisualization.Charting;


namespace FuzzyMeasure
{
    #region ファジー解析の要素となるクラス郡

    /// <summary>
    /// メンバーシップ関数の値を保存しておく構造体
    /// </summary>
    public class MemberSipFunctionData
    {
	    #region インスタンスフィールド

	    private double sstart;		//Smallスタート位置
	    private double send;		//Small終了位置
	    private double lstart;		//Largeスタート位置
	    private double lend;		//Largeスタート位置

	    private double sdegree;		///<@brief Smallの時の傾き
	    private double sintercept;	///<@brief Smallの時の切片
 	    private double ldegree;		///<@brief Largeの時の傾き
	    private double lintercept;	///<@brief Largeの時の切片
	
        #endregion

	    #region 関数群

        /// <summary>
        /// コンストラクタ(初期化)
        /// </summary>
	    private MemberSipFunctionData()
	    {
            Console.WriteLine("new MeberSipFunctionData");
		    sstart = send = lstart = lend = 0;
		    sdegree = sintercept = ldegree = lintercept = 0.0;
	    }

        /// <summary>
        /// 生成メソッド
        /// </summary>
        /// <returns>初期化生成されたMeberSipFunctionData構造体</returns>
        public static MemberSipFunctionData Load()
        {
            MemberSipFunctionData msipd = new MemberSipFunctionData();
            return msipd;
        }

        /// <summary>
        /// メンバーシップ関数作成メソッド
        /// </summary>
        /// <param name="ss">Smallスタート位置</param>
        /// <param name="se">Small終了位置</param>
        /// <param name="ls">Largeスタート位置</param>
        /// <param name="le">Large終了位置</param>
	    public void makeMenberSipFunction(double ss,double se,double ls,double le)
	    {
		    sstart = ss;
		    send = se;
		    lstart = ls;
		    lend = le;

		    //式 
		    //(x1,y1) (x2,y2) から傾きを求める a=(y2-y1)/(x2-x1) 
		    // b = y1 - a * x1

		    //'Small'の時
		    sdegree =  -1.0 / (send - sstart);
		    sintercept = 1.0 - sdegree * sstart;		

		    //'Large'の時
		    ldegree = 1.0 / (lend - lstart);			
		    lintercept = -ldegree * lstart;	
	    }


        /// <summary>
        /// 適合度を計算し返す関数
        /// </summary>
        /// <param name="value">調べる値(包含率)</param>
        /// <param name="sl">'S'か'L'の文字列</param>
        /// <returns>適合度の値</returns>
	    public double fuzzyset(double value,char sl)
	    {	
		    double grade = 0.0;

		    //'Small'の時
		    if(sl=='S')
		    {
			    // sstart以下の時
			    if(sstart > value) grade = 1.0;

			    // sstart <= value <= send
			    else if(sstart <= value && send >= value) grade = (sdegree * value) + sintercept;

			    // send以上の時
			    else if(send < value) grade = 0.0;
		    }
	
		    //Large
		    else
		    {
			    // lstart以下の時
			    if(lstart > value) grade = 0.0;

			    // lstart <= value <= lend
			    else if(lstart <= value && lend >= value) grade = (ldegree * value) + lintercept;
			
			    // lend以上の時
			    else if(lend < value) grade = 1.0;
		    }
		    return grade;
	    }

	    #endregion
    }

    /// <summary>
    /// 1組のファジールールを表す構造体
    /// </summary>
    public class FuzzyRule
    {
        #region インスタンスフィールド

        public char deltaState;
        public char thetaState;
        public char alphaState;
        public char betaState;
        public char gammaState;
        public char output;
        public double noWeight;
        public double yesWeight;
        
        #endregion

        #region メソッド

        //コンストラクタ
	    private FuzzyRule()
	    {
		    deltaState = ' ';
		    thetaState = ' ';
		    alphaState = ' ';
		    betaState = ' ';
		    gammaState = ' ';
		    output = ' '; 
		    noWeight = yesWeight = 0;
        }

        public static FuzzyRule Load()
        {
            FuzzyRule rule = new FuzzyRule();
            return rule;
        }

        #endregion
    };

    #endregion

    /// <summary>
    /// ファジー解析クラス
    /// </summary>
    public class MyFuzzyAnalyze
    {
        #region インスタンスフィールド

        //確認用フォームに必要なオブジェク郡
        private Form confirmForm;                      //確認用のウィンドウフォーム
        private chartControl.Chart confirmChart;              //チャートオブジェクト
        private chartControl.ChartArea confirmChartArea;      //(グラフ領域)チャートエリアコレクションクラス
        private chartControl.Legend confirmLegend;            //(凡例)レジェンドコレクションクラス
        private chartControl.Series confirmSeries;            //シリーズコレクションクラス
        private chartControl.Title confirmtTitle;              //タイトルコレクションクラス
        private string confirmKey;                            //チャートのキー

     
        //結果表示用に必要なオブジェク郡
        private Form gofdisplayForm;
        private PictureBox resultimage;                   //Yes or Noの画像を表示するためのコントロール
        private chartControl.Chart gofChart;              //チャートオブジェクト
        private chartControl.ChartArea gofChartArea;      //(グラフ領域)チャートエリアコレクションクラス
        private chartControl.Legend gofLegend;            //(凡例)レジェンドコレクションクラス
        private chartControl.Series gofSeries;            //シリーズコレクションクラス
        private chartControl.Title gofTitle;              //タイトルコレクションクラス
        private string gofKey;                            //チャートのキー

        //ファジー解析の要素
        private static List<FuzzyRule> ruletable;               //ルールテーブルの配列
        private static List<MemberSipFunctionData> bandmsip;    //メンバーシップ関数の配列
        private MyFFTDataFunction fftdata;
        private const double neutralPoint = 0.6;


        //各数値計算の要素
        private double Ytgrade, Ntgrade;
        private double[] bandp;

        #endregion

        #region コンストラクタ&デストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private MyFuzzyAnalyze()
        {
            Console.WriteLine("new MyFuzzyAnalyze");
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~MyFuzzyAnalyze()
        {
            Console.WriteLine("delete MyFuzzyAnalyze");
        }
        
        #endregion

        #region 生成メソッド

        /// <summary>
        /// 生成メソッド
        /// </summary>
        /// <param name="wavedata">波データ</param>
        /// <returns>初期化生成したファジー解析クラス</returns>
        public static MyFuzzyAnalyze Load(double[] wavedata)
        {
            MyFuzzyAnalyze mfa = new MyFuzzyAnalyze();
            mfa.Initialize(wavedata);
            return mfa;
        }

        #endregion

        #region 初期化メソッド

        /// <summary>
        /// 要素初期化メソッド
        /// </summary>
        /// <param name="wavedata">脳波データ</param>
        private void Initialize(double[] wavedata)
        {         
            #region ファジー計算部
           
            this.FuzzyInitialize();     //ファジー解析要素生成&初期化
            fftdata = MyFFTDataFunction.Load(wavedata, wavedata.Length, WindowFunction.HAMMING);
            double Nwgrade = 0;		//No重み付き適合度
            double Ngrade = 0;		//No適合度
            double Ywgrade = 0;		//Yes重み付き適合度
            double Ygrade = 0;		//Yes適合度
            int rulecount = ruletable.Count;

            bandp = new double[5];
            bandp[0] = fftdata.getWaveProbability(WaveBandType.DELTA) * 100;
            bandp[1] = fftdata.getWaveProbability(WaveBandType.TEATA) * 100;
            bandp[2] = fftdata.getWaveProbability(WaveBandType.ALPHA) * 100;
            bandp[3] = fftdata.getWaveProbability(WaveBandType.BETA ) * 100;
            bandp[4] = fftdata.getWaveProbability(WaveBandType.GAMMA) * 100;

            //ルールテーブルによる適合度計算
            for (int kk = 0; kk < rulecount; kk++)
            {
                double min =double.MaxValue;

                for (int aa = 0; aa < 5; aa++)
                {
                    char state='\n';

                    switch (aa)
                    {
                        case 0: state = ruletable[kk].deltaState; break;
                        case 1: state = ruletable[kk].thetaState; break;
                        case 2: state = ruletable[kk].alphaState; break;
                        case 3: state = ruletable[kk].betaState;  break;
                        case 4: state = ruletable[kk].gammaState; break;
                        default: break;
                    }

                    double ppp = bandmsip[aa].fuzzyset(bandp[aa],state);
                    if (min >= ppp) min = ppp;
                }

                //Yes出力
                Ygrade += min;
                Ywgrade += (min * ruletable[kk].yesWeight);

                //No出力
                Ngrade += min;
                Nwgrade += (min * ruletable[kk].noWeight);
            }

            Ytgrade = (Ygrade == 0) ? 0 : Ywgrade / Ygrade;
            Ntgrade = (Ngrade == 0) ? 0 : Nwgrade / Ngrade;

            #endregion

            #region 確認用ディスプレイの設定 旧ソース

            confirmKey = "ConrirmKey";
            confirmForm = new Form();
            confirmForm.Text = "ConfirmFormWindow";
            confirmForm.MaximizeBox = false;
            confirmForm.Size = new System.Drawing.Size(640, 400);
            confirmForm.FormBorderStyle = FormBorderStyle.FixedSingle;
            confirmForm.StartPosition = FormStartPosition.Manual;
            confirmForm.Location = new System.Drawing.Point(200, 200);

            ConfirmInitializeComponent();
            confirmForm.Show();

            #endregion

            #region 結果表示ディスプレイの設定

            gofKey = "resultKey";
            gofdisplayForm = new Form();
            gofdisplayForm.Text = "ResultWindow";
            gofdisplayForm.MaximizeBox = false;
            gofdisplayForm.Size = new System.Drawing.Size(640, 400);
            gofdisplayForm.FormBorderStyle = FormBorderStyle.FixedSingle;
            gofdisplayForm.StartPosition = FormStartPosition.Manual;
            gofdisplayForm.Location = new System.Drawing.Point(200, 200);
            GOFDisplayInitializeComponent();
            gofdisplayForm.Show();


            #endregion
        }
   
        #endregion

        #region 確認用ディスプレイのオブジェクト初期化

        /// <summary>
        ///  確認用ディスプレイのオブジェクト初期化
        /// </summary>
        private void ConfirmInitializeComponent()
        {
            #region オブジェクト生成

            confirmChartArea = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            confirmLegend = new System.Windows.Forms.DataVisualization.Charting.Legend();
            confirmSeries = new System.Windows.Forms.DataVisualization.Charting.Series();
            confirmtTitle = new System.Windows.Forms.DataVisualization.Charting.Title();
            confirmChart = new System.Windows.Forms.DataVisualization.Charting.Chart();

            #endregion

            #region オブジェクト初期化

            ((System.ComponentModel.ISupportInitialize)(this.confirmChart)).BeginInit();
            confirmForm.SuspendLayout();

            confirmChart.Series.Clear();
            confirmChart.Series.Add(confirmKey);      //チャートシリーズにキーを登録

            confirmChart.Series[confirmKey].ChartType = chartControl.SeriesChartType.Column;      //棒グラフ

            chartControl.DataPoint[] point = new chartControl.DataPoint[7];
            for (int ii = 0; ii < point.Length; ii++) point[ii] = new chartControl.DataPoint();

            point[0].SetValueXY("δ帯域", bandp[0]);
            point[0].Color = System.Drawing.Color.Blue;
            point[1].SetValueXY("θ帯域", bandp[1]);
            point[1].Color = System.Drawing.Color.SlateBlue;
            point[2].SetValueXY("α帯域", bandp[2]);
            point[2].Color = System.Drawing.Color.Green;
            point[3].SetValueXY("β帯域", bandp[3]);
            point[3].Color = System.Drawing.Color.Red;
            point[4].SetValueXY("γ帯域", bandp[4]);
            point[4].Color = System.Drawing.Color.Violet;
            point[5].SetValueXY("Yes", Ytgrade *100);
            point[6].SetValueXY("No" , Ntgrade *100);

            for (int ii = 0; ii < point.Length;ii++ ) confirmChart.Series[confirmKey].Points.Add(point[ii]);

            confirmChartArea.AxisX.Title = "周波数帯域";
            confirmChartArea.AxisY.Title = "割合[%]";
            confirmChartArea.AxisY.TextOrientation = chartControl.TextOrientation.Horizontal;

            confirmChartArea.AxisY.Minimum = 0.0;
            confirmChartArea.AxisY.Maximum = 100.0;
            confirmChartArea.AxisY.Interval = 5.0;

            confirmChart.ChartAreas.Add(confirmChartArea);
            confirmChart.Left = confirmForm.Size.Width / 5;
            confirmChart.Width = (confirmForm.Width / 5 * 4) -20;
            confirmChart.Height = confirmForm.Size.Height - 30;
            confirmChart.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top);
            confirmForm.Controls.Add(confirmChart);

            ((System.ComponentModel.ISupportInitialize)(this.confirmChart)).EndInit();
            confirmForm.ResumeLayout(false);


            #endregion

            TextBox log = new TextBox();
            log.Multiline = true;           //これがないと高さを変更できない

            log.Width = confirmForm.Size.Width / 5;
            log.Dock = DockStyle.Left;

            confirmForm.Controls.Add(log);

            log.AppendText("FuzzyAnalyzeResult\n");
            log.AppendText("DeltaP:" + String.Format("{0:f3}", bandp[0]) + "%" + System.Environment.NewLine);
            log.AppendText("ThetaP:" + String.Format("{0:f3}", bandp[1]) + "%" + System.Environment.NewLine);
            log.AppendText("AlphaP:" + String.Format("{0:f3}", bandp[2]) + "%" + System.Environment.NewLine);
            log.AppendText("BetaP:"  + String.Format("{0:f3}", bandp[3]) + "%" + System.Environment.NewLine);
            log.AppendText("GammaP:" + String.Format("{0:f3}", bandp[4]) + "%" + System.Environment.NewLine);

            log.AppendText("YesGOF:" + String.Format("{0:f3}", Ytgrade * 100) + "%" + System.Environment.NewLine);
            log.AppendText("No GOF:" + String.Format("{0:f3}", Ntgrade * 100) + "%" + System.Environment.NewLine);
        }


        #endregion

        #region 表示ディスプレイのオブジェクト初期化

        /// <summary>
        /// 表示ディスプレイのオブジェクト初期化
        /// </summary>
        private void GOFDisplayInitializeComponent()
        {
            #region エクセルオブジェクト生成

            gofChartArea = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            gofLegend = new System.Windows.Forms.DataVisualization.Charting.Legend();
            gofSeries = new System.Windows.Forms.DataVisualization.Charting.Series();
            gofTitle = new System.Windows.Forms.DataVisualization.Charting.Title();
            gofChart = new System.Windows.Forms.DataVisualization.Charting.Chart();

            #endregion

            #region エクセルオブジェクト初期化

            ((System.ComponentModel.ISupportInitialize)(this.gofChart)).BeginInit();
            gofdisplayForm.SuspendLayout();

            gofChart.Series.Clear();
            gofChart.Series.Add(gofKey);      //チャートシリーズにキーを登録

            gofChart.Series[gofKey].ChartType = chartControl.SeriesChartType.Column;      //棒グラフ

            chartControl.DataPoint[] point = new chartControl.DataPoint[2];
            for (int ii = 0; ii < point.Length; ii++) point[ii] = new chartControl.DataPoint();

            point[0].SetValueXY("No", Ntgrade * 100);
            point[0].Color = System.Drawing.Color.Blue;
            point[1].SetValueXY("Yes",Ytgrade * 100);
            point[1].Color = System.Drawing.Color.Violet;

            for (int ii = 0; ii < point.Length; ii++) gofChart.Series[gofKey].Points.Add(point[ii]);

            gofChartArea.AxisX.Title = "出力";
            gofChartArea.AxisY.Title = "割合[%]";
            gofChartArea.AxisY.TextOrientation = chartControl.TextOrientation.Horizontal;

            gofChartArea.AxisY.Minimum = 0.0;
            gofChartArea.AxisY.Maximum = 100.0;
            gofChartArea.AxisY.Interval = 5.0;

            gofChart.ChartAreas.Add(gofChartArea);
            gofChart.Left = 0;
            gofChart.Width = (gofdisplayForm.Width / 2 );
            gofChart.Height = gofdisplayForm.Size.Height-30;
            gofChart.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top);
            gofdisplayForm.Controls.Add(gofChart);

            ((System.ComponentModel.ISupportInitialize)(this.gofChart)).EndInit();
            gofdisplayForm.ResumeLayout(false);

            #endregion

            #region 出力画像の表示

            string location = "";
            bool yorn = (Ntgrade <= Ytgrade) ? true : false;                 //true:YesBig false:Nobig 
            //bool bign = (Ntgrade >= 0.5 && Ntgrade <= 0.7) ? true : false;   //true:70%以上　false:50～70%未満
            //bool bigy = (Ytgrade >= 0.5 && Ytgrade <= 0.7) ? true : false;   //true:70%以上　false:50～70%未満

            //Yesの場合
            if (yorn == true)
            {
                if (Ytgrade >= 0.5 && Ytgrade < 0.7) location = "lowYes.jpg";
                else location = "Yes.jpg";
            }

            //Noの場合
            else
            {
                if (Ntgrade >= 0.5 && Ntgrade < 0.7) location = "lowNo.jpg";
                else location = "No.jpg";
            
            }

            resultimage = new PictureBox();
            resultimage.Left = (gofdisplayForm.Size.Width / 2);
            resultimage.Height = gofdisplayForm.Height;
            resultimage.Width = (gofdisplayForm.Size.Width / 2);
            resultimage.SizeMode = PictureBoxSizeMode.StretchImage;
            resultimage.ImageLocation = location;
            gofdisplayForm.Controls.Add(resultimage);

            #endregion
        }

        #endregion

        #region ファジー要素の初期化関数

        /// <summary>
        /// ファジー要素の初期化関数
        /// </summary>
        private void FuzzyInitialize()
        {
            #region ルールテーブルの生成

            //シングルトン
            if (ruletable == null)
            {
                ruletable = new List<FuzzyRule>();
                string buffer = "";
               
                //ファイルが読み込める時
                try
                {
                    using (StreamReader sr = new StreamReader("FuzzyRuletable.csv", Encoding.GetEncoding("Shift_JIS")))
                    {
                        while ((buffer = sr.ReadLine()) != null)
                        { 
                            FuzzyRule rule = FuzzyRule.Load();
                            //１行読み込み
                            string[] dataparam = buffer.Split(',');         //,で分解
                            rule.deltaState = char.Parse(dataparam[0]);
                            rule.thetaState = char.Parse(dataparam[1]);
                            rule.alphaState = char.Parse(dataparam[2]);
                            rule.betaState = char.Parse(dataparam[3]);
                            rule.gammaState = char.Parse(dataparam[4]);
                            rule.output = char.Parse(dataparam[5]);
                            rule.yesWeight = double.Parse(dataparam[6]);
                            rule.noWeight = double.Parse(dataparam[7]);
                            ruletable.Add(rule);
                        }
                    }
                }
                
                catch (Exception e)
                {
                    string tmp = "FileReadError";
                    MessageBox.Show("ファイルがありません", tmp, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            #endregion

            #region メンバーシップ関数の生成

            if (bandmsip == null)
            {
                string buffer="";
                
                bandmsip = new List<MemberSipFunctionData>();

                //ファイルが読み込める時
                try
                {
                    using (StreamReader sr = new StreamReader("BandMembeSipData.csv", Encoding.GetEncoding("Shift_JIS")))
                    {
                        while ((buffer = sr.ReadLine()) != null)
                        {
                            MemberSipFunctionData msipData = MemberSipFunctionData.Load();
                            //１行読み込み
                            string[] dataparam = buffer.Split(',');         //,で分解
                            double ss = double.Parse(dataparam[0]);
                            double se = double.Parse(dataparam[1]);
                            double ls = double.Parse(dataparam[2]);
                            double le = double.Parse(dataparam[3]);
                            msipData.makeMenberSipFunction(ss, se, ls, le);
                            bandmsip.Add(msipData);
                        }
                    }
                }

                catch (Exception e)
                {
                    string tmp = "FileReadError";
                    MessageBox.Show("ファイルがありません", tmp, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }


            #endregion
        }

        #endregion

        #region オブジェクト開放メソッド

        /// <summary>
        /// オブジェクトコンポーネント開放メソッド
        /// </summary>
        public void ReleaseComponent()
        {
            //confirmForm.Close();
            gofdisplayForm.Close();
            
            //確認用ディスプレイ開放
            if (confirmForm != null)     confirmForm.Close();
            if (confirmChart!=null)      confirmChart.Dispose();
            if (confirmChartArea!=null)  confirmChartArea.Dispose();
            if (confirmLegend!=null)     confirmLegend.Dispose();
            if (confirmSeries!=null)     confirmSeries.Dispose();
            if (confirmtTitle!=null)     confirmtTitle.Dispose();
            if (confirmForm != null)     confirmForm.Dispose();
           
            //結果表示用
            if (gofdisplayForm != null) gofdisplayForm.Close();
            if (gofChart != null)       gofChart.Dispose();
            if (gofChartArea != null)   gofChartArea.Dispose();
            if (gofLegend != null)      gofLegend.Dispose();
            if (gofSeries != null)      gofSeries.Dispose();
            if (gofTitle != null)       gofTitle.Dispose();
            if (gofdisplayForm != null) gofdisplayForm.Dispose();

            //画像コンポーネントの開放
            if (resultimage != null)
            {
                resultimage.Image.Dispose();
                resultimage.Dispose();
            }
            
        }


        #endregion

        #region データ出力メソッド

        /// <summary>
        /// <para>解析データをファイルに出力するメソッド</para>
        /// <para>出力データの順番[秒数,年齢,性別,実験の種類,delta確率,theta確率,alpha確率,beta確率,gamma確率,Yes確率,No確率,ファイル名]</para>
        /// </summary>
        /// <param name="fileName">ファイル名(パス有りでも可)</param>
        /// <param name="inputData">パラメータの配列 [0]:秒数 [1]:年齢　[2]:性別　[3]:実験の種類,[4]元のファイル名</param>>
        public void WriteAnalyzeData(string fileName, string[] inputData)
        {
            string outputName = fileName + ".csv";

            //ファイルの書き出し
            using (StreamWriter sw = new StreamWriter(outputName, true, Encoding.GetEncoding("Shift_JIS")))
            {
                string writedata = "";      //書き込む内容
                string inputDataset = inputData[0] + "," + inputData[1] + "," + inputData[2] + "," + inputData[3] +",";
                string fuzzyanalyze = "";
                for (int ii = 0; ii < bandp.Length; ii++) fuzzyanalyze += string.Format("{0:f2}", bandp[ii]) +",";
                fuzzyanalyze += (string.Format("{0:f2}", Ytgrade) + "," + string.Format("{0:f2}", Ntgrade));
                writedata = (inputDataset + fuzzyanalyze + "," + inputData[4]);
                sw.WriteLine(writedata);
            }

            string tt = fileName + ".csv" + "に書き込みました";
            MessageBox.Show(tt, "SuccessWriteData", MessageBoxButtons.OK, MessageBoxIcon.None);
        }


        #endregion

    }
}
