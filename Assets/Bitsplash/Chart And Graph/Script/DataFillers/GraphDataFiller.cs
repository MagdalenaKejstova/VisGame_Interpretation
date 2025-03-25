#define Graph_And_Chart_PRO
using UnityEngine;
using System.Collections;
using System;
using ChartAndGraph;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

#if UNITY_EDITOR
using UnityEditor;

using UnityEditor.Graphs;
#endif

#if UNITY_2018_1_OR_NEWER
using System.IO;
using UnityEngine.Networking;
#endif

public enum Axis
{
    Vertical,
    Horizontal,
    X,
    Y
}

public class StackedCategoryData
{
    public string Name { get; set; }
    public List<DoubleVector2> DataPoints = new();
    public List<DoubleVector2> StackedDataPoints = new();
}

public class GraphDataFiller : MonoBehaviour
{
    [Serializable]
    public enum DataType
    {
        VectorArray,
        ArrayForEachElement,
        ObjectArray,
    }

    public enum DocumentFormat
    {
        XML,
        JSON
    }

    public enum VectorFormat
    {
        X_Y,
        Y_X,
        X_Y_SIZE,
        Y_X_SIZE,
        SIZE_X_Y,
        SIZE_Y_X,
        X_Y_GAP_SIZE,
        Y_X_GAP_SIZE
    }

    class VectorFormatData
    {
        public int X, Y, Size, Length;

        public VectorFormatData(int x, int y, int size, int length)
        {
            X = x;
            Y = y;
            Size = size;
            Length = length;
        }
    }

    [Serializable]
    public class CategoryData
    {
        public bool Enabled = true;

        [ChartFillerEditor(DataType.ObjectArray)]
        [ChartFillerEditor(DataType.ArrayForEachElement)]
        [ChartFillerEditorAttribute(DataType.VectorArray)]
        public string Name;

        /// <summary>
        /// The way the data is stored in the object
        /// </summary>
        public DataType DataType;

        [ChartFillerEditorAttribute(DataType.VectorArray)]
        public VectorFormat DataFormat;

        /// <summary>
        /// the amount of items to skip after each dataformat instance
        /// </summary>
        [ChartFillerEditorAttribute(DataType.VectorArray)]
        public int Skip = 0;

        /// <summary>
        /// if this is empty then DataObjectName is not relative
        /// </summary>
        [ChartFillerEditor(DataType.ObjectArray)] [ChartFillerEditor(DataType.ArrayForEachElement)]
        public string ParentObjectName;

        [ChartFillerEditorAttribute(DataType.VectorArray)]
        public string DataObjectName;


        [ChartFillerEditor(DataType.ObjectArray)] [ChartFillerEditor(DataType.ArrayForEachElement)]
        public string XDataObjectName;

        [ChartFillerEditor(DataType.ObjectArray)] [ChartFillerEditor(DataType.ArrayForEachElement)]
        public string YDataObjectName;

        [ChartFillerEditor(DataType.ObjectArray)] [ChartFillerEditor(DataType.ArrayForEachElement)]
        public string SizeDataObjectName;

        /// <summary>
        /// set to empty null or "none" for numbers. Set to a date format for a date :  https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
        /// </summary>
        [ChartFillerEditor(DataType.ObjectArray)] [ChartFillerEditor(DataType.ArrayForEachElement)]
        public string XDateFormat = "";

        [ChartFillerEditor(DataType.ObjectArray)] [ChartFillerEditor(DataType.ArrayForEachElement)]
        public string YDateFormat = "";
    }

    public GraphChartBase GraphObject;

    /// <summary>
    /// assign a graph chart prefab that will be used to copy category data
    /// </summary>
    public GraphChartBase CategoryPrefab;

    public GameObject AxesPrefab;

    public DocumentFormat Format;
    public string RemoteUrl;
    public string localPath;
    public bool FillOnStart;
    public bool StackCategories;
    public bool CenterCategories;
    public bool CurveCategories;
    public int SegmentsPerCurve = 10;
    public string DateDisplayFormat = "";
    public CategoryData[] Categories = new CategoryData[0];

