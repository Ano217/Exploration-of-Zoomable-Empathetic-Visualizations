using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;
//using UnityEngine.Networking.PlayerConnection;
using UnityEngine.UIElements;
using Unity.Entities.UniversalDelegates;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(MySceneManager)), CanEditMultipleObjects]
public class MySceneManagerEditor : Editor
{
    private bool _barChartFoldOut;
    private List<bool> _barChartInnerFouldout;

    private string[] _headers;

    private bool _sort1Group;
    private bool _sort2Group;
    private bool _uniformUnitGroup;
    private bool _customAvatarPanelGroup;
    private bool titleFoldout;
    private int _nbTitleFields;
    private bool _imageFoldout;
    private bool _infoFieldFouldout;
    private int _nbInfoField;
    private bool _immersiveViewFoldout;


    // ShapeAssociation
    //Target.sort2Column, Target.sort2Type, Target.sort2TimeRange, Target.sort2NumericalRange
    private string _datapath;
    private string _sort2Column;
    private SortType _sort2Type;
    private TimeRange _sort2TimeRange;
    private List<float> _sort2NumericalRange = new List<float>();
    Dictionary<string, List<DataPoint>> _sortedData;
    Dictionary<string, PrefabHolder> _prefabHolder;
    private List<bool> _fieldsFoldoutGroups = new List<bool>();

    private MySceneManager Target;


    //private SerializedObject _objectSO;
    //private SerializedProperty _objectSOProperty;

    [SerializeField] private SerializedObject[] _objectsSO;
    [SerializeField] private SerializedProperty[] _objectsSOProperties;

    void OnEnable()
    {
        //Debug.Log("On Enable");
        //list = serializedObject.FindProperty("listOfDictionaryKeys");
    }

    private void OnDisable()
    {
        
    }

