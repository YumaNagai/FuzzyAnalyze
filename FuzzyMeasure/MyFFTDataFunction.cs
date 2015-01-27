using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Windows.Forms;
using System.IO;

namespace FuzzyMeasure
{
    #region 解析のための窓関数列挙体 WindowFunction

    /// <summary>
    /// 解析のための窓関数列挙体
    /// </summary>
    public enum WindowFunction
    {
        /// <summary>
        /// 矩形窓
        /// </summary>
        RECTANGLE,

        /// <summary>
        /// ハニング窓
        /// </summary>
        HANNING,

        /// <summary>
        /// ハミング窓
        /// </summary>
        HAMMING,

        ///// <summary>
        ///// フラップトップ窓
        ///// </summary>
        //FLATTOP,

        ///// <summary>
        ///// 赤池窓
        ///// </summary>
        //AKAIKE,

        ///// <summary>
        ///// ランツォシュ窓
        ///// </summary>
        //LANCZOS,

        /// <summary>
        /// ブラックマン窓
        /// </summary>
        BLACKMAN,

        /// <summary>
        /// ガウシアン窓
        /// </summary>
        //GAUSS,

        /// <summary>
        /// 番兵
        /// </summary>
        MAX,
    }

    #endregion

    #region 解析範囲の列挙体 AnalyzeRange

    /// <summary>
    /// 解析範囲の列挙体
    /// </summary>
    public enum WaveBandType
    {
        /// <summary>
        /// 全体
        /// </summary>
        ALL,

        /// <summary>
        /// 0.1～100Hz
        /// </summary>
        BAND,

        /// <summary>
        /// δ帯域(0.1～3Hz)
        /// </summary>
        DELTA,

        /// <summary>
        /// θ帯域(4～7Hz)
        /// </summary>
        TEATA,

        /// <summary>
        /// α帯域(8～12Hz)
        /// </summary>
        ALPHA,

        /// <summary>
        /// β帯域(13～30Hz)
        /// </summary>
        BETA,

        /// <summary>
        /// γ帯域(30～100Hz)
        /// </summary>
        GAMMA,

        /// <summary>
        /// 番兵
        /// </summary>
        MAX,

    }


    #endregion

    #region FFTを行うクラス MyFFTDataFunction

    /// <summary>
    /// FFT、逆FFTを行い、必要な帯域を取り出すための機能を備えたクラス 
    /// </summary>
    public class MyFFTDataFunction
    {
        #region インスタンスフィールド

        private static Complex one = new Complex(1.0, 0.0);        //(1.0,0.0)
        private static Complex ione = new Complex(0.0, 1.0);      //(0.0,1.0)

        private List<double> window;            //窓関数

        private List<double> waveArray;           //変形前波形

        private List<double> windowWave;        //窓関数をかけた波形
        private List<Complex> fftArray;         //FFTした複素数配列
        private uint size;                      //fftArrayのサイズ
        private uint use;                       //与えられたサイズ
        private bool verbose;
        private double spectrumWidth;           //スペクトル幅のゲッター　

        #endregion