    private object[] mCategoryVisualStyle;

    delegate void CategoryLoader(CategoryData data);

    private Dictionary<DataType, CategoryLoader> mLoaders;
    private static Dictionary<VectorFormat, VectorFormatData> mVectorFormats;
    private ChartParser mParser;

    public List<StackedCategoryData> stackedCategories = new();

    static GraphDataFiller()
    {
        CreateVectorFormats();
    }

    void EnsureCreateDataTypes()
    {
        if (mLoaders != null)
            return;
        mLoaders = new Dictionary<DataType, CategoryLoader>();
        mLoaders[DataType.ArrayForEachElement] = LoadArrayForEachElement;
        mLoaders[DataType.ObjectArray] = LoadObjectArray;
        mLoaders[DataType.VectorArray] = LoadVectorArray;
    }

    static void CreateVectorFormats()
    {
        mVectorFormats = new Dictionary<VectorFormat, VectorFormatData>();
        mVectorFormats[VectorFormat.X_Y] = new VectorFormatData(0, 1, -1, 2);
        mVectorFormats[VectorFormat.Y_X] = new VectorFormatData(1, 0, -1, 2);
        mVectorFormats[VectorFormat.X_Y_SIZE] = new VectorFormatData(0, 1, 2, 3);
        mVectorFormats[VectorFormat.Y_X_SIZE] = new VectorFormatData(1, 0, 2, 3);
        mVectorFormats[VectorFormat.SIZE_X_Y] = new VectorFormatData(1, 2, 0, 3);
        mVectorFormats[VectorFormat.SIZE_Y_X] = new VectorFormatData(2, 1, 0, 3);
        mVectorFormats[VectorFormat.X_Y_GAP_SIZE] = new VectorFormatData(0, 1, 3, 4);
        mVectorFormats[VectorFormat.Y_X_GAP_SIZE] = new VectorFormatData(1, 0, 3, 4);
    }

    private double ParseItem(string item, string format)
    {
        if (String.IsNullOrEmpty(format) || format.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            double outputValueDouble;
            double.TryParse(string.Format(CultureInfo.InvariantCulture, "{0}", item), NumberStyles.Any,
                CultureInfo.InvariantCulture, out outputValueDouble);
            return outputValueDouble;
        }

        return ChartDateUtility.DateToValue(DateTime.ParseExact(item, format, CultureInfo.InvariantCulture));
    }

    void LoadArrayForEachElement(CategoryData data)
    {
        GraphChartBase graph = GraphObject.GetComponent<GraphChartBase>();
        if (mParser.SetPathRelativeTo(data.ParentObjectName) == false)
        {
            Debug.LogWarning("Object " + data.ParentObjectName + " does not exist in the document");
            return;
        }

        var xObj = mParser.GetObject(data.XDataObjectName);
        var yObj = mParser.GetObject(data.YDataObjectName);
        object sizeObj = null;
        if (String.IsNullOrEmpty(data.SizeDataObjectName) == false)
            sizeObj = mParser.GetObject(data.SizeDataObjectName);
        int length = Math.Min(mParser.GetArraySize(xObj), mParser.GetArraySize(yObj));
        if (sizeObj != null)
            length = Math.Min(length, mParser.GetArraySize(sizeObj));
        try
        {
            for (int i = 0; i < length; i++)
            {
                double x = ParseItem(mParser.GetItem(xObj, i), data.XDateFormat);
                double y = ParseItem(mParser.GetItem(yObj, i), data.YDateFormat);
                double pointSize = -1;
                if (sizeObj != null)
                    pointSize = double.Parse(mParser.GetItem(sizeObj, i));
                graph.DataSource.AddPointToCategory(data.Name, x, y, pointSize);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Data for category " + data.Name +
                             " does not match the specified format. Ended with exception : " + e.ToString());
        }
    }

