/*-------------------------------------------------------------------------------------------
// Copyright (C) 2020 成都，天龙互娱
//
// 模块名：EnterAssetCheck
// 创建日期：2020-7-22
// 创建者：qibo.li
// 模块描述：进入资源资源检测模块
//-------------------------------------------------------------------------------------------*/

using UnityEngine;

namespace DragonU3DSDK.Asset
{
    public class EnterAssetCheck : MonoBehaviour
    {
        private int leftCnt;
        private int rightCnt;

        private int loopTime = 1;
        
        private bool enter;
        
        public void LeftOnClick()
        {
            ++leftCnt;

            Enter();
        }

        public void RightOnClick()
        {
            ++rightCnt;

            Enter();
        }

        void Enter()
        {
            if (enter)
            {
                return;
            }
            
            if (leftCnt == 10 && rightCnt == 10)
            {
                if (loopTime > 0)
                {
                    loopTime--;
                    leftCnt = 0;
                    rightCnt = 0;
                    return;
                }
                
                enter = true;
                AssetCheck.CheckStart();
            }
        }
    }
}

