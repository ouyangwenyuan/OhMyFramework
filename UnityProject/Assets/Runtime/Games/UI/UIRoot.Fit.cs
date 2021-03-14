using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using DragonU3DSDK;
using UnityEngine.U2D;
using UnityEngine.UI;
using DragonU3DSDK.Asset;

public partial class UIRoot
{
    public bool FitUIPos(GameObject ui, GameObject safeArea)
    {
        if (ui == null)
        {
            return false;
        }
        float marginTop = 0f;
        float marginBottom = 0f;
        float marginLeft = 0f;
        float marginRight = 0f;
        if (safeArea != null)
        {
            var canvasRect = this.mRoot.GetComponent<RectTransform>();
            var safeRect = safeArea.GetComponent<RectTransform>();
            var safeAreaLocalPos = this.mRoot.transform.InverseTransformPoint(safeArea.transform.position);
            float widthGap = Math.Abs(safeRect.rect.width - canvasRect.rect.width);
            float heightGap = Math.Abs(safeRect.rect.height - canvasRect.rect.height);
            if (widthGap > 1)
            {
                marginLeft = widthGap / 2;
                marginRight = widthGap / 2;
                marginLeft += safeAreaLocalPos.x;
                marginRight -= safeAreaLocalPos.x;
            }
            if (heightGap > 1)
            {
                marginTop = heightGap / 2;
                marginBottom = heightGap / 2;
                marginTop -= safeAreaLocalPos.y;
                marginBottom += safeAreaLocalPos.y;
            }
        }
        return FitUIPos(ui, marginTop, marginBottom, marginLeft, marginRight);
    }

    public bool FitUIPos(GameObject ui, float marginTop = 0, float marginBottom = 0, float marginLeft = 0, float marginRight = 0)
    {
        bool haveChange = false;
        if (ui == null)
        {
            return haveChange;
        }
        var canvasRect = this.mRoot.GetComponent<RectTransform>();
        var panelRect = ui.GetComponent<RectTransform>();
        float halfWidth = panelRect.rect.width / 2;
        float halfHeight = panelRect.rect.height / 2;

        var localPos = this.mRoot.transform.InverseTransformPoint(ui.transform.position);
        if (localPos.x > 0)
        {
            float safeWidth = canvasRect.rect.width / 2 - marginRight;
            if (localPos.x >= (safeWidth - halfWidth))
            {
                localPos.x = safeWidth - halfWidth;
                haveChange = true;
            }
        }
        else
        {
            float safeWidth = canvasRect.rect.width / 2 - marginLeft;
            if (localPos.x <= -(safeWidth - halfWidth))
            {
                localPos.x = -(safeWidth - halfWidth);
                haveChange = true;
            }
        }

        if (localPos.y > 0)
        {
            float safeHeight = canvasRect.rect.height / 2 - marginTop;
            if (localPos.y >= (safeHeight - halfHeight))
            {
                localPos.y = safeHeight - halfHeight;
                haveChange = true;
            }
        }
        else
        {
            float safeHeight = canvasRect.rect.height / 2 - marginBottom;
            if (localPos.y <= -(safeHeight - halfHeight))
            {
                localPos.y = -(safeHeight - halfHeight);
                haveChange = true;
            }
        }
        ui.transform.position = this.mRoot.transform.TransformPoint(localPos);
        return haveChange;
    }
}

public enum BubbleArea
{
    LeftTop = 1,
    RightTop = 2,
    LeftBottom = 3,
    RightBottom = 4
}

public class BubbleLabelInfo
{
    public BubbleArea BubbleArea;
    public Vector3 CornerPos;
    public float Angle;
    public Vector3 LabelPos;
    private float L = 300f;

    public BubbleLabelInfo(GameObject ui, GameObject rootArea)
    {
        var rootRect = rootArea.GetComponent<RectTransform>();
        var localPos = rootArea.transform.InverseTransformPoint(ui.transform.position);

        if (localPos.x < 0 && localPos.y > 0)
        {
            this.BubbleArea = BubbleArea.LeftTop;
            this.CornerPos = new Vector3(-rootRect.rect.width / 2, rootRect.rect.height / 2, 0);
        }
        else if (localPos.x > 0 && localPos.y > 0)
        {
            this.BubbleArea = BubbleArea.RightTop;
            this.CornerPos = new Vector3(rootRect.rect.width / 2, rootRect.rect.height / 2, 0);
        }
        else if (localPos.x < 0 && localPos.y < 0)
        {
            this.BubbleArea = BubbleArea.LeftBottom;
            this.CornerPos = new Vector3(-rootRect.rect.width / 2, -rootRect.rect.height / 2, 0);
        }
        else
        {
            this.BubbleArea = BubbleArea.RightBottom;
            this.CornerPos = new Vector3(rootRect.rect.width / 2, -rootRect.rect.height / 2, 0);
        }

        float distance = Vector3.Distance(localPos, this.CornerPos);
        float bet = distance / (distance + L);
        float distanceX = rootRect.rect.width / 2 - Mathf.Abs(localPos.x);
        float deltaX = distanceX / bet - distanceX;
        float distanceY = rootRect.rect.height / 2 - Mathf.Abs(localPos.y);
        float deltaY = distanceY / bet - distanceY;

        Vector3 d2 = localPos - this.CornerPos;
        switch (this.BubbleArea)
        {
            case BubbleArea.LeftTop:
                {
                    Vector3 d1 = new Vector3(0, -1, 0);
                    float dot = Vector3.Dot(d1, d2.normalized);
                    this.Angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
                    this.LabelPos = new Vector3(deltaX, -deltaY, 0);
                }
                break;
            case BubbleArea.RightTop:
                {
                    Vector3 d1 = new Vector3(0, -1, 0);
                    float dot = Vector3.Dot(d1, d2.normalized);
                    this.Angle = 360 - Mathf.Acos(dot) * Mathf.Rad2Deg;
                    this.LabelPos = new Vector3(-deltaX, -deltaY, 0);
                }
                break;
            case BubbleArea.LeftBottom:
                {
                    Vector3 d1 = new Vector3(0, 1, 0);
                    float dot = Vector3.Dot(d1, d2.normalized);
                    this.Angle = 180 - Mathf.Acos(dot) * Mathf.Rad2Deg;
                    this.LabelPos = new Vector3(deltaX, deltaY, 0);
                }
                break;
            case BubbleArea.RightBottom:
                {
                    Vector3 d1 = new Vector3(0, 1, 0);
                    float dot = Vector3.Dot(d1, d2.normalized);
                    this.Angle = 180 + Mathf.Acos(dot) * Mathf.Rad2Deg;
                    this.LabelPos = new Vector3(-deltaX, deltaY, 0);
                }
                break;
        }
    }
}