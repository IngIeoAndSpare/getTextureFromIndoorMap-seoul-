using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
namespace WpfApp1
{
    class SbmAllWork
    {
        OverlapCheck overlapCheck = new OverlapCheck();
        AllDownSbm alldownSbm = AllDownSbm.GetInstance();

        public void SbmAllDownload(List<SbmAll> sbm, List<UrlComponent> urlCollecter)
        {
            string bldcode = sbm[0].bcode.ToString();
            string nameKr = sbm[0].namekr.ToString();
            int flag = 0;
            List<string> sbmFileName = new List<string>();
            WebClient webClient = null;

            if (!System.IO.Directory.Exists(@"C:\Dev\sbm\" + nameKr))
            {
                System.IO.Directory.CreateDirectory(@"C:\Dev\sbm\" + nameKr);
                foreach (SbmAll items in sbm)
                {
                    string floorid = items.floorId.ToString();
                    string sbmfileName = items.sbmFile.ToString();
                    sbmFileName.Add(sbmfileName);
                    string sbmallUrl = "http://115.84.164.128/app/download/sbm/" + bldcode + "/" + floorid + "/"; ;


                    webClient = new WebClient();
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                    webClient.DownloadFileAsync(new Uri(sbmallUrl), (@"C:\Dev\sbm\" + nameKr + "\\" + sbmfileName));
                    while (webClient.IsBusy) { }

                }
            } //파일이 존재하지 않을 때만 다운로드.            
            else
            {   
                flag = 1;
                foreach(SbmAll items in sbm)
                {
                    string sbmfileName = items.sbmFile.ToString();
                    sbmFileName.Add(sbmfileName);
                }
            }
      /*      if (flag == 0)
                MessageBox.Show("Download completed!");
            else
                MessageBox.Show("다운로드를 건너뜁니다.");
                */
            this.MakeFileList(sbmFileName, urlCollecter, nameKr);
        }// sbmAllDownload


        private void MakeFileList(List<string> sbmfileList, List<UrlComponent> urlCollecter, string nameKr)
        {

            List<string> sbm = new List<string>();
            byte[] formatName = new byte[8];
            foreach (string fileList in sbmfileList)
            {
                FileStream fileStream = new FileStream(@"C:\Dev\sbm\"+nameKr+"\\"+ fileList, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
                BinaryReader binaryReader = new BinaryReader(fileStream);//, Encoding.UTF8);
                string checkList = "";

                try
                {
                    formatName = binaryReader.ReadBytes(8); //"SMB-----"
                    byte version = binaryReader.ReadByte();

                    uint meterialCount = binaryReader.ReadUInt16();
                    uint meshCount = binaryReader.ReadUInt16();


                    //byte[] meshBlock = binaryReader.ReadBytes((int)meshCount * (6));
                    for (int i = 0; i < meterialCount; i++)
                    {
                        //byte[] meterialBlock = 

                        uint id = binaryReader.ReadUInt16();
                        binaryReader.ReadBytes(((12 * 4) + 1));
                        UInt16 stringLength = binaryReader.ReadUInt16();
                        //이부분이 이름출력됨.

                        if (stringLength < 5)
                            continue;


                        char[] textureFileName = binaryReader.ReadChars(stringLength);
                        // int testLet = binaryReader.PeekChar();
                        checkList = new string(textureFileName).Substring(5);
                        sbm.Add(checkList);
                    }
                    sbm = overlapCheck.CheckOverlap(sbm);
                    TextureDownload(sbm, urlCollecter);
                }
                catch(Exception ex)
                {
                    alldownSbm.setErrorList(checkList);
                    alldownSbm.downThreadPoint();
                }

                binaryReader.Close();
                binaryReader.Dispose();

                fileStream.Close();
            }
            //중복 제거
        }


        private void TextureDownload(List<string> sbmFileList, List<UrlComponent> items)
        {
            //115.84.164.128/app/download/maps/P0482/Z0471/B0470/20160111-175113/?fileName=FireBox01_t01.jpg
            string prjCode = "P0"+items[0].prjid;
            string zoneCode = items[0].zcode;
            string bldCode = "B0"+items[0].bldid;
            string bldVer = items[0].bldVer;

            int flag = 0;
            string downUrl = "http://115.84.164.128/app/download/maps/" + prjCode + "/" + zoneCode + "/" + bldCode + "/" + bldVer + "/?fileName=";

            if (!Directory.Exists(@"C:\Dev\texture\" + prjCode + "_All"))
            {
                Directory.CreateDirectory(@"C:\Dev\texture\" + prjCode + "_All");
                foreach (string fileName in sbmFileList)
                {
                    WebClient webClient = new WebClient();
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                    webClient.DownloadFileAsync(new Uri(downUrl + fileName), @"C:\Dev\texture\" + prjCode + "_All\\" + fileName);
                    while(webClient.IsBusy){ }

                    webClient.Dispose();
                }
            }// 텍스쳐 파일이 없을 때
            else
            {
                flag = 1;
            }
/*
            if (flag == 0)
                MessageBox.Show("texture Download completed!");
            else
                MessageBox.Show("텍스쳐가 이미 존재합니다!");
                */
           alldownSbm.downThreadPoint();
            //세마포어를 좀 해놔야겠다... 
           
        }
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            //nothing
        }//sbm download completed



    }
}
