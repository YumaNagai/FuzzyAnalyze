using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

//ExcelApplicationとFormを動かすためのstaticクラスのApplicationクラスの名前が衝突
//するためあえてこのように記述する
using Excel = Microsoft.Office.Interop.Excel;
using chartControl = System.Windows.Forms.DataVisualization.Charting;

namespace FuzzyMeasure
{
    /// <summary>
    /// 同期ファイルと波データを重ねて表示するフォーム
    /// </summary>
    public class MyTimingSynchroForm : Form
    {
        #region インスタンスフィールド

        private chartControl.Chart chart;           //チャートオブジェクト
        private chartControl.ChartArea chartArea;   //(グラフ領域)チャートエリアコレクションクラス
        private chartControl.Legend legend;         //(凡例)レジェンドコレクションクラス
        private chartControl.Series series;         //シリーズコレクションクラス
        private chartControl.Title title;           //タイトルコレクションクラス
        private string wavekey;                     //チャートのキー

        private int prenumber;                      //前に選んだインデクッス


        #endregion

        #region コンストラクタ＆デストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private MyTimingSynchroForm()
        {
            Console.WriteLine("new MyTimingSynchroForm");
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~MyTimingSynchroForm() 
        {
            Console.WriteLine("delete MyTimingSynchroForm");
        }

        #endregion

        #region 生成メソッド

        /// <summary>
        /// 初期化メソッド
        /// </summary>
        /// <param name="wavedata">計測した脳波データ</param>
        /// <param name="synchroDataSet">同期データセット</param>
        public static MyTimingSynchroForm Load(List<double> wavedata, List<MySynchroDataSet> synchroDataSet)
        {
            MyTimingSynchroForm mtsf = new MyTimingSynchroForm();
            mtsf.Initialize(wavedata,synchroDataSet);
            return mtsf;
        }

        #endregion

        #region 初期化メソッド

        /// <summary>
        /// 初期化メソッド
        /// </summary>
        /// <param name="wavedata">計測した脳波データ</param>
        /// <param name="synchroDataSet">同期データセット</param>
        private void Initialize(List<double> wavedata,List<MySynchroDataSet> synchroDataSet)
        {
            this.Size = new System.Drawing.Size(640, 400);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new System.Drawing.Point(200, 200);
            this.Text = "TimingconfirmForm";

            this.prenumber = 0;             //初期化

            #region エクセルオブジェクト生成

            wavekey = "wave";
            chart       = new System.Windows.Forms.DataVisualization.Charting.Chart();
            chartArea   = new System.Windows.Forms.DataVisualization.Charting.ChartArea(); 
            legend      = new System.Windows.Forms.DataVisualization.Charting.Legend();
            series      = new System.Windows.Forms.DataVisualization.Charting.Series();       
            title       = new System.Windows.Forms.DataVisualization.Charting.Title();

            #endregion

            #region エクセルオブジェクト初期化

            ((System.ComponentModel.ISupportInitialize)(this.chart)).BeginInit();
            this.SuspendLayout();

            chart.Series.Clear();
            chart.Series.Add(wavekey);
    
            chart.Series[wavekey].ChartType = chartControl.SeriesChartType.Line;
            chartArea.AxisX.Title = "秒数[s]    (Noデータ:青　Yesデータ:赤)\n";
            chartArea.AxisY.Title = "脳波の離散値";
            //chartArea.AxisY.TextOrientation = chartControl.TextOrientation.Horizontal;

            double second = 0;
            int count = wavedata.Count;
            int jj = 0;
            MySynchroDataSet tmp = synchroDataSet[0];
            for (int ii = 0; ii < count; ii++)
            {
                second += (1.0 / 512.0);

                chartControl.DataPoint pointData = new System.Windows.Forms.DataVisualization.Charting.DataPoint();
                pointData.SetValueXY(second, wavedata[ii]);

                if ((tmp.startindex <= ii && tmp.lastindex >= ii))
                {
                    //Yesならば赤色
                    if (string.Equals(tmp.type, "Yes")) pointData.Color = System.Drawing.Color.Red;

                    //Noならば青色
                    else if (string.Equals(tmp.type, "No")) pointData.Color = System.Drawing.Color.Blue;
                    
                    if (tmp.lastindex == ii && jj < synchroDataSet.Count-1)
                    {
                        tmp = synchroDataSet[++jj];
                    }
                }

                else
                {
                    pointData.Color = System.Drawing.Color.Blue;
                }
               
                chart.Series[wavekey].Points.Add(pointData);

            }

            chartArea.AxisX.Interval = 5;       //5s間隔

            chart.ChartAreas.Add(chartArea);
            this.chart.Size = new System.Drawing.Size(this.Width - 10, this.Height - 20);
            chart.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top);

            this.Controls.Add(this.chart);
            this.Name = "SynchroData";

            ((System.ComponentModel.ISupportInitialize)(this.chart)).EndInit();
            this.ResumeLayout(false);

            #endregion

            this.Show();

        }

        #endregion

        #region コンポーネント開放メソッド

        /// <summary>
        /// オブジェクト開放メソッド
        /// </summary>
        public void ReleaseComponent()
        {
            if (chart != null)      chart.Dispose();
            if (chartArea != null)  chartArea.Dispose();
            if (legend != null)     legend.Dispose();
            if (series != null)     series.Dispose();
            if (title != null)      title.Dispose();
        }

        #endregion

        #region 現在選択している場所を表示

        /// <summary>
        /// アップダウンコントロールの現在の数値を読み取りラベルとして表示するメソッド
        /// </summary>
        /// <param name="index">現在アップダウンが指している数値</param>
        /// <param name="inf">情報(好きな情報を付加し表示)</param>
        public void displayCurrentInformation(int index,string inf)
        {
            //前の時の表示を残さないようにする
            if (prenumber != index)
            {
                chart.Series[wavekey].Points[prenumber].SetDefault(true);
            }
            
            string dispaly = string.Format("(水色)現在選択中の番号:{0:d}", index) + " " + inf;
            chartArea.AxisX.Title = "秒数[s]    (Noデータ:青　Yesデータ:赤)\n\n";
            chartArea.AxisX.Title += dispaly;
            chart.Series[wavekey].Points[index].MarkerStyle = chartControl.MarkerStyle.Circle;
            chart.Series[wavekey].Points[index].MarkerSize = 10;
            chart.Series[wavekey].Points[index].MarkerColor = Color.Aqua;
            prenumber = index;
            
        }

        #endregion
         
        #region 固定点解析モード時の解析点表示

        /// <summary>
        /// 固定点解析モード時の解析点表示
        /// </summary>
        /// <param name="yessIndex">Yes開始点</param>
        /// <param name="nosIndex">No開始点</param>
        public void defaultInformation(List<int> yessIndex,List<int> nosIndex)
        {
            for(int ii=0;ii<yessIndex.Count;ii++)
            {
                //Yes
                int tmp = yessIndex[ii];
                chart.Series[wavekey].Points[tmp].MarkerStyle = chartControl.MarkerStyle.Circle;
                chart.Series[wavekey].Points[tmp].MarkerSize = 10;
                chart.Series[wavekey].Points[tmp].MarkerColor = Color.Aqua;
            }

            for(int jj=0;jj<nosIndex.Count;jj++)
            {
                //Yes
                int tmp = nosIndex[jj];
                chart.Series[wavekey].Points[tmp].MarkerStyle = chartControl.MarkerStyle.Circle;
                chart.Series[wavekey].Points[tmp].MarkerSize = 10;
                chart.Series[wavekey].Points[tmp].MarkerColor = Color.Green;
            }
        }

        #endregion

    }
}