    public void Awake()
    {
        Target = (MySceneManager)target;
        if (_objectsSO == null)
        {
            _objectsSO = new SerializedObject[50];
            _objectsSOProperties = new SerializedProperty[50];
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        if (_objectsSO == null)
        {
            _objectsSO = new SerializedObject[50];
            _objectsSOProperties = new SerializedProperty[50];
            
        }



        var dataPath = Target.dataPath;
        dataPath = EditorGUILayout.TextField("CVS Path", Target.dataPath);
        serializedObject.Update();
        var changedData = false;
        if (dataPath.Length > 0)
        {
            if(dataPath != Target.dataPath)
            {
                Debug.Log("Changed data");
                Target.dataPath = dataPath;
                Target.makeData();
                _datapath = Target.dataPath;
                Target.barChartDescriptions = new List<SortDescriptionD3>();
                changedData = true;
            }
            if (Target.csvData == null)
            {
                Target.makeData();
            }
            _headers = Target.csvData.getHeadersTab();

            ////////////////////////////
            /// NEW BARCHART SECTION ///
            ////////////////////////////
            if (_barChartInnerFouldout == null || changedData)
            {
                if (Target.barChartDescriptions.Count > 0)
                {
                    _barChartInnerFouldout = new List<bool>();
                    foreach(var d in  Target.barChartDescriptions) _barChartInnerFouldout.Add(false);
                }
                else _barChartInnerFouldout = new List<bool>();
            }
            _barChartFoldOut = EditorGUILayout.Foldout(_barChartFoldOut, "Create sorts category for barchart");
            if(_barChartFoldOut)
            {
                //Debug.Log("Nb descriptions = " + Target.barChartDescriptions.Count);
                if (_barChartInnerFouldout.Count == 0) _barChartInnerFouldout.Add(true);
                _barChartInnerFouldout[0] = EditorGUILayout.Foldout(_barChartInnerFouldout[0], "Default sort");
                if (_barChartInnerFouldout[0])
                {
                    // Fonction qui crée et met à jour le sort
                    createBarChartSortCategory(0);
                    //EditorGUILayout.LabelField("TODO");
                }
                for(int i=1; i<_barChartInnerFouldout.Count; i++)
                {
                    // Add sort selectors
                    _barChartInnerFouldout[i] = EditorGUILayout.Foldout(_barChartInnerFouldout[i], "Sort option n°" + i.ToString());
                    if (_barChartInnerFouldout[i])
                    {
                        // Call function
                        createBarChartSortCategory(i);
                        if (GUILayout.Button("Delete this sort option"))
                        {
                            _barChartInnerFouldout.RemoveAt(i);
                            Target.barChartDescriptions.RemoveAt(i);
                        }
                    }
                }
                if (GUILayout.Button("Add new sort option"))
                {
                    _barChartInnerFouldout.Add(false);
                }

                if (Target.barChartDescriptions.Count > _barChartInnerFouldout.Count)
                {
                    for (int i = Mathf.Max(1, _barChartInnerFouldout.Count); i < Target.barChartDescriptions.Count; i++)
                    {
                        Debug.Log("Remove description " + i);
                        Target.barChartDescriptions.RemoveAt(i);
                    }
                }
            }


            ///////////////////////
            /// BARCHART SECTION //
            ///////////////////////
            /*_sort1Group = EditorGUILayout.Foldout(_sort1Group, "Barchart visualization");
            if (_sort1Group)
            {
                // BARCHART
                // Title
                var titleLabel = new GUIContent("Barchart title:");
                Target.barChartOrganization.title = EditorGUILayout.TextField(titleLabel, Target.barChartOrganization.title);

                EditorGUILayout.LabelField("Default sort:");
                // Sort By
                GUIContent dropDownLabel = new GUIContent("Sort by");
                Target.barChartOrganization.columnID = EditorGUILayout.Popup(dropDownLabel, Target.barChartOrganization.columnID, _headers);
                Target.barChartOrganization.columnName = _headers[Target.barChartOrganization.columnID];

                // Sort Type
                GUIContent sortTypeLabel = new GUIContent("Sort type");
                // public static Enum EnumPopup(GUIContent label, Enum selected, Func<Enum,bool> checkEnabled, bool includeObsolete, params GUILayoutOption[] options);
                Target.barChartOrganization.sortType = (SortType)EditorGUILayout.EnumPopup(sortTypeLabel, Target.barChartOrganization.sortType, sortTypeSort1, false);

                if (Target.barChartOrganization.sortType == SortType.NumericalRange)
                {
                    GUIContent sort1NumericalRangeLabel = new GUIContent("Enter the max value of each category");
                    serializedObject.Update();
                    //EditorGUILayout.PropertyField(serializedObject.FindProperty("sort1NumericalRange"), sort1NumericalRangeLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("barChartOrganization.numericalRangesOptional"), sort1NumericalRangeLabel);
                    serializedObject.ApplyModifiedProperties();
                    // EditorGUILayout.PropertyField(Target.barChartOrganization.numericalRangesOptional, sort1NumericalRangeLabel);
                }
                else if (Target.barChartOrganization.sortType == SortType.TimeRange)
                {
                    GUIContent sort1TimeRangeLabel = new GUIContent("Sort Time By ");
                    Target.barChartOrganization.timeRangeOptional = (TimeRange)EditorGUILayout.EnumPopup(sort1TimeRangeLabel, Target.barChartOrganization.timeRangeOptional);
                }

            }*/

            ////////////////////////////////////////
            /// UNIFORM UNIT VISUALIZATION SECTION//
            ////////////////////////////////////////
            /*_uniformUnitGroup = EditorGUILayout.Foldout(_uniformUnitGroup, "Uniform unit visualization");
            if (_uniformUnitGroup)
            {
                Target.tileCovering = EditorGUILayout.Slider("Tile covering", Target.tileCovering, 0, 1.0f);//EditorGUILayout.FloatField("Tile covering", Target.tileCovering);
                Target.visuPrefab = (GameObject)EditorGUILayout.ObjectField("Uniform visu prefab", Target.visuPrefab, typeof(GameObject));
            }*/


            ////////////////////////////////////////
            // GRANURALIZED VISUALIZATION SECTION //
            ////////////////////////////////////////
            _sort2Group = EditorGUILayout.Foldout(_sort2Group, "Granuralized visualization");
            if (_sort2Group)
            {
                GUIContent dropDownLabel2 = new GUIContent("Sort by");
                Target.humanVisualization.columnID = EditorGUILayout.Popup(dropDownLabel2, Target.humanVisualization.columnID, _headers);
                Target.humanVisualization.columnName = _headers[Target.humanVisualization.columnID];

                GUIContent sort2TypeLabel = new GUIContent("Sort type");
                Target.humanVisualization.sortType = (SortType)EditorGUILayout.EnumPopup(sort2TypeLabel, Target.humanVisualization.sortType);

                if (Target.humanVisualization.sortType == SortType.NumericalRange)
                {
                    GUIContent sort2NumericalRangeLabel = new GUIContent("Enter the max value of each category");
                    serializedObject.Update();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("humanVisualization.numericalRangesOptional"), sort2NumericalRangeLabel);
                    serializedObject.ApplyModifiedProperties();
                }
                else if (Target.humanVisualization.sortType == SortType.TimeRange)
                {
                    GUIContent sort2TimeRangeLabel = new GUIContent("Sort Time By ");
                    Target.humanVisualization.timeRangeOptional = (TimeRange)EditorGUILayout.EnumPopup(sort2TimeRangeLabel, Target.humanVisualization.timeRangeOptional);
                }

                //// AU DESSUS TOUT S'ENREGISTRE    
                // On va plutot vérifier si Target.sort2ShapeAssociation correspond à ce qu'on veut en fonction des valeurs au dessus
                bool identical = true;
                bool remakeAssociation = false;

                if (Target.humanVisualization.sortType != SortType.IndividualAssociation)
                {
                    try
                    {
                        _sortedData = Target.calculateData(Target.csvData.getAllData(), Target.humanVisualization);
                    }
                    catch
                    {
                        Debug.Log("Sort type not compatible with data type");
                    }
                }
                    
                //_sortedData = Target.getSortedData(Target.csvData.getAllData(), Target.humanVisualization.columnName, Target.humanVisualization.sortType, Target.humanVisualization.timeRangeOptional, Target.humanVisualization.numericalRangesOptional);
                
                if (Target.humanVisShapeAssociation != null && Target.humanVisualization.sortType != SortType.IndividualAssociation)
                {
                    if (Target.humanVisShapeAssociation.Count != _sortedData.Keys.Count)
                    {
                        //Debug.Log("!= counts");
                        remakeAssociation = true;
                    }
                    else
                    {
                        foreach (var holder in Target.humanVisShapeAssociation)
                        {
                            if (!_sortedData.ContainsKey(holder.associatedName))
                            {
                                //Debug.Log("Key " + holder.associatedName + " does not belong to the keys");
                                remakeAssociation = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    remakeAssociation = true;
                    //Debug.Log("humanVisShapeAssociation is null");
                }

                //if (_sortedData == null || Target.sort2Column != _sort2Column || Target.sort2Type != _sort2Type || Target.sort2TimeRange != _sort2TimeRange || !identical)
                if (remakeAssociation  && Target.humanVisualization.sortType!=SortType.IndividualAssociation)
                {
                    //_sortedData = Target.getSortedData(Target.csvData.getAllData(), Target.sort2Column, Target.sort2Type, Target.sort2TimeRange, Target.sort2NumericalRange);

                    Target.humanVisShapeAssociation = new List<PrefabHolder>();
                    Debug.Log("Creating new sort2ShapeAssociation List");
                    _sort2Column = Target.humanVisualization.columnName;
                    _sort2Type = Target.humanVisualization.sortType;
                    _sort2TimeRange = Target.humanVisualization.timeRangeOptional;
                    _sort2NumericalRange = new List<float>();
                    foreach (float val in Target.humanVisualization.numericalRangesOptional) { _sort2NumericalRange.Add(val); }


                    foreach (var key in _sortedData.Keys)
                    {
                        PrefabHolder prefHold = new PrefabHolder();// ScriptableObject.CreateInstance<PrefabHolder>();
                        prefHold.associatedName = key;
                        prefHold.prefabs = new List<GameObject>();
                        Target.humanVisShapeAssociation.Add(prefHold);
                    }
                }

                if(Target.humanVisualization.sortType != SortType.IndividualAssociation)
                {
                    //
                    int i = 0;
                    SerializedProperty serializedProperty = serializedObject.FindProperty("sort2ShapeAssociation");
                    foreach (var key in _sortedData.Keys)
                    {
                        if (i < Target.humanVisShapeAssociation.Count && Target.humanVisShapeAssociation[i] != null)
                        {
                            if (Target.humanVisShapeAssociation[i].prefabs == null)
                            {
                                Debug.Log("shapeAssociation is null");
                            }
                            /*else
                            {
                                //Debug.Log("ShapeAssiociation already has prefab");
                                if (Target.humanVisShapeAssociation[i].prefabs.Count > 0)
                                {
                                    Debug.Log("And prefab.count>0");
                                }
                                else
                                {
                                    Debug.Log("But it is empty");
                                }
                            }*/


                            var objectSO = new SerializedObject(Target.humanVisShapeAssociation[i]);
                            objectSO.Update();
                            var listRE = objectSO.FindProperty("prefabs");
                            GUIContent catLabel = new GUIContent(key);
                            EditorGUILayout.PropertyField(listRE, catLabel, true);

                            objectSO.ApplyModifiedProperties();
                            i++;
                        }
                    }
                }
            }


            /////////////////////////////////
            // CUSTOM AVATAR PANEL SECTION //
            /////////////////////////////////
            _customAvatarPanelGroup = EditorGUILayout.Foldout(_customAvatarPanelGroup, "Avatar information panel");
            if (_customAvatarPanelGroup)
            {
                // TITLE
                // Foldout for title
                titleFoldout = EditorGUILayout.Foldout(titleFoldout, "Panel Title");

                var cpt = 0;
                if (Target.title == null || changedData) Target.title = new List<int>();
                else
                {
                    foreach(var value in Target.title)
                    {
                        if (value>-1) cpt++;
                    }
                }
                if (cpt > _nbTitleFields) _nbTitleFields = cpt;   
                if(titleFoldout)
                {
                    _nbTitleFields = EditorGUILayout.IntField(_nbTitleFields);
                    if(Target.title.Count < _nbTitleFields)
                    {
                        for (var i = 0; i < _nbTitleFields; i++) Target.title.Add(- 1);
                    }
                    if(Target.title.Count > _nbTitleFields)
                    {
                        for(var i= _nbTitleFields; i< Target.title.Count; i++)
                        {
                            Target.title[i] = -1;
                        }
                    }
                    for(var i =0; i < _nbTitleFields;i++)
                    {
                        var nb = i + 1;
                        GUIContent inputLabel = new GUIContent(nb.ToString());
                        Target.title[i] = EditorGUILayout.Popup(inputLabel, Target.title[i], _headers);
                    }
                }

                // IMAGE
                _imageFoldout = EditorGUILayout.Foldout(_imageFoldout, "Image");
                if (_imageFoldout)
                {
                    var toggleLabel = new GUIContent("Add an image:");
                    Target.displayImage = EditorGUILayout.Toggle(toggleLabel, Target.displayImage);
                    if (Target.displayImage)
                    {
                        var linkLabel = new GUIContent("Link to image:");
                        Target.imageID = EditorGUILayout.Popup(linkLabel, Target.imageID, _headers);
                        var typeLinkLabel = new GUIContent("Type of link:");
                        Target.imageSource = (ImageSource)EditorGUILayout.EnumPopup(typeLinkLabel, Target.imageSource);
                    }
                }

                // INFO FIELDS
                var infoFieldCpt = 0;
                if (Target.infoFields == null || changedData) Target.infoFields = new List<PanelInfoField>();
                foreach (var value in Target.infoFields)
                {
                    if (value.dataIdx != -1) infoFieldCpt++;
                }
                if(infoFieldCpt > _nbInfoField) _nbInfoField= infoFieldCpt;

                // Information Fields Fouldout
                _infoFieldFouldout = EditorGUILayout.Foldout(_infoFieldFouldout, "Information fields");
                if (_infoFieldFouldout)
                {
                    var nbFieldsLabel = new GUIContent("Number of fields");
                    _nbInfoField = EditorGUILayout.IntField(nbFieldsLabel, _nbInfoField);

                    if (Target.infoFields.Count < _nbInfoField)
                    {
                        for(var i = Target.infoFields.Count; i < _nbInfoField; i++)
                        {
                            Target.infoFields.Add(new PanelInfoField { dataIdx=-1});
                        }
                    }
                    if(Target.infoFields.Count > _nbInfoField)
                    {
                        for(var i= _nbInfoField; i< Target.infoFields.Count ; i++)
                        {
                            Target.infoFields[i] = new PanelInfoField { dataIdx=-1};
                        }
                    }

                    if(_fieldsFoldoutGroups.Count < _nbInfoField)
                    {
                        for(var i=_fieldsFoldoutGroups.Count; i< _nbInfoField; i++)
                        {
                            _fieldsFoldoutGroups.Add(false);
                        }
                    }

                    for (var i = 0; i < _nbInfoField; i++)
                    {
                        _fieldsFoldoutGroups[i] = EditorGUILayout.Foldout(_fieldsFoldoutGroups[i], "Field n°" + i);
                        if (_fieldsFoldoutGroups[i])
                        {
                            var fieldLabelLabel = new GUIContent("Field Label:");
                            PanelInfoField panelInfoField = new PanelInfoField();
                            /*Target.infoFields[i].label = EditorGUILayout.TextField(fieldLabelLabel, Target.infoFields[i].label);

                            var fieldContentLabel = new GUIContent("Field content:");
                            Target.infoFields[i].dataIdx = EditorGUILayout.Popup(fieldContentLabel, Target.infoFields[i].dataIdx, headers);

                            var fieldTypeLabel = new GUIContent("Field type:");
                            Target.infoFields[i].fieldType = (InfoFieldType)EditorGUILayout.EnumPopup(fieldTypeLabel, Target.infoFields[i].fieldType);*/
                            panelInfoField.label = EditorGUILayout.TextField(fieldLabelLabel, Target.infoFields[i].label);

                            var fieldContentLabel = new GUIContent("Field content:");
                            panelInfoField.dataIdx = EditorGUILayout.Popup(fieldContentLabel, Target.infoFields[i].dataIdx, _headers);

                            var fieldTypeLabel = new GUIContent("Field type:");
                            panelInfoField.fieldType = (InfoFieldType)EditorGUILayout.EnumPopup(fieldTypeLabel, Target.infoFields[i].fieldType);
                            Target.infoFields[i] = panelInfoField;
                        }
                    }
                }
            }


            //////////////////////
            // 360 VIEW SECTION //
            //////////////////////
            _immersiveViewFoldout = EditorGUILayout.Foldout(_immersiveViewFoldout, "360 View");
            if (_immersiveViewFoldout)
            {
                var visuTypeLabel = new GUIContent("360 visu type:");
                Target.immersiveVisu = (Type360Visu)EditorGUILayout.EnumPopup(visuTypeLabel, Target.immersiveVisu);
                if (Target.immersiveVisu == Type360Visu.Image360)
                {
                    var linkFieldLabel = new GUIContent("Image Link:");
                    Target.immersiveLocationField = EditorGUILayout.Popup(linkFieldLabel, Target.immersiveLocationField, _headers);

                    var imageTypeLabel = new GUIContent("Type of link:");
                    Target.typeImmersiveImage = (ImageSource) EditorGUILayout.EnumPopup(imageTypeLabel, Target.typeImmersiveImage);
                }
                else if (Target.immersiveVisu == Type360Visu.UnityScene)
                {
                    var toggleLabel = new GUIContent("Single scene:");
                    Target.sceneIsUnique = EditorGUILayout.Toggle(toggleLabel, Target.sceneIsUnique);

                    if (Target.sceneIsUnique)
                    {
                        // Same scene for all avatars
                        var sceneNameLabel = new GUIContent("Scene name:");
                        Target.immesiveSceneName = EditorGUILayout.TextField(sceneNameLabel, Target.immesiveSceneName);
                    }
                    else
                    {
                        // Different scene between avatars
                        var sceneFieldLabel = new GUIContent("Scene reference:");
                        Target.immersiveLocationField = EditorGUILayout.Popup(sceneFieldLabel, Target.immersiveLocationField, _headers);
                    }

                    var coordinatesLabel = new GUIContent("Coordinates:");
                    Target.coordinatesField = EditorGUILayout.Popup(coordinatesLabel, Target.coordinatesField, _headers);
                }
            }
                
        }
        EditorUtility.SetDirty(Target);
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
    }

    public bool sortTypeSort1(Enum enumValue)
    {
        switch ((SortType)enumValue)
        {
            case SortType.Nominal:
                return true;
            case SortType.NumericalRange:
                return true;
            case SortType.TimeRange:
                return true;
            case SortType.IndividualAssociation: return false;
            default: return false;
        }
    }

    public void createBarChartSortCategory(int sortNb)
    {
        Debug.Log("Barchart description n°" + sortNb);
        if (Target.barChartDescriptions == null)
        {
            Debug.Log("Barchart description was null");
            Target.barChartDescriptions = new List<SortDescriptionD3>();
        }
        if(Target.barChartDescriptions.Count <= sortNb)
        {
            for (int i = Target.barChartDescriptions.Count; i < sortNb+2; i++)
            {
                Debug.Log("Add new description");
                Target.barChartDescriptions.Add(new SortDescriptionD3 { });
            }
        }
        
        SortDescriptionD3 sortDescriptionD3;
        if (Target.barChartDescriptions.Count > sortNb) sortDescriptionD3 = Target.barChartDescriptions[sortNb];
        else sortDescriptionD3 = new SortDescriptionD3();

        // Title
        var titleLabel = new GUIContent("Barchart title:");
        sortDescriptionD3.title = EditorGUILayout.TextField(titleLabel, sortDescriptionD3.title);

        // Sort By
        GUIContent dropDownLabel = new GUIContent("Sort by");
        sortDescriptionD3.columnID = EditorGUILayout.Popup(dropDownLabel, sortDescriptionD3.columnID, _headers);
        sortDescriptionD3.columnName = _headers[sortDescriptionD3.columnID];

        // Sort Type
        GUIContent sortTypeLabel = new GUIContent("Sort type");
        sortDescriptionD3.sortType = (SortType)EditorGUILayout.EnumPopup(sortTypeLabel, sortDescriptionD3.sortType, sortTypeSort1, false);



        if (sortDescriptionD3.sortType == SortType.NumericalRange)
        {
            GUIContent sort1NumericalRangeLabel = new GUIContent("Enter the max value of each category");
            
            int nbRange;
            if (sortDescriptionD3.numericalRangesOptional == null) nbRange = 0;
            else nbRange = sortDescriptionD3.numericalRangesOptional.Length;

            // Nb of ranges
            GUIContent nbRangeLabel = new GUIContent("Number of categories: ");
            nbRange = EditorGUILayout.IntField(nbRangeLabel, nbRange);

            // Range values
            EditorGUILayout.LabelField("Enter the max value of each category:");
            float[] ranges;
            if (sortDescriptionD3.numericalRangesOptional != null && nbRange == sortDescriptionD3.numericalRangesOptional.Length) ranges = sortDescriptionD3.numericalRangesOptional;
            else ranges = new float[nbRange];

            for(int i=0; i<ranges.Length; i++)
            {
                GUIContent lab = new GUIContent("Category n°" + (i + 1) + ":");
                ranges[i] = EditorGUILayout.FloatField(lab, ranges[i]);
            }

            sortDescriptionD3.numericalRangesOptional = ranges;
        }
        else if (sortDescriptionD3.sortType == SortType.TimeRange)
        {
            GUIContent sort1TimeRangeLabel = new GUIContent("Sort Time By ");
            sortDescriptionD3.timeRangeOptional = (TimeRange)EditorGUILayout.EnumPopup(sort1TimeRangeLabel, sortDescriptionD3.timeRangeOptional);
        }
        Target.barChartDescriptions[sortNb] = sortDescriptionD3;
    }
}
