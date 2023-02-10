using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
       
        /// <summary>
        /// 確認字串是否有X
        /// </summary>
        const string x1 = "X";

        /// <summary>
        /// 確認字串是否有Y
        /// </summary>
        const string y1 = "Y";

        /// <summary>
        /// 確認字串是否有Z
        /// </summary>
        const string z1 = "Z";
        
        /// <summary>
        /// 取得檔案路徑(包含檔名)
        /// </summary>
        string TargetFilePathName = "";

        /// <summary>
        /// 取得檔案名稱
        /// </summary>
        string TargetFileName = "";

        /// <summary>
        /// 取得檔案路徑
        /// </summary>
        string TargetFilePath = "";
        
        public Form1()
        {
            InitializeComponent();

            //開啟程式時，選擇檔案
            GetTargetFilePathName();
        }

        /// <summary>
        /// 取得檔案路徑和檔案名稱
        /// </summary>
        public void GetTargetFilePathName()
        {
            OpenFileDialog file = new OpenFileDialog();

            file.ShowDialog();

            //取得檔案路徑(包含檔名)
            TargetFilePathName = Path.GetFullPath(file.FileName);

            //取得檔案名稱
            TargetFileName = Path.GetFileName(file.FileName);

            //取得檔案路徑
            TargetFilePath = Path.GetDirectoryName(file.FileName);
        }

        /// <summary>
        /// 讀取檔案，進行整理
        /// </summary>
        /// <param name="TargetFilePathName">檔案名稱</param>
        /// <returns>轉換完成的檔案內容</returns>
        public ArrayList ConversionOperation(String TargetFilePathName)
        {
            //轉換完成的檔案內容
            ArrayList newFileArray = new ArrayList();

            //讀取此路徑檔案的內容
            StreamReader str = new StreamReader(TargetFilePathName);

            string readFileLine;
            
            //讀取第一列，使 while 迴圈能開始運作
            readFileLine = str.ReadLine();

            //Rot是被旋轉點座標XYZ
            string originValueX = "0.0", originValueY = "0.0", originValueZ = "0.0";

            //此字串不須加Z
            //string G90G55G0str = "G90G55G0";

            //檔案中存放檔名的那行字串，當中的XYZ不需做轉換，其數值也不會被傳承
            string colon1000str = ":1000";
            
            //不須轉換的字串
            string G91G28str = "G91G28";

            //不須轉換的字串
            string G0Zstr = "G0Z";

            //不須轉換的字串
            //string G43str = "G43";

            //不須轉換的字串
            //string G53str = "G53";
            
            //防呆警報 
            string G2str = "G2(?=X|Y)";

            //防呆警報
            string G3str = "G3(?=X|Y)";

            //防呆警報  
            string G02str = "G02";

            //防呆警報        
            string G03str = "G03";

            ///防呆警報 
            string G68str = "G68";

            ///防呆警報 
            string G69str = "G69";


            //一列一列看整個文件，直到那一列是空直值
            while (readFileLine != null)
            {
                //====================================================================================================================================
 
                //把成功放入XYZ旋轉點公式的陣列重新整合成一個字串
                string splicingAll = "";

                //紀錄取得的那列字串中，哪個位置要改成有旋轉公式的點
                ArrayList arrayReadFileLine = new ArrayList();

                //將取得的字串轉換成char陣列
                char[] charArray = readFileLine.ToCharArray();

                //將char陣列轉換成string陣列
                string[] strArray = new string[charArray.Length]; 
                for (int i = 0; i < charArray.Length; i++)
                {
                    strArray[i] = charArray[i].ToString();
                }

                //====================================================================================================================================

                bool X = readFileLine.Contains(x1);
                bool Y = readFileLine.Contains(y1);
                bool Z = readFileLine.Contains(z1);
                bool colon1000 = readFileLine.Contains(colon1000str);

                if (colon1000 == false && X == true || Y == true || Z == true)
                {
                    //====================================================================================================================================

                    //從string陣列第0位置的元素檢查到最後一個位置的元素
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        //如果陣列中的元素為X或Y或Z的話，就分別做某些事
                        if (strArray[i] == "X")
                        {
                            Tuple<string, int> Info = GetOriginValue(i, strArray.Length, strArray, arrayReadFileLine, "X");
                            originValueX = Info.Item1;
                            i = Info.Item2;

                        }
                        else if (strArray[i] == "Y")
                        {
                            Tuple<string, int> Info = GetOriginValue(i, strArray.Length, strArray, arrayReadFileLine, "Y");
                            originValueY = Info.Item1;
                            i = Info.Item2;
                        }
                        else if (strArray[i] == "Z")
                        {
                            Tuple<string, int> Info = GetOriginValue(i, strArray.Length, strArray, arrayReadFileLine, "Z");
                            originValueZ = Info.Item1;
                            i = Info.Item2;
                        }
                        else
                        {
                            arrayReadFileLine.Add(strArray[i]);
                        }
                    }

                    //====================================================================================================================================

                    //如果readFileLine字串是這些，那就1.不要補上XYZ、2.XYZ不要被帶換成旋轉後的數值 =>沒有做更動
                    bool G91G28 = readFileLine.Contains(G91G28str);
                    bool G0Z = readFileLine.Contains(G0Zstr);
                    //bool G43 = readFileLine.Contains(G43str);
                    //bool G53 = readFileLine.Contains(G53str);
                    bool colon1000Two = readFileLine.Contains(colon1000str);
                    if (G91G28 == false && G0Z == false && colon1000Two == false)//G53 == false && G43 == false && 
                    {
                        /*bool G90G55G0 = readFileLine.Contains(G90G55G0str);
                        if (G90G55G0 == false) //G90G55G0不要加Z
                        {
                            //找出arrayReadFileLine當中是否有X或Y或Z，如果傳回值為-1表示此Arraylist沒有此項目
                            int ARLIndX = arrayReadFileLine.IndexOf(x1);
                            int ARLIndY = arrayReadFileLine.IndexOf(y1);
                            int ARLIndZ = arrayReadFileLine.IndexOf(z1);
                            if (ARLIndX != -1 && ARLIndY == -1 && ARLIndZ == -1)//只有 X，沒有 Y Z //arrayReadFileLine = A1 B2 X3 D4 F5
                            {
                                arrayReadFileLine.Insert((ARLIndX + 1), "Y"); //把Y插入到arrayReadFileLine第(ARLIndX + 1)的位置//arrayReadFileLine = A1 B2 X3 Y4 D5 F6
                                arrayReadFileLine.Insert((ARLIndX + 2), "Z"); //arrayReadFileLine = A1 B2 X3 Y4 Z5 D6 F7
                            }
                            else if (ARLIndX == -1 && ARLIndY != -1 && ARLIndZ == -1)
                            {
                                arrayReadFileLine.Insert(ARLIndY, "X");
                                arrayReadFileLine.Insert((ARLIndY + 2), "Z");
                            }
                            else if (ARLIndX == -1 && ARLIndY == -1 && ARLIndZ != -1)
                            {
                                arrayReadFileLine.Insert(ARLIndZ, "X");
                                arrayReadFileLine.Insert(ARLIndZ + 1, "Y");
                            }
                            else if (ARLIndX == -1 && ARLIndY != -1 && ARLIndZ != -1)
                            {
                                arrayReadFileLine.Insert(ARLIndY, "X");
                            }
                            else if (ARLIndX != -1 && ARLIndY == -1 && ARLIndZ != -1)
                            {
                                arrayReadFileLine.Insert(ARLIndZ, "Y");
                            }
                            else if (ARLIndX != -1 && ARLIndY != -1 && ARLIndZ == -1)
                            {
                                arrayReadFileLine.Insert((ARLIndY + 1), "Z");
                            }
                        }*/

                        //====================================================================================================================================
                        
                        if(radioButton1.Checked.Equals(true))//公式
                        { 
                            newFileArray.Add( SelectFormula( originValueX, originValueY, originValueZ, splicingAll, arrayReadFileLine, newFileArray ));
                        }
                        else //數值
                        {
                            newFileArray.Add( SelectValue( originValueX, originValueY, originValueZ, arrayReadFileLine, splicingAll ));
                        }
                    }
                    else
                    {
                        newFileArray.Add(readFileLine);//把沒有要做更動的字串加入到myAL陣列
                    }
                }
                else //把沒有XYZ的那列字串加入到myAL陣列
                {
                    newFileArray.Add(readFileLine);
                }
                
                readFileLine = str.ReadLine();

                //====================================================================================================================================

                if (readFileLine != null)//讀取下一行，如果為null就不做這個修改
                {
                    //防呆警報
                    Regex G2 = new Regex(G2str);
                    Regex G3 = new Regex(G3str);
                    bool G02 = readFileLine.Contains(G02str);
                    bool G03 = readFileLine.Contains(G03str);
                    bool G68 = readFileLine.Contains(G68str);
                    bool G69 = readFileLine.Contains(G69str);
                    if (G2.IsMatch(readFileLine) || G3.IsMatch(readFileLine) || G02 || G03 || G68 || G69)
                    {
                        MessageBox.Show("文件中含有非核准之G碼\n" + "G2、G3、G02、G03、G68或G69\n" + "請重新檢查文件並刪除：" + readFileLine);
                    }

                    //====================================================================================================================================

                    //如果下一列有X或Y或Z的話就洗掉上一次迴圈的X或Y或Z的數值，沒有的話就不改變上一次的X或Y或Z數值以供下一列來使用
                    ClearValue(x1, colon1000str, originValueX, readFileLine);
                    ClearValue(y1, colon1000str, originValueY, readFileLine);
                    ClearValue(z1, colon1000str, originValueZ, readFileLine);
                }

            }
            str.Close();//結束讀取文件

            //====================================================================================================================================
            
            if (radioButton4.Checked.Equals(true))//新代
            {
                SyntecSituation(newFileArray);
            }
            else//Fanuc
            {
                FanucSituation(newFileArray);
            }
            
            //回傳 "轉換完成的檔案內容"
            return newFileArray;
        }

        /// <summary>
        /// 選擇公式
        /// </summary>
        /// <param name="SForiginValueX"></param>
        /// <param name="SForiginValueY"></param>
        /// <param name="SForiginValueZ"></param>
        /// <param name="SFarrayReadFileLine"></param>
        /// <param name="SFsplicingAll"></param>
        /// <param name="newFileArray">?</param>
        /// <returns></returns>
        public string SelectFormula(string SForiginValueX , string SForiginValueY , string SForiginValueZ , string SFsplicingAll , ArrayList SFarrayReadFileLine , ArrayList SFnewFileArray)
        {

            string formulaX = "#1=[[[" + SForiginValueX + "-0.0]*COS[" + textBox1.Text + "]-[" + SForiginValueZ + "-0.0]*SIN[" + textBox1.Text + "]+0.0]*1.0]";
            string formulaY = "#2=[[[" + SForiginValueY + "-0.0]*COS[" + textBox2.Text + "]-[[" + SForiginValueX + "-0.0]*SIN[" + textBox1.Text + "]+[" + SForiginValueZ + "-0.0]*COS[" + textBox1.Text + "]+0.0-0.0]*SIN[" + textBox2.Text + "]+0.0]*1.0]";
            string formulaZ = "#3=[[[" + SForiginValueY + "-0.0]*SIN[" + textBox2.Text + "]+[[" + SForiginValueX + "-0.0]*SIN[" + textBox1.Text + "]+[" + SForiginValueZ + "-0.0]*COS[" + textBox1.Text + "]+0.0-0.0]*COS[" + textBox2.Text + "]+0.0]*1.0]";

            string variableX = "X#1";
            string variableY = "Y#2";
            string variableZ = "Z#3";

            //把原本的被旋轉點座標，改成旋轉完成的點座標
            for (int arl = 0; arl < SFarrayReadFileLine.Count; arl++) 
            {
                string ChangeARL = ChangString(SFarrayReadFileLine[arl].ToString(), variableX, variableY, variableZ);
                SFsplicingAll = SFsplicingAll + ChangeARL; //重新整合成一個字串
            }
            
            //添加公式
            JoinFormula(x1, formulaX, SFsplicingAll, SFnewFileArray);
            JoinFormula(y1, formulaY, SFsplicingAll, SFnewFileArray);
            JoinFormula(z1, formulaZ, SFsplicingAll, SFnewFileArray);

            return SFsplicingAll;
        }

        /// <summary>
        /// 添加公式到陣列中
        /// </summary>
        /// <param name="coordinateJF"></param>
        /// <param name="formulaJF"></param>
        /// <param name="splicingAllJF"></param>
        /// <param name="newFileArrayJF"></param>
        public void JoinFormula(string coordinateJF, string formulaJF, string splicingAllJF, ArrayList newFileArrayJF)
        {
            bool coordinateXYZ = splicingAllJF.Contains(coordinateJF);
            if (coordinateXYZ == true)
            {
                newFileArrayJF.Add(formulaJF);
            }
        }
        
        /// <summary>
        /// 選擇數值
        /// </summary>
        /// <param name="SVoriginValueX"></param>
        /// <param name="SVoriginValueY"></param>
        /// <param name="SVoriginValueZ"></param>
        /// <param name="SVarrayReadFileLine"></param>
        /// <param name="SVsplicingAll"></param>
        /// <returns></returns>
        public string SelectValue(string SVoriginValueX , string SVoriginValueY , string SVoriginValueZ , ArrayList SVarrayReadFileLine, string SVsplicingAll)
        {
            //取得使用者輸入的XZ、YZ的角度
            float XZ = float.Parse(textBox1.Text), YZ = float.Parse(textBox2.Text);

            //計算XZ、YZ的角度
            double angleXZ = Math.PI * (XZ) / 180.0, angleYZ = Math.PI * (YZ) / 180.0;

            //計算XZ平面上轉的角度sin、cos數值為多少
            double sinXZ = Math.Sin(angleXZ), cosXZ = Math.Cos(angleXZ);

            //計算YZ平面上轉的角度sin、cos數值為多少
            double sinYZ = Math.Sin(angleYZ), cosYZ = Math.Cos(angleYZ);

            //轉換為Double型態
            double doubleOriginValueX = Convert.ToDouble(SVoriginValueX + "0"), doubleOriginValueY = Convert.ToDouble(SVoriginValueY + "0"), doubleOriginValueZ = Convert.ToDouble(SVoriginValueZ + "0");

            //算出XYZ旋轉完的數值
            double CalresX = ( doubleOriginValueX - 0.0 ) * cosXZ - ( doubleOriginValueZ - 0.0 ) * sinXZ + 0.0;
            double CalresY = ( doubleOriginValueY - 0.0 ) * cosYZ - ( ( doubleOriginValueX - 0.0 ) * sinXZ + ( doubleOriginValueZ - 0.0 ) * cosXZ + 0.0 - 0.0 ) * sinYZ + 0.0;
            double CalresZ = ( doubleOriginValueY - 0.0 ) * sinYZ + ( ( doubleOriginValueX - 0.0 ) * sinXZ + ( doubleOriginValueZ - 0.0 ) * cosXZ + 0.0 - 0.0 ) * cosYZ + 0.0;

            //取道小數點後3位
            string newValueX = "X" + Math.Round(CalresX, 3).ToString();//旋轉完的點座標X
            string newValueY = "Y" + Math.Round(CalresY, 3).ToString();//旋轉完的點座標Y
            string newValueZ = "Z" + Math.Round(CalresZ, 3).ToString();//旋轉完的點座標Z

            //整數的話，增加小數點
            newValueX = AddPoint(newValueX);
            newValueY = AddPoint(newValueY);
            newValueZ = AddPoint(newValueZ);

            for (int arl = 0; arl < SVarrayReadFileLine.Count; arl++) //把原本的被旋轉點座標，改成旋轉完成的點座標
            {
                string changStr = ChangString(SVarrayReadFileLine[arl].ToString(), newValueX, newValueY, newValueZ);
                SVsplicingAll = SVsplicingAll + changStr; //重新整合成一個字串
            }

            return SVsplicingAll;
        }

        /// <summary>
        /// 把整數變為小數型態
        /// </summary>
        /// <param name="integerAP"></param>
        /// <returns></returns>
        public string AddPoint(string integerAP)
        {
            string pointStr = ".";
            bool poin = integerAP.Contains(pointStr);
            if (poin == false)
            {
                integerAP = integerAP + ".0";
            }
            return integerAP;
        }

        /// <summary>
        /// 選擇Syntec新代 
        /// </summary>
        /// <param name="SSmyAL"></param>
        public void SyntecSituation(ArrayList finalOutputArraySS)
        {
            string leftBracketsStr = "(";
            for (int i = 0; i < finalOutputArraySS.Count; i++)
            {
                if (i == 0)
                {
                    finalOutputArraySS[i] = finalOutputArraySS[i] + " @MACRO";
                }
                else
                {
                    bool percentage = finalOutputArraySS[i].ToString().Contains("%");
                    if (percentage == false)
                    {
                        finalOutputArraySS[i] = finalOutputArraySS[i] + ";";
                    }
                    
                }
                
                int leftBrackets = finalOutputArraySS[i].ToString().IndexOf(leftBracketsStr);
                if (leftBrackets != 0 && leftBrackets != -1)
                {
                    finalOutputArraySS[i] = finalOutputArraySS[i].ToString().Replace("(", ";//(");
                }
                else
                {
                    finalOutputArraySS[i] = finalOutputArraySS[i].ToString().Replace("(", "//(");
                }

                finalOutputArraySS[i] = finalOutputArraySS[i].ToString().Replace("[", "(").Replace("]", ")").Replace("=", ":=").Replace("/M168", "//M168").Replace("M99", "M30").Replace("G5P0", "//G5P0").Replace("G5P10000", "//G5P10000");
                
            }
            
        }
        /// <summary>
        /// 選擇Fanuc發那科
        /// </summary>
        /// <param name="finalOutputArrayFS"></param>
        public void FanucSituation(ArrayList finalOutputArrayFS)
        {
            for (int i = 0; i < finalOutputArrayFS.Count; i++)
            {
                finalOutputArrayFS[i] = finalOutputArrayFS[i].ToString().Replace("M30", "M99").Replace("G5P0", "/G5P0").Replace("G5P10000", "/G5P10000");
            }
        }

        /// <summary>
        /// 把原本的被旋轉點座標，改成旋轉完成的點座標
        /// </summary>
        /// <param name="valueEndSpin"></param>
        /// <param name="originValueX"></param>
        /// <param name="originValueY"></param>
        /// <param name="originValueZ"></param>
        /// <returns>旋轉完的點座標</returns>
        static public string ChangString(string valueEndSpin, string originValueX, string originValueY, string originValueZ)
        {
            if (valueEndSpin.Equals("X"))
            {
                valueEndSpin = originValueX;
            }
            else if (valueEndSpin.Equals("Y"))
            {
                valueEndSpin = originValueY;
            }
            else if (valueEndSpin.Equals("Z"))
            {
                valueEndSpin = originValueZ;
            }
            return valueEndSpin;
        }

        /// <summary>
        /// 清除XYZ數值
        /// </summary>
        /// <param name="xyzCV"></param>
        /// <param name="colon1000CV"></param>
        /// <param name="originValueCV"></param>
        /// <param name="readFileLineCV"></param>
        public void ClearValue(string xyzCV, string colon1000CV, string originValueCV, string readFileLineCV)
        {
            bool xyz = readFileLineCV.Contains(xyzCV);
            bool colon1000 = readFileLineCV.Contains(colon1000CV);
            if (colon1000 == false && xyz == true)
            {
                originValueCV = "";
            }
        }

        /// <summary>
        /// 檢查是否是我們想要的0~9或減號或點
        /// </summary>
        /// <param name="iCountGOV"></param>
        /// <param name="strArrayLengthGOV"></param>
        /// <param name="strArrayGOV"></param>
        /// <param name="arrayReadFileLineGOV"></param>
        /// <param name="xyzGOV"></param>
        /// <returns></returns>
        public Tuple<string, int> GetOriginValue(int iCountGOV, int strArrayLengthGOV, String[] strArrayGOV, ArrayList arrayReadFileLineGOV, string xyzGOV)
        {
            string oneToNine = "[0-9]";
            string originValue = "";
            int i = iCountGOV + 1;

            //找出X後面的元素是否是我們想要的0~9或減號或點
            while (i < strArrayLengthGOV) 
            {
                Regex Nbon = new Regex(oneToNine);
                if (Nbon.IsMatch(strArrayGOV[i]) || strArrayGOV[i] == "-" || strArrayGOV[i] == ".")
                {
                    originValue = originValue + strArrayGOV[i];
                }
                else
                {
                    iCountGOV = i - 1; //如果不是數字0~9或減號或點的元素，那就跳出while迴圈，並記錄目前那個元素的前一個位置(使下一輪for迴圈能從那個元素檢測是否為XYZ)
                    break;
                }
                i++;
                iCountGOV = i; //此迴圈用while的主要原因，當所找的那一列已經是最後一個元素了，而剛好那個元素是0~9或是減號或是點
                                  //，那就會跳過else裡的 strA = i - 1的這個紀錄動作，所以必須用while迴圈在i++;後做紀錄的動作
            }
            arrayReadFileLineGOV.Add(xyzGOV);

            return new Tuple<string, int>(originValue, iCountGOV); 
        }
        
        /// <summary>
        /// 建立檔案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {

            ArrayList newFileArray = ConversionOperation(this.TargetFilePathName);

            if (radioButton3.Checked.Equals(true))//Fanuc //P公式 //V數值
            {
                if (radioButton1.Checked.Equals(true))
                    OutFile(TargetFilePath + "\\P", TargetFileName, newFileArray);
                else
                    OutFile(TargetFilePath + "\\V", TargetFileName, newFileArray);
            }
            else //新代
            {
                if (radioButton1.Checked.Equals(true))
                    OutFile(TargetFilePath, "\\O1111", newFileArray);//公式
                else
                    OutFile(TargetFilePath, "\\O1112", newFileArray);//數值
            }

            MessageBox.Show("轉檔完成");

            //====================================================================================================================================

            //轉檔完重新整理資料
            newFileArray.Clear();
        }

        /// <summary>
        /// 輸出結果
        /// </summary>
        /// <param name="outputPathOF"></param>
        /// <param name="outputNameOF"></param>
        /// <param name="newFileArrayOF"></param>
        static public void OutFile(string outputPathOF, string outputNameOF, ArrayList newFileArrayOF)
        {
            StreamWriter wri = new StreamWriter(outputPathOF + outputNameOF);
            foreach (Object obj in newFileArrayOF)
            {
                wri.WriteLine(obj);
            }
            wri.Close();
        }
    }
}