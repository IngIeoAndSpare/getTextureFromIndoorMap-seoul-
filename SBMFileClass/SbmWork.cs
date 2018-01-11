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
    class SbmWork
    {
        OverlapCheck overlapCheck = new OverlapCheck();
        
        public void SbmDownload(List<SbmIssue> sbm)
        {

            var sbmJsonInfo = ((SbmIssue)sbm[0]);
            string sbmfileName = sbmJsonInfo.sbmFile.ToString();
            string bldcode = sbmJsonInfo.bcode.ToString();
            string floorid = sbmJsonInfo.floorId.ToString();
            string sbmDownUrl = "http://115.84.164.128/app/download/sbm/" + bldcode + "/" + floorid + "/";

            WebClient webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            webClient.DownloadFileAsync(new Uri(sbmDownUrl), (@"C:\Dev\sbm\" + sbmfileName));
            while (webClient.IsBusy) { }

            MessageBox.Show("Download completed!");
            webClient.Dispose();
            MakeFileList(sbmfileName, sbm);

        }//sbmDownload end 


        private void MakeFileList(String sbmfileName, List<SbmIssue> sbmIssue)
        {
            FileStream fileStream = new FileStream(@"C:\Dev\sbm\" + sbmfileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
            BinaryReader binaryReader = new BinaryReader(fileStream);//, Encoding.UTF8);

            List<string> sbm = new List<string>();

            byte[] formatName = binaryReader.ReadBytes(8); //"SMB-----"
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

                if (stringLength < 5 )
                {
                    continue;
                }

                char[] textureFileName = binaryReader.ReadChars(stringLength);
                // int testLet = binaryReader.PeekChar();

                //TODO : null 을 처리하려고 아래와 같은 조건을 썻으나 그냥 통과되어버림... 그냥 download 될 때 줘야 될 것 같음.
                               
                sbm.Add(new string(textureFileName).Substring(5));
            }


            /************** 이하 부분은 아직 사용할 일이 없음--
            /*
            for (int i = 0; i < meshCount; i++)
            {
                uint stringId = binaryReader.ReadUInt16();
                uint mat_id = binaryReader.ReadUInt16();
                uint vtCount = binaryReader.ReadUInt16();
                for (int j = 0; j < vtCount; j++)
                {
                    binaryReader.ReadSingle();
                    binaryReader.ReadSingle();
                    binaryReader.ReadSingle();
                }
                uint norCount = binaryReader.ReadUInt16(); // Normal Vector
                for (int j = 0; j < norCount; j++)
                {
                    binaryReader.ReadSingle();
                    binaryReader.ReadSingle();
                    binaryReader.ReadSingle();
                    binaryReader.ReadSingle();
                }
                uint tcCount = binaryReader.ReadUInt16();//Texture Coordinate
                for (int j = 0; j < tcCount; j++)
                {
                    binaryReader.ReadSingle();
                    binaryReader.ReadSingle();
                }
                uint idxCount = binaryReader.ReadUInt16();
                for (int j = 0; j < idxCount; j++)
                {
                    binaryReader.ReadUInt16();
                    binaryReader.ReadUInt16();
                    binaryReader.ReadUInt16();
                }
            }
            */

            binaryReader.Close();
            binaryReader.Dispose();

            fileStream.Close();
            //중복제거
            sbm = overlapCheck.CheckOverlap(sbm);
            TextureDownload(sbm, sbmIssue);
        }

        private void TextureDownload(List<string> sbmFileList, List<SbmIssue> items)
        {
            //115.84.164.128/app/download/maps/P0482/Z0471/B0470/20160111-175113/?fileName=FireBox01_t01.jpg
            string prjCode = items[0].prjCode;
            string zoneCode = items[0].zcode;
            string bldCode = items[0].bcode;
            string bldVer = items[0].bldVer;

            string downUrl = "http://115.84.164.128/app/download/maps/" + prjCode + "/" + zoneCode + "/" + bldCode + "/" + bldVer + "/?fileName=";
            int flag = 0;
            WebClient webClient = null;

            if (!Directory.Exists(@"C:\Dev\texture\" + prjCode + "_default"))
            {
                Directory.CreateDirectory(@"C:\Dev\texture\" + prjCode + "_default");
                foreach (string fileName in sbmFileList)
                {
                    if (fileName == null || fileName.Length == 0)
                    {
                        Console.WriteLine("# fileName Error");
                        continue;
                    }

                    webClient = new WebClient();
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                    try
                    {
                        webClient.DownloadFileAsync(new Uri(downUrl + fileName), @"C:\Dev\texture\" + prjCode + "_default\\" + fileName);
                        while (webClient.IsBusy) { }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message.ToString());
                    }

                }
            }//if
            else
            {
                flag = 1;
            }
            if (flag == 0)
                MessageBox.Show("texture Download completed!");
            else
                MessageBox.Show("텍스쳐 파일이 이미 있습니다.");
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            //nothing
        }//sbm download completed


    }
}