        #region コンストラクタ＆デストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private MyFFTDataFunction()
        {
            Console.WriteLine("new MySpectrumClass");
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~MyFFTDataFunction()
        {
            Console.WriteLine("delete MySpectrumClass");
        }


        #endregion

        #region 初期化メソッド＆生成メソッド

        /// <summary>
        /// 初期化メソッド
        /// </summary>
        /// <param name="wavedata">波形データ</param>
        /// <param name="size">サンプルした波形のサイズ</param>
        /// <param name="func">窓関数の種類</param>
        private void Initialize(double[] wavedata, int size, WindowFunction func)
        {
            use = (uint)size;
            this.size = NextPow2((uint)size);
            verbose = false;

            //スペクトル幅設定
            spectrumWidth = 512.0 / (double)this.size;

            //リスト生成
            waveArray = new List<double>(wavedata);
            fftArray = new List<Complex>((int)this.size);
            window = new List<double>();
            windowWave = new List<double>();
            WindowFunctionGenerate(func);           //窓関数をかけた波を生成
            FastFourierTransform(false);            //通常のFFT
        }

        /// <summary>
        /// 生成メソッド
        /// </summary>
        /// <param name="wavedata">波形データ</param>
        /// <param name="size">サンプルした波形のサイズ</param>
        /// <param name="func">窓関数の種類</param>
        /// <returns>初期化生成したMySpectrumClassクラスのアドレス</returns>
        public static MyFFTDataFunction Load(double[] wavedata, int size, WindowFunction func)
        {
            MyFFTDataFunction msc = new MyFFTDataFunction();        //生成
            msc.Initialize(wavedata, size, func);
            return msc;
        }


        #endregion

        #region 窓関数生成メソッド

        /// <summary>
        /// 窓関数生成メソッド
        /// </summary>
        /// <param name="function">窓関数の種類</param>
        /// <param name="sigma">ガウシアン窓パラメータσ</param>
        public void WindowFunctionGenerate(WindowFunction function, double sigma = 0.0)
        {
            //int size = waveArray.Count;
            window.Clear();
            windowWave.Clear();

            for (int ii = 0; ii < this.size; ii++)
            {
                double tt = 0;

                switch (function)
                {
                    #region 矩形窓

                    case WindowFunction.RECTANGLE:
                    {
                        window.Add(1.0);

                        if (ii < waveArray.Count)
                        {
                            windowWave.Add(window[ii] * waveArray[ii]);
                        }

                        else
                        {
                            windowWave.Add(window[ii] * 0);
                        }

                    } break;

                    #endregion

                    #region ハニング窓

                    case WindowFunction.HANNING:
                    {
                        tt = 0.5 - 0.5 * Math.Cos((2 * Math.PI * ii) / (double)(size - 1));

                        window.Add(tt);

                        if (ii < waveArray.Count)
                        {
                            windowWave.Add(window[ii] * waveArray[ii]);
                        }

                        else
                        {
                            windowWave.Add(window[ii] * 0);
                        }

                    } break;


                    #endregion

                    #region ハミング窓

                    case WindowFunction.HAMMING:
                    {
                        tt = 0.54 - 0.46 * Math.Cos((2 * Math.PI * ii) / (double)(size - 1));
                        window.Add(tt);

                        if (ii < waveArray.Count)
                        {
                            windowWave.Add(window[ii] * waveArray[ii]);
                        }

                        else
                        {
                            windowWave.Add(window[ii] * 0);
                        }

                    } break;



                    #endregion

                    #region ブラックマン窓

                    case WindowFunction.BLACKMAN:
                    {
                        tt = 0.42 - 0.5 * Math.Cos((2 * Math.PI * ii) / (double)(size - 1)) + 0.08 * Math.Cos((4 * Math.PI * ii) / (double)(size - 1));
                        window.Add(tt);

                        if (ii < waveArray.Count)
                        {
                            windowWave.Add(window[ii] * waveArray[ii]);
                        }

                        else
                        {
                            windowWave.Add(window[ii] * 0);
                        }

                    } break;

                    #endregion

                    #region ガウシアン窓

                    //case WindowFunction.GAUSS:
                    //{
                    //    tt = Math.Exp(-4.5 * (2.0 * ii - (size - 1)) / (size - 1) * (2.0 * ii - (size - 1) / (size - 1)));
                    //    window.Add(tt);
                    //    windowWave.Add(window[ii] * waveArray[ii]);

                    //} break;

                    #endregion

                    #region 旧ソース

                    //#region フラットトップ窓

                    //case WindowFunction.FLATTOP:
                    //    {


                    //    } break;


                    //#endregion

                    //#region 赤池窓

                    //case WindowFunction.AKAIKE: 
                    //{ 


                    //} break;


                    //#endregion

                    //#region ランツォシュ窓

                    //case WindowFunction.LANCZOS: 
                    //{ 

                    //}break;

                    //#endregion

                    #endregion
                }
            }

            //窓関数をかけたデータが生成されたらデータから複素数配列を作る
            GenerateComplexToWave();
        }

        /// <summary>
        /// データから複素数の配列を生成するメソッド
        /// </summary>
        private void GenerateComplexToWave()
        {
            fftArray.Clear();

            for (int ii = 0; ii < this.size; ii++)
            {
                Complex tmp;
                if(ii < windowWave.Count) tmp = new Complex(windowWave[ii], 0.0);
                else tmp = Complex.Zero;
           
                fftArray.Add(tmp);
            }

        }

        #endregion

        #region メソッド郡

        /// <summary>
        /// 要素を入れ替えるメソッド
        /// </summary>
        /// <param name="ii">入れ替えるアドレスのインデクッス1</param>
        /// <param name="jj">入れ替えるアドレスのインデクッス2</param>
        private void Listswap(int ii, int jj)
        {
            Complex tmp = fftArray[ii];
            fftArray[ii] = fftArray[jj];
            fftArray[jj] = tmp;
        }

        /// <summary>
        /// s以上の最小の2のべき乗を返す
        /// </summary>
        /// <param name="s">数</param>
        /// <returns>s以上の最小の2のべき乗</returns>
        private uint NextPow2(uint s)
        {
            uint n = 1;
            while (n < s) n <<= 1;      //左に1ビットシフト(2倍)
            return n;
        }

        /// <summary>
        /// ビット反転
        /// </summary>
        private void BitReverse()
        {
            uint k, b, a;

            for (int ii = 0; ii < size; ii++)
            {
                k = 0;
                b = size >> 1;
                a = 1;

                while (b >= a)
                {
                    if ((b & ii) != 0) k |= a;
                    if ((a & ii) != 0) k |= b;
                    b >>= 1;                    //1/2倍(右に1ビットシフト)
                    a <<= 1;                    //2倍　(左に1ビットシフト)
                }
                //要素入れ替え
                if (ii < k) Listswap(ii, (int)k);
            }
        }

        /// <summary>
        /// 情報ダンプ
        /// </summary>
        private void Dump()
        {
            uint end = verbose ? size : use;

            for (uint ii = 0; ii < end; ii++)
            {
                string tt = string.Format("Real {0:F}  Imaginary{1:F}", fftArray[(int)ii].Real, fftArray[(int)ii].Imaginary);
                Console.WriteLine(tt);
            }
        }


        #endregion

        #region ゲッターメソッド

        /// <summary>
        /// スペクトル幅の取得メソッド
        /// </summary>
        /// <returns>スペクトル幅</returns>
        public double GetSpectrumWidth() { return spectrumWidth; }

        #endregion

        #region 高速フーリエ変換＆逆高速フーリエ変換

        /// <summary>
        /// 高速フーリエ変換
        /// </summary>
        /// <param name="isReverse">反転させるかのフラグ</param>
        public void FastFourierTransform(bool isReverse = false)
        {
            BitReverse();
            uint m = 2;
            Complex w, ww, t;

            while (m <= size)
            {
                double arg = -2.0 * Math.PI / m;
                w = new Complex(Math.Cos(arg), Math.Sin(arg));

                //-1乗 -(-2.0*PI/size) = 2.0*PI/size
                if (isReverse) w /= one;

                for (uint ii = 0; ii < size; ii += m)
                {
                    ww = 1.0;

                    for (uint jj = 0; jj < m / 2; jj++)
                    {
                        int a = (int)(ii + jj);
                        int b = (int)(ii + jj + m / 2);

                        t = ww * fftArray[b];

                        fftArray[b] = fftArray[a] - t;
                        fftArray[a] = fftArray[a] + t;

                        ww *= w;
                    }
                }
                m *= 2;
            }
        }

        //逆高速フーリエ変換
        private void InverseFastFourierTransform()
        {
            FastFourierTransform(true);
            float s = (float)size;
            for (int ii = 0; ii < size; ii++) fftArray[ii] /= s;
        }

        #endregion

        #region 各振幅・パワースペクトルを得るための関数 旧ソースコード

        //#region δ帯域

        ///// <summary>
        ///// δ帯域(0.1～3Hz)の振幅スペクトルの取得
        ///// </summary>
        ///// <returns>デルタ帯域の振幅スペクトル値</returns>
        //public double GetDeltaAmplitudeSpectrum()
        //{
        //    double delta = 0;

        //    for (int ii = (int)(0.1 / spectrumWidth); ii <= (3.0 / spectrumWidth); ii++)
        //    {
        //        delta += Complex.Abs(fftArray[ii]);
        //    }
        //    return delta;
        //}

        ///// <summary>
        ///// δ帯域(0.1～3Hz)のパワースペクトルの取得
        ///// </summary>
        ///// <returns>デルタ帯域のパワースペクトル値</returns>
        //public double GetDeltaPowerSpectrum()
        //{
        //    double delta = 0;

        //    for (int ii = (int)(0.1 / spectrumWidth); ii <= (3.0 / spectrumWidth); ii++)
        //    {
        //        delta += (Complex.Abs(fftArray[ii]) * Complex.Abs(fftArray[ii]));
        //    }
        //    return delta;
        //}

        //#endregion

        //#region θ帯域

        ///// <summary>
        ///// θ帯域(4～7Hz)の振幅スペクトルの取得
        ///// </summary>
        ///// <returns>θ帯域の振幅スペクトル値</returns>
        //public double GetThetaAmplitudeSpectrum()
        //{
        //    double theta = 0;

        //    for (int ii = (int)(4.0 / spectrumWidth); ii <= (int)(7.0 / spectrumWidth); ii++)
        //    {
        //        theta += Complex.Abs(fftArray[ii]);
        //    }
        //    return theta;
        //}

        ///// <summary>
        ///// θ帯域(4～7Hz)のパワースペクトルの取得
        ///// </summary>
        ///// <returns>θ帯域のパワースペクトル値</returns>
        //public double GetThetaPowerSpectrum()
        //{
        //    double theta = 0;

        //    for (int ii = (int)(4.0 / spectrumWidth); ii <= (int)(7.0 / spectrumWidth); ii++)
        //    {
        //        theta += (Complex.Abs(fftArray[ii]) * Complex.Abs(fftArray[ii]));
        //    }
        //    return theta;
        //}

        //#endregion

        //#region α帯域

        ///// <summary>
        ///// α帯域(8～12Hz)の振幅スペクトルの取得
        ///// </summary>
        ///// <returns>α帯域の振幅スペクトル値</returns>
        //public double GetAlphaAmplitudeSpectrum()
        //{
        //    double alpha = 0;

        //    for (int ii = (int)(8.0 / spectrumWidth); ii <= (int)(12.0 / spectrumWidth); ii++)
        //    {
        //        alpha += Complex.Abs(fftArray[ii]);
        //    }
        //    return alpha;
        //}

        ///// <summary>
        ///// α帯域(8～12Hz)のパワースペクトルの取得
        ///// </summary>
        ///// <returns>α帯域のパワースペクトル値</returns>
        //public double GetAlphaPowerSpectrum()
        //{
        //    double alpha = 0;

        //    for (int ii = (int)(8.0 / spectrumWidth); ii <= (int)(12.0 / spectrumWidth); ii++)
        //    {
        //        alpha += (Complex.Abs(fftArray[ii]) * Complex.Abs(fftArray[ii]));
        //    }
        //    return alpha;
        //}

        //#endregion

        //#region β帯域

        ///// <summary>
        ///// β帯域(13～30Hz)の振幅スペクトルの取得
        ///// </summary>
        ///// <returns>β帯域の振幅スペクトル値</returns>
        //public double GetBetaAmplitudeSpectrum()
        //{
        //    double beta = 0;

        //    for (int ii = (int)(13.0 / spectrumWidth); ii <= (int)(30.0 / spectrumWidth); ii++)
        //    {
        //        beta += Complex.Abs(fftArray[ii]);
        //    }
        //    return beta;
        //}

        ///// <summary>
        ///// β帯域(13～30Hz)のパワースペクトルの取得
        ///// </summary>
        ///// <returns>β帯域のパワースペクトル値</returns>
        //public double GetBetaPowerSpectrum()
        //{
        //    double beta = 0;

        //    for (int ii = (int)(13.0 / spectrumWidth); ii <= (int)(30.0 / spectrumWidth); ii++)
        //    {
        //        beta += (Complex.Abs(fftArray[ii]) * Complex.Abs(fftArray[ii]));
        //    }
        //    return beta;
        //}


        //#endregion

        //#region γ帯域

        ///// <summary>
        ///// γ帯域(30～100Hz)の振幅スペクトルの取得
        ///// </summary>
        ///// <returns>γ帯域の振幅スペクトル値</returns>
        //public double GetGammaAmplitudeSpectrum()
        //{
        //    double gamma = 0;

        //    for (int ii = (int)(30.0 / spectrumWidth); ii <= (int)(100.0 / spectrumWidth); ii++)
        //    {
        //        gamma += Complex.Abs(fftArray[ii]);
        //    }
        //    return gamma;
        //}

        ///// <summary>
        ///// γ帯域(30～100Hz)のパワースペクトルの取得
        ///// </summary>
        ///// <returns>γ帯域のパワースペクトル値</returns>
        //public double GetGammaPowerSpectrum()
        //{
        //    double gamma = 0;

        //    for (int ii = (int)(30.0 / spectrumWidth); ii <= (int)(100.0 / spectrumWidth); ii++)
        //    {
        //        gamma += (Complex.Abs(fftArray[ii]) * Complex.Abs(fftArray[ii]));
        //    }
        //    return gamma;
        //}

        //#endregion

        //#endregion

        //#region 各スペクトルバンドを得るための関数

        ///// <summary>
        ///// スペクトルの振幅バンドを得るためのメソッド
        ///// </summary>
        ///// <param name="range">解析レンジの列挙体</param>
        ///// <returns>範囲で取り出した振幅スペクトルの配列</returns>
        //public List<double> GetSpectrumAmplitudeBand(AnalyzeRange range)
        //{
        //    List<double> band = new List<double>();

        //    //解析レンジにしたがって得る範囲の振幅スペクトルリストを抜き出す
        //    switch (range)
        //    {
        //        #region δ帯域

        //        case AnalyzeRange.DELTA:
        //            {
        //                for (int ii = (int)(0.1 / spectrumWidth); ii <= (3.0 / spectrumWidth); ii++)
        //                {
        //                    band.Add(Complex.Abs(fftArray[ii]));
        //                }

        //            } break;

        //        #endregion

        //        #region θ帯域

        //        case AnalyzeRange.TEATA:
        //            {
        //                for (int ii = (int)(4.0 / spectrumWidth); ii <= (int)(7.0 / spectrumWidth); ii++)
        //                {
        //                    band.Add(Complex.Abs(fftArray[ii]));
        //                }

        //            } break;

        //        #endregion

        //        #region α帯域

        //        case AnalyzeRange.ALPHA:
        //            {
        //                for (int ii = (int)(8.0 / spectrumWidth); ii <= (int)(12.0 / spectrumWidth); ii++)
        //                {
        //                    band.Add(Complex.Abs(fftArray[ii]));
        //                }
        //            } break;

        //        #endregion

        //        #region β帯域

        //        case AnalyzeRange.BETA:
        //            {
        //                for (int ii = (int)(13.0 / spectrumWidth); ii <= (int)(30.0 / spectrumWidth); ii++)
        //                {
        //                    band.Add(Complex.Abs(fftArray[ii]));
        //                }

        //            } break;

        //        #endregion

        //        #region γ帯域

        //        case AnalyzeRange.GAMMA:
        //            {
        //                for (int ii = (int)(30.0 / spectrumWidth); ii <= (int)(100.0 / spectrumWidth); ii++)
        //                {
        //                    band.Add(Complex.Abs(fftArray[ii]));
        //                }

        //            } break;

        //        #endregion

        //        #region 0.1～100Hz

        //        case AnalyzeRange.BAND:
        //            {
        //                for (int ii = (int)(0.1 / spectrumWidth); ii <= (int)(100.0 / spectrumWidth); ii++)
        //                {
        //                    band.Add(Complex.Abs(fftArray[ii]));
        //                }

        //            } break;

        //        #endregion

        //        #region AllBand

        //        case AnalyzeRange.ALL:
        //            {
        //                for (int ii = (int)(0.1 / spectrumWidth); ii <= (int)(256.0 / spectrumWidth); ii++)
        //                {
        //                    band.Add(Complex.Abs(fftArray[ii]));
        //                }
        //            } break;

        //        #endregion
        //    }

        //    return band;
        //}


        ///// <summary>
        ///// スペクトルのパワーバンドを得るためのメソッド
        ///// </summary>
        ///// <param name="range">解析レンジの列挙体</param>
        ///// <returns>範囲で取り出した振幅スペクトルの配列</returns>
        //public List<double> GetSpectrumPowerBand(AnalyzeRange range)
        //{
        //    List<double> band = new List<double>();

        //    //解析レンジにしたがって得る範囲のパワースペクトルリストを抜き出す
        //    switch (range)
        //    {
        //        #region δ帯域

        //        case AnalyzeRange.DELTA:
        //            {
        //                for (int ii = (int)(0.1 / spectrumWidth); ii <= (3.0 / spectrumWidth); ii++)
        //                {
        //                    band.Add(Complex.Abs(fftArray[ii]) * Complex.Abs(fftArray[ii]));
        //                }

        //            } break;

        //        #endregion

        //        #region θ帯域

        //        case AnalyzeRange.TEATA:
        //            {
        //                for (int ii = (int)(4.0 / spectrumWidth); ii <= (int)(7.0 / spectrumWidth); ii++)
        //                {
        //                    band.Add(Complex.Abs(fftArray[ii]) * Complex.Abs(fftArray[ii]));
        //                }

        //            } break;

        //        #endregion

        //        #region α帯域

        //        case AnalyzeRange.ALPHA:
        //            {
        //                for (int ii = (int)(8.0 / spectrumWidth); ii <= (int)(12.0 / spectrumWidth); ii++)
        //                {
        //                    band.Add(Complex.Abs(fftArray[ii]) * Complex.Abs(fftArray[ii]));
        //                }
        //            } break;

        //        #endregion

        //        #region β帯域

        //        case AnalyzeRange.BETA:
        //            {
        //                for (int ii = (int)(13.0 / spectrumWidth); ii <= (int)(30.0 / spectrumWidth); ii++)
        //                {
        //                    band.Add(Complex.Abs(fftArray[ii]) * Complex.Abs(fftArray[ii]));
        //                }

        //            } break;

        //        #endregion

        //        #region γ帯域

        //        case AnalyzeRange.GAMMA:
        //            {
        //                for (int ii = (int)(30.0 / spectrumWidth); ii <= (int)(100.0 / spectrumWidth); ii++)
        //                {
        //                    band.Add(Complex.Abs(fftArray[ii]) * Complex.Abs(fftArray[ii]));
        //                }

        //            } break;

        //        #endregion

        //        #region 0.1～100Hz

        //        case AnalyzeRange.BAND:
        //            {
        //                for (int ii = (int)(0.1 / spectrumWidth); ii <= (int)(100.0 / spectrumWidth); ii++)
        //                {
        //                    band.Add(Complex.Abs(fftArray[ii]) * Complex.Abs(fftArray[ii]));
        //                }

        //            } break;

        //        #endregion

        //        #region AllBand

        //        case AnalyzeRange.ALL:
        //            {
        //                for (int ii = (int)(0.1 / spectrumWidth); ii <= (int)(256.0 / spectrumWidth); ii++)
        //                {
        //                    band.Add(Complex.Abs(fftArray[ii]) * Complex.Abs(fftArray[ii]));
        //                }
        //            } break;

        //        #endregion
        //    }

        //    return band;
        //}


        #endregion

        #region 開放メソッド

        /// <summary>
        /// リソース開放
        /// </summary>
        public void AllClear()
        {
            window.Clear();
            windowWave.Clear();
            waveArray.Clear();
            fftArray.Clear();
        }

        #endregion

        #region 確率取得メソッド

        /// <summary>
        /// 各帯域の包含率を取得するメソッド(0.1～100Hz)
        /// </summary>
        /// <param name="wbtype">帯域の種類</param>
        /// <returns>引数で選んだ帯域の包含率</returns>
        public double getWaveProbability(WaveBandType wbtype)
        {
            double minmamband = 0.25;
            double bandpower = 0;
            double sum = 0;
            double power = 0;
            int ii = 1;                 //0が直流成分

            //分解能が足りない場合の補正
            if(spectrumWidth > minmamband)
            {
                ii = 1; //直流を含まない最初のインデクス
            }

            //分解能が足りている場合
            else
            {
                double dd = spectrumWidth;

                //分解能が足りる場合該当インデクスまで進める
                while(minmamband >= dd)
                {
                    dd +=  spectrumWidth;
                    ii++;
                }
            }

            for (; ii < (int)(100.0 / spectrumWidth); ii++)
            {
                power = (Complex.Abs(fftArray[ii]) * Complex.Abs(fftArray[ii]));

                switch (wbtype)
                {
                    //δ波
                    case WaveBandType.DELTA:
                    {
                        //bool bandmin = (ii >  (int)(minmamband / spectrumWidth));
                        bool bandmax = (ii <= (int)(3.0 / spectrumWidth));

                        if (bandmax == true) bandpower += power;

                    } break;

                    //θ波
                    case WaveBandType.TEATA:
                    {
                        bool bandmin = (ii > (int)(4.0 / spectrumWidth));
                        bool bandmax = (ii <= (int)(7.0 / spectrumWidth));

                        if (bandmin == true && bandmax == true) bandpower += power;
                    } break;

                    //α波
                    case WaveBandType.ALPHA:
                    {
                        bool bandmin = (ii > (int)(8.0 / spectrumWidth));
                        bool bandmax = (ii <= (int)(12.0 / spectrumWidth));

                        if (bandmin == true && bandmax == true) bandpower += power;

                    } break;

                    //β波
                    case WaveBandType.BETA:
                    {
                        bool bandmin = (ii > (int)(13.0 / spectrumWidth));
                        bool bandmax = (ii <= (int)(30.0 / spectrumWidth));

                        if (bandmin == true && bandmax == true) bandpower += power;

                    } break;

                    //γ波
                    case WaveBandType.GAMMA:
                    {
                        bool bandmin = (ii > (int)(30.0 / spectrumWidth));
                        bool bandmax = (ii <= (int)(100.0 / spectrumWidth));

                        if (bandmin == true && bandmax == true) bandpower += power;

                    } break;
                }

                /*NeuroSky周波数定義では3.0～4.0となど周波数間が空く場合が
                あるためその分の周波数配列要素に関しては加算を除外する*/
                //δ-θ間
                bool aa = (ii > (int)(3.0 / spectrumWidth));
                bool bb = (ii <= (int)(4.0 / spectrumWidth));

                //θ-α間
                bool cc = (ii > (int)(7.0 / spectrumWidth));
                bool dd = (ii <= (int)(8.0 / spectrumWidth));

                //α-β間
                bool ee = (ii > (int)(12.0 / spectrumWidth));
                bool ff = (ii <= (int)(13.0 / spectrumWidth));

                //空文実行
                if ((aa && bb) || (cc && dd) || (ee && ff))
                {
                    ;
                }

                //その他
                else
                {
                    sum += power;
                }
            }

            return (bandpower/sum);
        }

        #endregion
    }

    #endregion

}
