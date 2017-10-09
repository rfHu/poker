using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace MTTMsgPage
{
    public class MTTMsgP3 : MonoBehaviour
    {
        MTTType nowType;

        public MyParams adapterParams;

        MyScrollRectAdapter _Adapter;

        void OnEnable()
        {
            _Adapter = new MyScrollRectAdapter();
            _Adapter.Init(adapterParams);
        }

        void OnDisable()
        {
            if (_Adapter != null)
                _Adapter.Dispose();
        }

        [Serializable]
        public class MyParams : BaseParams
        {
            public RectTransform BBLevelListGo;

            public List<MTTPageData> rowData = new List<MTTPageData>();
        }

        public class MyScrollRectAdapter : ScrollRectItemsAdapter8<MyParams, MTTCellView>
        {
            protected override float GetItemWidth(int index)
            { return 780; }

            protected override float GetItemHeight(int index)
            { return 64; }

            protected override MTTCellView CreateViewsHolder(int itemIndex)
            {
                var data = _Params.rowData[itemIndex];
                MTTCellView instance;

                instance = new BBLevelListGo();
                instance.Init(_Params.BBLevelListGo, itemIndex);

                return instance;
            }

            protected override void UpdateViewsHolder(MTTCellView newOrRecycled)
            {
                MTTPageData model = _Params.rowData[newOrRecycled.itemIndex];
                newOrRecycled.SetData(model);
            }

            protected override bool IsRecyclable(MTTCellView potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, float heightOfItemThatWillBecomeVisible)
            { return potentiallyRecyclable.CanPresentModelType(_Params.rowData[indexOfItemThatWillBecomeVisible].cachedType); }
        }

        public void SetPage() 
        {
            StartCoroutine(SetPageCor());
        }

        IEnumerator SetPageCor()
        {
            var rowData = new List<MTTPageData>();

            int[,] newArr = new int[,] { };
            if (GameData.MatchData.MTTType == MTTType.Normal)
                newArr = new int[,]{{20, 0},{30, 0},{50, 0},{100, 0},{150, 10},{200, 10},{250, 25},{300, 25},{400, 50},{600, 50},
                        {800, 75},{1000, 100},{1200, 125},{1600, 150}, {2000, 200}, {2500, 250}, {3000,300}, {4000,400}, {5000,500},{ 6000,600}, 
                        {7000,700}, {8000,800}, {10000,1000}, {12000,1200}, {16000,1600}, {20000,2000},{25000,2500}, {30000,3000}, {40000,4000}, 
                        {50000,5000}, {60000,6000}, {80000,8000}, {100000,10000}, {120000,12000}, {150000,15000}, {180000,18000}, {210000,21000}, 
                        {240000,24000}, {280000,28000}, {320000,32000}, {360000,36000}, {400000,40000}, {450000,45000}, {500000,50000}, 
                        {550000,55000}, {600000,60000}, {650000,65000}, {700000,70000}, {800000,80000}, {900000,90000},{1000000,100000}, 
                        {1200000,120000}, {1400000,140000}, {1600000,160000}, {1800000,180000}, {2000000,200000}};
            else
                newArr = new int[,] {{20, 0}, {40, 0}, {80, 0}, {160, 0}, {240, 0}, {320, 0}, {400, 40}, {640, 80}, {800, 120},
                        {1200, 200},{1600, 200}, {2400, 400}, {3200, 600}, {4000, 600}, {6400, 800}, {8000, 1200}, {12000, 2000}, 
                        {16000, 2000}, {24000, 4000}, {32000, 6000}, {40000, 6000}, {64000, 8000}, {80000, 12000}, {120000, 20000},
                        {160000, 20000}, {240000, 40000}, {320000, 60000}, {400000, 60000}, {640000, 80000},{800000, 120000} };

            bool needWaiting = transform.parent.parent.GetComponent<MTTMsg>().SetGoSize(true);

            if (needWaiting)
            {
                yield return new WaitForSeconds(0.2f);
            }

            _Adapter.Init(adapterParams);

            for (int i = 0; i < newArr.GetLength(0); i++)
            {
                var awardData = new BBLevelListGoData()
                {
                    Level = i + 1,
                    BB = newArr[i, 0],
                    Ante = newArr[i, 1],
                };

                rowData.Add(awardData);
            }

            adapterParams.rowData.Clear();
            adapterParams.rowData.AddRange(rowData);
            _Adapter.ChangeItemCountTo(rowData.Count);
        }
    }
}