    // Sum of Y-values on each X-position of the graph
    private List<double> GetCompoundSum(List<List<double>> valueLists)
    {
        var compoundSum = Enumerable.Repeat(0d, valueLists.First().Count).ToList();
        foreach (var valueList in valueLists)
        {
            compoundSum = compoundSum.Zip(valueList, (num1, num2) => num1 + num2).ToList();
        }

        return compoundSum;
    }

    private void FillPaddingCategory()
    {
        GraphChartBase graph = GraphObject.GetComponent<GraphChartBase>();
        
        List<List<double>> allCategoriesYValues = new();
        foreach (var stackedCategory in stackedCategories)
        {
            var isEnabled = Categories.ToList().Find(cat => cat.Name == stackedCategory.Name).Enabled;
            if (isEnabled)
            {
                allCategoriesYValues.Add(stackedCategory.DataPoints.Select(point => point.y).ToList());
            }
        }

        var compoundSum = GetCompoundSum(allCategoriesYValues);
        var graphYRange = graph.DataSource.VerticalViewSize;
        var paddingYValues = compoundSum.Select(value => (graphYRange - value) / 2).ToList();

        var paddingCategory = stackedCategories.Find(cat => cat.Name == "Padding");

        for (int i = 0; i < paddingCategory.DataPoints.Count; i++)
        {
            var dataPoint = paddingCategory.DataPoints[i];
            dataPoint.y = paddingYValues[i];
            paddingCategory.DataPoints[i] = dataPoint;
            paddingCategory.StackedDataPoints[i] = dataPoint;
        }
    }

    void AddCategoriesToGraph()
    {
        GraphChartBase graph = GraphObject.GetComponent<GraphChartBase>();

        // Add largest categories first in order not to hide the smaller ones behind
        for (int i = Categories.Length - 1; i >= 0; i--)
        {
            if (!Categories[i].Enabled)
            {
                continue;
            }

            var data = Categories[i];
            var stackedCategory = stackedCategories.Find(cat => cat.Name == data.Name);

            var parent = mParser.GetObject(data.ParentObjectName);
            if (parent == null)
            {
                Debug.LogWarning("Object " + data.ParentObjectName + " does not exist in the document");
                return;
            }

            if (CurveCategories)
            {
                graph.DataSource.ClearAndMakeBezierCurve(stackedCategory.Name);
                var curvedCategoryData = graph.DataSource.GetCategoryData(stackedCategory.Name);
                curvedCategoryData.SegmentsPerCurve = SegmentsPerCurve;
            }
            else
            {
                graph.DataSource.ClearAndMakeLinear(stackedCategory.Name);
            }

            for (int j = 0; j < stackedCategory.StackedDataPoints.Count; j++)
            {
                object item = mParser.GetItemObject(parent, j);
                var x = stackedCategory.StackedDataPoints[j].x;
                var y = stackedCategory.StackedDataPoints[j].y;
                double pointSize = -1;
                if (String.IsNullOrEmpty(data.SizeDataObjectName) == false)
                    pointSize = double.Parse(mParser.GetChildObjectValue(item, data.SizeDataObjectName));

                if (CurveCategories)
                {
                    if (j == 0)
                    {
                        graph.DataSource.SetCurveInitialPoint(stackedCategory.Name, x, y, pointSize);
                    }
                    else
                    {
                        graph.DataSource.AddLinearCurveToCategory(stackedCategory.Name, new DoubleVector2(x, y),
                            pointSize);
                    }
                }
                else
                {
                    graph.DataSource.AddPointToCategory(data.Name, x, y, pointSize);
                }

                if (CurveCategories)
                {
                    graph.DataSource.MakeCurveCategorySmooth(stackedCategory.Name);
                }
            }
        }
    }

