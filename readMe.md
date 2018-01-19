
# 서울시 indoor 의 텍스쳐를 긁어보자.

오픈 API를 가져오는 공부와 세마포어를 적용해보자! 라는 것으로 만들어본 응용 어플리케이션  
결과물은 여러개의 png 파일이며 지정된 파일 경로에서 적절하게 건물 id 로 묶여서 저장된다.

진행순서는 다음과 같다.

API를 이용하여 건물 정보 및 id 저장 -> 해당 건물 sbm File 긁어오기 -> sbm file save  
 -> sbm 내용물 read -> 건물 텍스쳐 file get -> 텍스쳐 download

API 정보는 다음을 참고 [서울시 indoor API](http://indoormap.seoul.go.kr/openapi/request.html)
(이 API는 api key 가 없어도 이용이 가능하다.)

각 코드의 세부 설명은 주석 및 아래를 참고 

* 리스트 뷰 xaml - prjid, bldid, fileid, nameKr 출력
* sbm 저장 위치 C - dev - sbm 에 각 건물 이름 폴더가 생성되고 저장됨
* sbmall - 한 건물 내 전체 sbm 받아옴
* sbm - 해당 건물의 디폴트 floorid 를 이용하여 한개의 sbm 만 받아옴.
* refresh - 내부지도가 있는 전체 건물 목록을 리스트 뷰에 올려줌.
* refresh 를 눌러 건물 목록을 받은 후, 해당 건물을 클릭한 후 sbmall 이나 sbm 버튼을 누르면 됨.
* json를 파싱한 후 어레로 변환하는 get, set 은 각 issue.cs, sbmissue.cs, sbmall.cs 에 지정되어 있음.

다음은 실행화면을 나타낸 것이다.

![초기화면](https://github.com/IngIeoAndSpare/getTextureFromIndoorMap-seoul-/blob/master/%EC%8B%A4%ED%96%89%ED%99%94%EB%A9%B4/start.png)

각 버튼의 설명은 위의 설명을 참고
실행순서는 refresh 를 먼저 실행한 후 건물을 선택해서 sbm을 누르거나 선택하지 않는 상태에서 sbmALL을 누른 후 ALLDown 을 누르면 텍스쳐를 다운받을 수 있다.  
SBM, SBMALL은 말 그대로 sbm파일을 다운 받는 것이다. 텍스쳐를 받으려면 SBM 혹은 SBMALL이 우선 실행되어야 한다.

다음은 refresh 를 실행한 화면이다.

![refresh 실행화면](https://github.com/IngIeoAndSpare/getTextureFromIndoorMap-seoul-/blob/master/%EC%8B%A4%ED%96%89%ED%99%94%EB%A9%B4/loadSBM.png)  
보다시피 indoor 에서 제공하는 건물의 목록이 나타나진다. 