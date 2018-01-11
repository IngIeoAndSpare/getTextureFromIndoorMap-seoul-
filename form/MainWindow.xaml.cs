/* 리스트 뷰 xaml - prjid, bldid, fileid, nameKr 출력
 * json 제공 api - http://indoormap.seoul.go.kr/openapi/request.html
 * sbm 저장 위치 C - dev - sbm 에 각 건물 이름 폴더가 생성되고 저장됨
 * sbmall - 한 건물 내 전체 sbm 받아옴
 * sbm - 해당 건물의 디폴트 floorid 를 이용하여 한개의 sbm 만 받아옴.
 * refresh - 내부지도가 있는 전체 건물 목록을 리스트 뷰에 올려줌.
 * refresh 를 눌러 건물 목록을 받은 후, 해당 건물을 클릭한 후 sbmall 이나 sbm 버튼을 누르면 됨.
 * json를 파싱한 후 어레로 변환하는 get, set 은 각 issue.cs, sbmissue.cs, sbmall.cs 에 지정되어 있음.
 * 
 */


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
using System.Threading;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        SbmWork sbmWork = new SbmWork(); // 디폴트 sbm만 처리함
        SbmAllWork sbmAllWork = new SbmAllWork(); // 한 건물 내에 있는 sbm 전체 처리함
        List<UrlComponent> urlCollecter = new List<UrlComponent>(); // prjid, bldid, zoneCode, bldVer
        AllDownSbm alldownSbm = AllDownSbm.GetInstance();

        public MainWindow()
        {
            InitializeComponent();
        }


        private async void Refresh_Button_Click(object sender, RoutedEventArgs e)
        {
            string url = "http://indoormap.seoul.go.kr/app/openapi/seoulcity/svcproject/list.json";
            string json = await RequestJson(url);
            this.ParseJson(json, 0);
        }

        private async void Sbm_Button_Click(object sender, RoutedEventArgs e)
        {
            if (IssueListView.SelectedItems.Count != 0)
            {
                string prjid = urlCollecter[0].prjid;

                string url = "http://indoormap.seoul.go.kr/app/openapi/seoulcity/svcmanage/getSvcBuildingInfo.json?prjId=" + prjid;
                string json = await RequestJson(url);
                this.ParseJson(json, 1);
            }

        }//sbm pars

        private async void SbmAll_Button_Click(object sender, RoutedEventArgs e)
        {
            if (IssueListView.SelectedItems.Count != 0)
            {
                string prjid = urlCollecter[0].prjid;
                string bldid = urlCollecter[0].bldid;

                string url = "http://indoormap.seoul.go.kr/app/openapi/seoulcity/svcmanage/getSvcFloorList.json?prjId=" + prjid + "&bldId=" + bldid;
                string json = await RequestJson(url);
                this.ParseJson(json, 2);
            }
        }//sbm_all pars

        private async void AllDown_Button_Click(object sender, RoutedEventArgs e)
        {
            foreach(Issue items in IssueListView.Items.SourceCollection)
            {
                urlCollecter.Clear();

                UrlComponent urlComponent = new UrlComponent();

                urlComponent.bldid = items.bldId;
                urlComponent.prjid = items.PrjId.ToString();
                urlComponent.zcode = items.code.ToString();
                urlComponent.bldVer = items.bldVer.ToString();
                urlCollecter.Add(urlComponent);

                while (alldownSbm.checkThreadPoint()) { }
                alldownSbm.upThreadPoint();
                string url = "http://indoormap.seoul.go.kr/app/openapi/seoulcity/svcmanage/getSvcFloorList.json?prjId="
                         + items.PrjId + "&bldId=" + items.bldId;

                string json = await RequestJson(url);
                this.ParseJson(json, 2);

            }
            List<string> list = new List<string>();
            list = alldownSbm.getErrorList();
            System.IO.File.WriteAllLines(@"C:\Dev\texture\ErrorSbm.txt", list);

        }// all download

        private async Task<string> RequestJson(String url)
        {

            HttpClient client = new HttpClient();
            Task<string> getStringTask = client.GetStringAsync(url);
            string result = await getStringTask;
            return result;
        }

        private void ParseJson(String json, int flag)
        {
            int num = 0; // 단순 번호 매김. 수정시 Issue.cs 수정할 것

            if (flag == 0)
            {
                List<Issue> issues = new List<Issue>();

                JObject obj = JObject.Parse(json);
                JArray array = JArray.Parse(obj["result"].ToString());
                foreach (JObject itemObj in array)
                {
                    num++;
                    Issue issue = new Issue();
                    issue.idnum = num;
                    issue.bldId = itemObj["buildingInfo"]["bldId"].ToString();
                    issue.bldVer = itemObj["buildingInfo"]["bldVer"].ToString();
                    issue.PrjId = itemObj["prjId"].ToString();
                    issue.fileId = itemObj["file"]["fileId"].ToString();
                    issue.nameKr = itemObj["nameKr"].ToString();
                    issue.code = itemObj["zoneInfo"]["code"].ToString();
                    issues.Add(issue);
                }

                IssueListView.ItemsSource = issues;
            } // 건물목록 불러올 때.

            else if (flag == 1)
            {
                List<SbmIssue> sbmIssues = new List<SbmIssue>();
                JObject sObj = JObject.Parse(json);
                JArray sbArray = JArray.Parse(sObj["result"].ToString());
                foreach (JObject itemObj in sbArray)
                {
                    SbmIssue sbmIssue = new SbmIssue();
                    sbmIssue.floorId = itemObj["defaultFloorInfo"]["floorId"].ToString();
                    sbmIssue.sbmFile = itemObj["defaultFloorInfo"]["sbmFile"].ToString();
                    sbmIssue.prjCode = itemObj["projectInfo"]["code"].ToString();
                    sbmIssue.zcode = itemObj["zoneInfo"]["code"].ToString();
                    sbmIssue.bcode = itemObj["code"].ToString();
                    sbmIssue.bldVer = itemObj["bldVer"].ToString();
                    sbmIssues.Add(sbmIssue);
                }
                sbmWork.SbmDownload(sbmIssues);
            }//sbm 관련 정보 불러올 때. 

            else if (flag == 2)
            {
                List<SbmAll> sbmallCollect = new List<SbmAll>();
                JObject sAllObj = JObject.Parse(json);
                JArray saArray = JArray.Parse(sAllObj["floorInfoList"].ToString());

                foreach (JObject itemObj in saArray)
                {
                    SbmAll sbmall = new SbmAll();
                    sbmall.floorId = itemObj["floorId"].ToString();
                    sbmall.bcode = itemObj["buildingInfo"]["code"].ToString();
                    sbmall.namekr = itemObj["buildingInfo"]["nameKr"].ToString();
                    sbmall.sbmFile = itemObj["sbmFile"].ToString();
                    sbmallCollect.Add(sbmall);
                }

                sbmAllWork.SbmAllDownload(sbmallCollect, urlCollecter);

            }//한 건물내 전체 층 sbm관련 정보 불러올 때.
        }

        private void IssueListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (IssueListView.SelectedItems.Count != 0)
            {
                urlCollecter.Clear();
                UrlComponent urlComponent = new UrlComponent();
                var selectedItem = ((Issue)IssueListView.SelectedItem);

                //!notice - 나중에 필요하면 urlCollecter.cs 에 아래 변수를 넣을것.
                string selectedFileId = selectedItem.fileId.ToString();
                string selectedNameKr = selectedItem.nameKr.ToString();

                urlComponent.bldid = selectedItem.bldId.ToString();
                urlComponent.prjid = selectedItem.PrjId.ToString();
                urlComponent.zcode = selectedItem.code.ToString();
                urlComponent.bldVer = selectedItem.bldVer.ToString();
                urlCollecter.Add(urlComponent);

            }
        }//listview eventhandler

    }
}