    // TODO remove original once new implementation has been tested
    // void LoadObjectArray(CategoryData data)
    // {
    //     GraphChartBase graph = GraphObject.GetComponent<GraphChartBase>();
    //     var parent = mParser.GetObject(data.ParentObjectName);
    //     if (parent == null)
    //     {
    //         Debug.LogWarning("Object " + data.ParentObjectName + " does not exist in the document");
    //         return;
    //     }
    //
    //     int length = mParser.GetArraySize(parent);
    //
    //     var stackedCategory = stackedCategories.Find(cat => cat.Name == data.Name);
    //     if (stackedCategory == null)
    //     {
    //         var newStackedCategory = new StackedCategoryData();
    //         newStackedCategory.Name = data.Name;
    //         for (int i = 0; i < length; i++)
    //         {
    //             newStackedCategory.DataPoints.Add(new DoubleVector2(0, 0));
    //             newStackedCategory.StackedDataPoints.Add(new DoubleVector2(0, 0));
    //         }
    //
    //         stackedCategories.Add(newStackedCategory);
    //         stackedCategory = newStackedCategory;
    //     }
    //
    //
    //     var currentCategoryIndex = stackedCategories.Select(cat => cat.Name).ToList().IndexOf(data.Name);
    //     try
    //     {
    //         for (int i = 0; i < length; i++)
    //         {
    //             object item = mParser.GetItemObject(parent, i);
    //             double x = ParseItem(mParser.GetChildObjectValue(item, data.XDataObjectName), data.XDateFormat);
    //             double y = ParseItem(mParser.GetChildObjectValue(item, data.YDataObjectName), data.YDateFormat);
    //             var dataPoint = stackedCategory.DataPoints[i];
    //             dataPoint.x = x;
    //             dataPoint.y = y;
    //             stackedCategory.DataPoints[i] = dataPoint;
    //
    //             if (StackCategories)
    //             {
    //                 // Add all subcategories to current value
    //                 for (int j = 0; j < currentCategoryIndex; j++)
    //                 {
    //                     var categoryBelow = Categories.ToList().Find(cat => cat.Name == stackedCategories[j].Name);
    //                     if (categoryBelow.Enabled)
    //                     {
    //                         y += stackedCategories[j].DataPoints[i].y;
    //                     }
    //                 }
    //             }
    //
    //             dataPoint.y = y;
    //             stackedCategory.StackedDataPoints[i] = dataPoint;
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         Debug.LogWarning("Data for category " + data.Name +
    //                          " does not match the specified format. Ended with exception : " + e.ToString());
    //     }
    // }

    void PerformCategoryStacking()
    {
        for (int currentCategoryIndex = 0; currentCategoryIndex < stackedCategories.Count; currentCategoryIndex++)
        {
            var currentCategory = stackedCategories[currentCategoryIndex];

            for (int dataPointIndex = 0; dataPointIndex < currentCategory.DataPoints.Count; dataPointIndex++)
            {
                // Add all subcategories to current value
                for (int subcategoryIndex = 0; subcategoryIndex < currentCategoryIndex; subcategoryIndex++)
                {
                    var subCategory = Categories.ToList()
                        .Find(cat => cat.Name == stackedCategories[subcategoryIndex].Name);
                    if (subCategory.Enabled)
                    {
                        var subCategoryValue = stackedCategories[subcategoryIndex].DataPoints[dataPointIndex].y;
                        var stackedDataPoint = currentCategory.StackedDataPoints[dataPointIndex];
                        stackedDataPoint.y += subCategoryValue;
                        currentCategory.StackedDataPoints[dataPointIndex] = stackedDataPoint;
                    }
                }
            }
        }
    }

    void LoadObjectArray(CategoryData data)
    {
        GraphChartBase graph = GraphObject.GetComponent<GraphChartBase>();
        var parent = mParser.GetObject(data.ParentObjectName);
        if (parent == null)
        {
            Debug.LogWarning("Object " + data.ParentObjectName + " does not exist in the document");
            return;
        }

        int length = mParser.GetArraySize(parent);

        var stackedCategory = stackedCategories.Find(cat => cat.Name == data.Name);
        if (stackedCategory == null)
        {
            var newStackedCategory = new StackedCategoryData();
            newStackedCategory.Name = data.Name;
            for (int i = 0; i < length; i++)
            {
                newStackedCategory.DataPoints.Add(new DoubleVector2(0, 0));
                newStackedCategory.StackedDataPoints.Add(new DoubleVector2(0, 0));
            }

            stackedCategories.Add(newStackedCategory);
            stackedCategory = newStackedCategory;
        }

        try
        {
            for (int i = 0; i < length; i++)
            {
                object item = mParser.GetItemObject(parent, i);
                double x = ParseItem(mParser.GetChildObjectValue(item, data.XDataObjectName), data.XDateFormat);
                double y = ParseItem(mParser.GetChildObjectValue(item, data.YDataObjectName), data.YDateFormat);
                var dataPoint = stackedCategory.DataPoints[i];
                dataPoint.x = x;
                dataPoint.y = y;
                stackedCategory.DataPoints[i] = dataPoint;
                stackedCategory.StackedDataPoints[i] = dataPoint;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Data for category " + data.Name +
                             " does not match the specified format. Ended with exception : " + e.ToString());
        }
    }

    void LoadVectorArray(CategoryData data)
    {
        GraphChartBase graph = GraphObject.GetComponent<GraphChartBase>();
        var obj = mParser.GetObject(data.DataObjectName);
        int size = mParser.GetArraySize(obj);
        VectorFormatData formatData = mVectorFormats[data.DataFormat];
        if (size < 0) // this is not an array , show warning
        {
            Debug.LogWarning("DataType " + data.DataType + " does not match category " + data.Name);
            return;
        }

        int itemLength = data.Skip + formatData.Length;
        try
        {
            for (int i = 0; i < size; i += itemLength)
            {
                double x = ParseItem(mParser.GetItem(obj, i + formatData.X), data.XDateFormat);
                double y = ParseItem(mParser.GetItem(obj, i + formatData.Y), data.YDateFormat);
                double pointSize = -1;
                if (formatData.Size >= 0)
                    pointSize = double.Parse(mParser.GetItem(obj, i + formatData.Size));
                graph.DataSource.AddPointToCategory(data.Name, x, y, pointSize);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Data for category " + data.Name +
                             " does not match the specified format. Ended with exception : " + e.ToString());
        }
    }

    void Start()
    {
        if (FillOnStart)
            Fill();
    }

    public void Fill()
    {
        if (StackCategories)
            stackedCategories = new List<StackedCategoryData>();
        Fill(null);
    }

    public void Fill(WWWForm postData)
    {
        StartCoroutine(GetData(postData));
    }

    void LoadAxesVisualStyle(GraphChartBase graph)
    {
        var sourceVerticalAxis = AxesPrefab.GetComponent<VerticalAxis>();
        var targetVerticalAxis = graph.GetComponent<VerticalAxis>();
        #if UNITY_EDITOR
        Undo.RecordObject(targetVerticalAxis, "ReplaceComponent");
        EditorUtility.CopySerialized(sourceVerticalAxis, targetVerticalAxis);
        EditorUtility.SetDirty(targetVerticalAxis);
        #endif

        var sourceHorizontalAxis = AxesPrefab.GetComponent<HorizontalAxis>();
        var targetHorizontalAxis = graph.GetComponent<HorizontalAxis>();
        #if UNITY_EDITOR
        Undo.RecordObject(targetHorizontalAxis, "ReplaceComponent");
        EditorUtility.CopySerialized(sourceHorizontalAxis, targetHorizontalAxis);
        EditorUtility.SetDirty(targetHorizontalAxis);
        #endif
    }

    public UnityEngine.Color GetAxisColor(Axis axis)
    {
        GraphChartBase graph = GraphObject.GetComponent<GraphChartBase>();
        switch (axis)
        {
            case Axis.Vertical:
            case Axis.X:
                var verticalAxis = AxesPrefab.GetComponent<VerticalAxis>();
                var verticalAxisColor = verticalAxis.MainDivisions.Material.GetColor("_Color");
                return verticalAxisColor;
            case Axis.Horizontal:
            case Axis.Y:
                var horizontalAxis = AxesPrefab.GetComponent<HorizontalAxis>();
                var horizontalAxisColor = horizontalAxis.MainDivisions.Material.GetColor("_Color");
                return horizontalAxisColor;
            default:
                return new UnityEngine.Color(0,0,0);
        }
    }
    
    public void SetAxisColor(Axis axis, UnityEngine.Color color)
    {
        GraphChartBase graph = GraphObject.GetComponent<GraphChartBase>();
        switch (axis)
        {
            case Axis.Vertical:
            case Axis.X:
                var sourceVerticalAxis = AxesPrefab.GetComponent<VerticalAxis>();
                sourceVerticalAxis.MainDivisions.Material.SetColor("_Color", color);
                var targetVerticalAxis = graph.GetComponent<VerticalAxis>();
                #if UNITY_EDITOR
                Undo.RecordObject(targetVerticalAxis, "ReplaceComponent");
                EditorUtility.CopySerialized(sourceVerticalAxis, targetVerticalAxis);
                EditorUtility.SetDirty(targetVerticalAxis);
                #endif
                break;
            case Axis.Horizontal:
            case Axis.Y:
                var sourceHorizontalAxis = AxesPrefab.GetComponent<HorizontalAxis>();
                sourceHorizontalAxis.MainDivisions.Material.SetColor("_Color", color);
                var targetHorizontalAxis = graph.GetComponent<HorizontalAxis>();
                #if UNITY_EDITOR
                Undo.RecordObject(targetHorizontalAxis, "ReplaceComponent");
                EditorUtility.CopySerialized(sourceHorizontalAxis, targetHorizontalAxis);
                EditorUtility.SetDirty(targetHorizontalAxis);
                #endif
                break;
        }
    }
    
    public void UpdateVisualStyles()
    {
        GraphChartBase graph = GraphObject.GetComponent<GraphChartBase>();
        LoadCategoryVisualStyle(graph);
        LoadAxesVisualStyle(graph);
        var categoryNames = graph.DataSource.CategoryNames.ToList();
        graph.DataSource.StartBatch();
        for (int i = 0; i < categoryNames.Count(); i++)
        {
            int visualIndex = Math.Min(i, mCategoryVisualStyle.Length - 1);
            object visualStyle = mCategoryVisualStyle[visualIndex];
            graph.DataSource.RestoreCategory(categoryNames[i], visualStyle);
        }
        graph.DataSource.EndBatch();
        
    }
    
    void LoadCategoryVisualStyle(GraphChartBase graph)
    {
        var prefab = CategoryPrefab;
        if (prefab == null)
        {
            if (graph is GraphChart)
                prefab = ((GameObject)Resources.Load("Chart And Graph/DefualtGraphCategoryStyle2D"))
                    .GetComponent<GraphChartBase>();
            else
                prefab = ((GameObject)Resources.Load("Chart And Graph/DefualtGraphCategoryStyle3D"))
                    .GetComponent<GraphChartBase>(); // load default
        }

        if (prefab == null)
            Debug.LogError("missing resources for graph and chart, please reimport the package");
        else
            mCategoryVisualStyle = prefab.DataSource.StoreAllCategoriesinOrder();
    }

    public void ApplyData(string text)
    {
        GraphChartBase graph = GraphObject.GetComponent<GraphChartBase>();
        if (Format == DocumentFormat.JSON)
            mParser = new JsonParser(text);
        else
            mParser = new XMLParser(text);

        LoadCategoryVisualStyle(graph);
        LoadAxesVisualStyle(graph);
        EnsureCreateDataTypes();
        if (mCategoryVisualStyle.Length == 0)
        {
            Debug.LogWarning("no visual styles defeind for GraphDataFiller, aborting");
            return;
        }

        if (mCategoryVisualStyle.Length < Categories.Length)
            Debug.LogWarning("not enough visual styles in GraphDataFiller");

        graph.DataSource.StartBatch();
        for (int i = 0; i < Categories.Length; i++)
        {
            var cat = Categories[i];
            if (cat.Enabled == false)
                continue;
            int visualIndex = Math.Min(i, mCategoryVisualStyle.Length - 1);
            object visualStyle = mCategoryVisualStyle[visualIndex];

            if (graph.DataSource.HasCategory(cat.Name))
                graph.DataSource.RemoveCategory(cat.Name);
            graph.DataSource.AddCategory(cat.Name, null, 0, new MaterialTiling(), null, false, null, 0);
            graph.DataSource.RestoreCategory(cat.Name,
                visualStyle); // set the visual style of the category to the one in the prefab
            var loader = mLoaders[cat.DataType]; // find the loader based on the data type
            loader(cat); // load the category data
        }

        if (CenterCategories)
        {
            FillPaddingCategory();
        }

        if (StackCategories)
        {
            PerformCategoryStacking();
        }


        AddCategoriesToGraph();

        // Stacked categories are filled in reverse to not hide the smaller areas with the bigger ones
        if (StackCategories)
        {
            var categoryNames = graph.DataSource.CategoryNames.ToList();
            var categoriesCount = categoryNames.Count();
            for (int i = 0; i < categoriesCount; i++)
            {
                var category = categoryNames[i];
                graph.DataSource.SetCategoryViewOrder(category, categoriesCount - i - 1);
            }
        }

        if (!string.IsNullOrEmpty(DateDisplayFormat))
        {
            graph.CustomDateTimeFormat = (date) => { return date.ToString(DateDisplayFormat); };
        }

        graph.DataSource.EndBatch();
    }
#if UNITY_2018_1_OR_NEWER
    UnityWebRequest CreateRequest(WWWForm postData)
    {
        if (postData == null)
            return UnityWebRequest.Get(RemoteUrl);
        return UnityWebRequest.Post(RemoteUrl, postData);
    }

    IEnumerator GetData(WWWForm postData)
    {
        bool isLoadingLocal = !string.IsNullOrEmpty(localPath);
        if (isLoadingLocal)
        {
            // try
            // {
                //string text = File.ReadAllText(localPath);
                TextAsset jsonData = Resources.Load<TextAsset>(localPath);
                if (jsonData != null) {
                ApplyData(jsonData.text);
            } else {
                Debug.LogError("Failed to load data from Resources at path: " + localPath);
            }
            // }
            // catch (Exception ex)
            // {
            //     Debug.Log($"An error occurred when when trying to parse data from local path: {ex.Message}");
            // }
        }
        else
        {
            using (UnityWebRequest webRequest = CreateRequest(postData))
            {
                yield return webRequest.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
                if (webRequest.result != UnityWebRequest.Result.Success)
#else
            if (webRequest.isNetworkError)
#endif
                    Debug.LogError("Graph Data Filler : URL request failed ," + webRequest.error);
                else
                {
                    try
                    {
                        string text = webRequest.downloadHandler.text;
                        ApplyData(text);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(
                            "Graph Data Filler : Invalid document format, please check your settings , with exception " +
                            e.ToString());
                    }
                }
            }
        }
    }
#else
    IEnumerator GetData(WWWForm postData)
    {
        WWW request;
        if (postData != null)
        {
            request = new WWW(RemoteUrl, postData);
        }
        else
            request = new WWW(RemoteUrl);
        yield return request;
        if (String.IsNullOrEmpty(request.error))
        {
            try
            {
                string text = request.text;
                ApplyData(text);
            }
            catch (Exception e)
            {
                Debug.LogError("Graph Data Filler : Invalid document format, please check your settings , with exception " + e.ToString());
            }
        }
        else
        {
            Debug.LogError("Graph Data Filler : URL request failed ," + request.error);
        }
    }
#endif
    void Update()
    {
    }
}