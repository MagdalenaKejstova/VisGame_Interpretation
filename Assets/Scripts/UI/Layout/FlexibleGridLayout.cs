using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FlexibleGridLayout : LayoutGroup
{
    public enum FitType
    {
        Uniform,
        Width,
        Height,
        FixedRows,
        FixedColumns
    }

    public int rows;
    public int columns;
    public Vector2 cellSize;
    public Vector2 spacing;
    public FitType fitType;

    public bool fitX;
    public bool fitY;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        FitType[] autoLayoutTypes = { FitType.Uniform, FitType.Width, FitType.Height };
        if (autoLayoutTypes.Contains(fitType))
        {
            fitX = true;
            fitY = true;

            float childCountSqrt = Mathf.Sqrt(transform.childCount);
            rows = Mathf.CeilToInt(childCountSqrt);
            columns = Mathf.CeilToInt(childCountSqrt);
        }

        switch (fitType)
        {
            case FitType.Width:
            case FitType.FixedColumns:
                rows = Mathf.CeilToInt(transform.childCount / (float)columns);
                break;
            case FitType.Height:
            case FitType.FixedRows:
                columns = Mathf.CeilToInt(transform.childCount / (float)rows);
                break;
        }

        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;

        float cellWidth = (parentWidth - spacing.x * 2 - padding.left - padding.right) / columns;
        float cellHeight = (parentHeight - spacing.y * 2 - padding.top - padding.bottom) / rows;

        cellSize.x = fitX ? cellWidth : cellSize.x;
        cellSize.y = fitY ? cellHeight : cellSize.y;

        int columnOffset;

        int rowOffset;
        for (int i = 0;
             i < rectChildren.Count;
             i++)
        {
            rowOffset = i / columns;
            columnOffset = i % columns;

            var gridItem = rectChildren[i];

            var xPos = (cellSize.x + spacing.x) * columnOffset + padding.left;
            var yPos = (cellSize.y + spacing.y) * rowOffset + padding.top;

            SetChildAlongAxis(gridItem, 0, xPos, cellSize.x);
            SetChildAlongAxis(gridItem, 1, yPos, cellSize.y);
        }
    }

    public override void CalculateLayoutInputVertical()
    {
    }

    public override void SetLayoutHorizontal()
    {
    }

    public override void SetLayoutVertical()
    {
    }
}