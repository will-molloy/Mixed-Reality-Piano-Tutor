using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace UnityEditor.PostProcessing
{
    [PostProcessingModelEditor(typeof(ColorGradingModel))]
    public class ColorGradingModelEditor : PostProcessingModelEditor
    {
        // Neutral tonemapping curve helper
        private const int k_CurveResolution = 24;
        private const float k_NeutralRangeX = 2f;
        private const float k_NeutralRangeY = 1f;

        private static readonly GUIContent[] s_Tonemappers =
        {
            new GUIContent("None"),
            new GUIContent("Filmic (ACES)"),
            new GUIContent("Neutral")
        };

        private static readonly GUIContent[] s_Curves =
        {
            new GUIContent("YRGB"),
            new GUIContent("Hue VS Hue"),
            new GUIContent("Hue VS Sat"),
            new GUIContent("Sat VS Sat"),
            new GUIContent("Lum VS Sat")
        };

        private static Material s_MaterialSpline;
        private BasicSettings m_Basic;
        private ChannelMixerSettings m_ChannelMixer;
        private ColorWheelsSettings m_ColorWheels;
        private Dictionary<SerializedProperty, Color> m_CurveDict;

        private CurveEditor m_CurveEditor;
        private CurvesSettings m_Curves;
        private readonly Vector3[] m_CurveVertices = new Vector3[k_CurveResolution];
        private readonly Vector3[] m_LineVertices = new Vector3[2];
        private Rect m_NeutralCurveRect;
        private readonly Vector3[] m_RectVertices = new Vector3[4];

        private TonemappingSettings m_Tonemapping;

        public override void OnEnable()
        {
            // Tonemapping settings
            m_Tonemapping = new TonemappingSettings
            {
                tonemapper = FindSetting((ColorGradingModel.Settings x) => x.tonemapping.tonemapper),
                neutralBlackIn = FindSetting((ColorGradingModel.Settings x) => x.tonemapping.neutralBlackIn),
                neutralWhiteIn = FindSetting((ColorGradingModel.Settings x) => x.tonemapping.neutralWhiteIn),
                neutralBlackOut = FindSetting((ColorGradingModel.Settings x) => x.tonemapping.neutralBlackOut),
                neutralWhiteOut = FindSetting((ColorGradingModel.Settings x) => x.tonemapping.neutralWhiteOut),
                neutralWhiteLevel = FindSetting((ColorGradingModel.Settings x) => x.tonemapping.neutralWhiteLevel),
                neutralWhiteClip = FindSetting((ColorGradingModel.Settings x) => x.tonemapping.neutralWhiteClip)
            };

            // Basic settings
            m_Basic = new BasicSettings
            {
                exposure = FindSetting((ColorGradingModel.Settings x) => x.basic.postExposure),
                temperature = FindSetting((ColorGradingModel.Settings x) => x.basic.temperature),
                tint = FindSetting((ColorGradingModel.Settings x) => x.basic.tint),
                hueShift = FindSetting((ColorGradingModel.Settings x) => x.basic.hueShift),
                saturation = FindSetting((ColorGradingModel.Settings x) => x.basic.saturation),
                contrast = FindSetting((ColorGradingModel.Settings x) => x.basic.contrast)
            };

            // Channel mixer
            m_ChannelMixer = new ChannelMixerSettings
            {
                channels = new[]
                {
                    FindSetting((ColorGradingModel.Settings x) => x.channelMixer.red),
                    FindSetting((ColorGradingModel.Settings x) => x.channelMixer.green),
                    FindSetting((ColorGradingModel.Settings x) => x.channelMixer.blue)
                },
                currentEditingChannel =
                    FindSetting((ColorGradingModel.Settings x) => x.channelMixer.currentEditingChannel)
            };

            // Color wheels
            m_ColorWheels = new ColorWheelsSettings
            {
                mode = FindSetting((ColorGradingModel.Settings x) => x.colorWheels.mode),
                log = FindSetting((ColorGradingModel.Settings x) => x.colorWheels.log),
                linear = FindSetting((ColorGradingModel.Settings x) => x.colorWheels.linear)
            };

            // Curves
            m_Curves = new CurvesSettings
            {
                master = FindSetting((ColorGradingModel.Settings x) => x.curves.master.curve),
                red = FindSetting((ColorGradingModel.Settings x) => x.curves.red.curve),
                green = FindSetting((ColorGradingModel.Settings x) => x.curves.green.curve),
                blue = FindSetting((ColorGradingModel.Settings x) => x.curves.blue.curve),

                hueVShue = FindSetting((ColorGradingModel.Settings x) => x.curves.hueVShue.curve),
                hueVSsat = FindSetting((ColorGradingModel.Settings x) => x.curves.hueVSsat.curve),
                satVSsat = FindSetting((ColorGradingModel.Settings x) => x.curves.satVSsat.curve),
                lumVSsat = FindSetting((ColorGradingModel.Settings x) => x.curves.lumVSsat.curve),

                currentEditingCurve = FindSetting((ColorGradingModel.Settings x) => x.curves.e_CurrentEditingCurve),
                curveY = FindSetting((ColorGradingModel.Settings x) => x.curves.e_CurveY),
                curveR = FindSetting((ColorGradingModel.Settings x) => x.curves.e_CurveR),
                curveG = FindSetting((ColorGradingModel.Settings x) => x.curves.e_CurveG),
                curveB = FindSetting((ColorGradingModel.Settings x) => x.curves.e_CurveB)
            };

            // Prepare the curve editor and extract curve display settings
            m_CurveDict = new Dictionary<SerializedProperty, Color>();

            var settings = CurveEditor.Settings.defaultSettings;

            m_CurveEditor = new CurveEditor(settings);
            AddCurve(m_Curves.master, new Color(1f, 1f, 1f), 2, false);
            AddCurve(m_Curves.red, new Color(1f, 0f, 0f), 2, false);
            AddCurve(m_Curves.green, new Color(0f, 1f, 0f), 2, false);
            AddCurve(m_Curves.blue, new Color(0f, 0.5f, 1f), 2, false);
            AddCurve(m_Curves.hueVShue, new Color(1f, 1f, 1f), 0, true);
            AddCurve(m_Curves.hueVSsat, new Color(1f, 1f, 1f), 0, true);
            AddCurve(m_Curves.satVSsat, new Color(1f, 1f, 1f), 0, false);
            AddCurve(m_Curves.lumVSsat, new Color(1f, 1f, 1f), 0, false);
        }

        private void AddCurve(SerializedProperty prop, Color color, uint minPointCount, bool loop)
        {
            var state = CurveEditor.CurveState.defaultState;
            state.color = color;
            state.visible = false;
            state.minPointCount = minPointCount;
            state.onlyShowHandlesOnSelection = true;
            state.zeroKeyConstantValue = 0.5f;
            state.loopInBounds = loop;
            m_CurveEditor.Add(prop, state);
            m_CurveDict.Add(prop, color);
        }

        public override void OnDisable()
        {
            m_CurveEditor.RemoveAll();
        }

        public override void OnInspectorGUI()
        {
            DoGUIFor("Tonemapping", DoTonemappingGUI);
            EditorGUILayout.Space();
            DoGUIFor("Basic", DoBasicGUI);
            EditorGUILayout.Space();
            DoGUIFor("Channel Mixer", DoChannelMixerGUI);
            EditorGUILayout.Space();
            DoGUIFor("Trackballs", DoColorWheelsGUI);
            EditorGUILayout.Space();
            DoGUIFor("Grading Curves", DoCurvesGUI);
        }

        private void DoGUIFor(string title, Action func)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            func();
            EditorGUI.indentLevel--;
        }

        private void DoTonemappingGUI()
        {
            var tid = EditorGUILayout.Popup(EditorGUIHelper.GetContent("Tonemapper"), m_Tonemapping.tonemapper.intValue,
                s_Tonemappers);

            if (tid == (int) ColorGradingModel.Tonemapper.Neutral)
            {
                DrawNeutralTonemappingCurve();

                EditorGUILayout.PropertyField(m_Tonemapping.neutralBlackIn, EditorGUIHelper.GetContent("Black In"));
                EditorGUILayout.PropertyField(m_Tonemapping.neutralWhiteIn, EditorGUIHelper.GetContent("White In"));
                EditorGUILayout.PropertyField(m_Tonemapping.neutralBlackOut, EditorGUIHelper.GetContent("Black Out"));
                EditorGUILayout.PropertyField(m_Tonemapping.neutralWhiteOut, EditorGUIHelper.GetContent("White Out"));
                EditorGUILayout.PropertyField(m_Tonemapping.neutralWhiteLevel,
                    EditorGUIHelper.GetContent("White Level"));
                EditorGUILayout.PropertyField(m_Tonemapping.neutralWhiteClip, EditorGUIHelper.GetContent("White Clip"));
            }

            m_Tonemapping.tonemapper.intValue = tid;
        }

        private void DrawNeutralTonemappingCurve()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorGUI.indentLevel * 15f);
                m_NeutralCurveRect = GUILayoutUtility.GetRect(128, 80);
            }

            // Background
            m_RectVertices[0] = PointInRect(0f, 0f);
            m_RectVertices[1] = PointInRect(k_NeutralRangeX, 0f);
            m_RectVertices[2] = PointInRect(k_NeutralRangeX, k_NeutralRangeY);
            m_RectVertices[3] = PointInRect(0f, k_NeutralRangeY);

            Handles.DrawSolidRectangleWithOutline(
                m_RectVertices,
                Color.white * 0.1f,
                Color.white * 0.4f
            );

            // Horizontal lines
            for (var i = 1; i < k_NeutralRangeY; i++)
                DrawLine(0, i, k_NeutralRangeX, i, 0.4f);

            // Vertical lines
            for (var i = 1; i < k_NeutralRangeX; i++)
                DrawLine(i, 0, i, k_NeutralRangeY, 0.4f);

            // Label
            Handles.Label(
                PointInRect(0, k_NeutralRangeY) + Vector3.right,
                "Neutral Tonemapper", EditorStyles.miniLabel
            );

            // Precompute some values
            var tonemap = ((ColorGradingModel) target).settings.tonemapping;

            const float scaleFactor = 20f;
            const float scaleFactorHalf = scaleFactor * 0.5f;

            var inBlack = tonemap.neutralBlackIn * scaleFactor + 1f;
            var outBlack = tonemap.neutralBlackOut * scaleFactorHalf + 1f;
            var inWhite = tonemap.neutralWhiteIn / scaleFactor;
            var outWhite = 1f - tonemap.neutralWhiteOut / scaleFactor;
            var blackRatio = inBlack / outBlack;
            var whiteRatio = inWhite / outWhite;

            const float a = 0.2f;
            var b = Mathf.Max(0f, Mathf.LerpUnclamped(0.57f, 0.37f, blackRatio));
            var c = Mathf.LerpUnclamped(0.01f, 0.24f, whiteRatio);
            var d = Mathf.Max(0f, Mathf.LerpUnclamped(0.02f, 0.20f, blackRatio));
            const float e = 0.02f;
            const float f = 0.30f;
            var whiteLevel = tonemap.neutralWhiteLevel;
            var whiteClip = tonemap.neutralWhiteClip / scaleFactorHalf;

            // Tonemapping curve
            var vcount = 0;
            while (vcount < k_CurveResolution)
            {
                var x = k_NeutralRangeX * vcount / (k_CurveResolution - 1);
                var y = NeutralTonemap(x, a, b, c, d, e, f, whiteLevel, whiteClip);

                if (y < k_NeutralRangeY)
                {
                    m_CurveVertices[vcount++] = PointInRect(x, y);
                }
                else
                {
                    if (vcount > 1)
                    {
                        // Extend the last segment to the top edge of the rect.
                        var v1 = m_CurveVertices[vcount - 2];
                        var v2 = m_CurveVertices[vcount - 1];
                        var clip = (m_NeutralCurveRect.y - v1.y) / (v2.y - v1.y);
                        m_CurveVertices[vcount - 1] = v1 + (v2 - v1) * clip;
                    }

                    break;
                }
            }

            if (vcount > 1)
            {
                Handles.color = Color.white * 0.9f;
                Handles.DrawAAPolyLine(2.0f, vcount, m_CurveVertices);
            }
        }

        private void DrawLine(float x1, float y1, float x2, float y2, float grayscale)
        {
            m_LineVertices[0] = PointInRect(x1, y1);
            m_LineVertices[1] = PointInRect(x2, y2);
            Handles.color = Color.white * grayscale;
            Handles.DrawAAPolyLine(2f, m_LineVertices);
        }

        private Vector3 PointInRect(float x, float y)
        {
            x = Mathf.Lerp(m_NeutralCurveRect.x, m_NeutralCurveRect.xMax, x / k_NeutralRangeX);
            y = Mathf.Lerp(m_NeutralCurveRect.yMax, m_NeutralCurveRect.y, y / k_NeutralRangeY);
            return new Vector3(x, y, 0);
        }

        private float NeutralCurve(float x, float a, float b, float c, float d, float e, float f)
        {
            return (x * (a * x + c * b) + d * e) / (x * (a * x + b) + d * f) - e / f;
        }

        private float NeutralTonemap(float x, float a, float b, float c, float d, float e, float f, float whiteLevel,
            float whiteClip)
        {
            x = Mathf.Max(0f, x);

            // Tonemap
            var whiteScale = 1f / NeutralCurve(whiteLevel, a, b, c, d, e, f);
            x = NeutralCurve(x * whiteScale, a, b, c, d, e, f);
            x *= whiteScale;

            // Post-curve white point adjustment
            x /= whiteClip;

            return x;
        }

        private void DoBasicGUI()
        {
            EditorGUILayout.PropertyField(m_Basic.exposure, EditorGUIHelper.GetContent("Post Exposure (EV)"));
            EditorGUILayout.PropertyField(m_Basic.temperature);
            EditorGUILayout.PropertyField(m_Basic.tint);
            EditorGUILayout.PropertyField(m_Basic.hueShift);
            EditorGUILayout.PropertyField(m_Basic.saturation);
            EditorGUILayout.PropertyField(m_Basic.contrast);
        }

        private void DoChannelMixerGUI()
        {
            var currentChannel = m_ChannelMixer.currentEditingChannel.intValue;

            EditorGUI.BeginChangeCheck();
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PrefixLabel("Channel");
                    if (GUILayout.Toggle(currentChannel == 0, EditorGUIHelper.GetContent("Red|Red output channel."),
                        EditorStyles.miniButtonLeft)) currentChannel = 0;
                    if (GUILayout.Toggle(currentChannel == 1, EditorGUIHelper.GetContent("Green|Green output channel."),
                        EditorStyles.miniButtonMid)) currentChannel = 1;
                    if (GUILayout.Toggle(currentChannel == 2, EditorGUIHelper.GetContent("Blue|Blue output channel."),
                        EditorStyles.miniButtonRight)) currentChannel = 2;
                }
            }
            if (EditorGUI.EndChangeCheck()) GUI.FocusControl(null);

            var serializedChannel = m_ChannelMixer.channels[currentChannel];
            m_ChannelMixer.currentEditingChannel.intValue = currentChannel;

            var v = serializedChannel.vector3Value;
            v.x = EditorGUILayout.Slider(
                EditorGUIHelper.GetContent("Red|Modify influence of the red channel within the overall mix."), v.x, -2f,
                2f);
            v.y = EditorGUILayout.Slider(
                EditorGUIHelper.GetContent("Green|Modify influence of the green channel within the overall mix."), v.y,
                -2f, 2f);
            v.z = EditorGUILayout.Slider(
                EditorGUIHelper.GetContent("Blue|Modify influence of the blue channel within the overall mix."), v.z,
                -2f, 2f);
            serializedChannel.vector3Value = v;
        }

        private void DoColorWheelsGUI()
        {
            var wheelMode = m_ColorWheels.mode.intValue;

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(15);
                if (GUILayout.Toggle(wheelMode == (int) ColorGradingModel.ColorWheelMode.Linear, "Linear",
                    EditorStyles.miniButtonLeft)) wheelMode = (int) ColorGradingModel.ColorWheelMode.Linear;
                if (GUILayout.Toggle(wheelMode == (int) ColorGradingModel.ColorWheelMode.Log, "Log",
                    EditorStyles.miniButtonRight)) wheelMode = (int) ColorGradingModel.ColorWheelMode.Log;
            }

            m_ColorWheels.mode.intValue = wheelMode;
            EditorGUILayout.Space();

            if (wheelMode == (int) ColorGradingModel.ColorWheelMode.Linear)
            {
                EditorGUILayout.PropertyField(m_ColorWheels.linear);
                WheelSetTitle(GUILayoutUtility.GetLastRect(), "Linear Controls");
            }
            else if (wheelMode == (int) ColorGradingModel.ColorWheelMode.Log)
            {
                EditorGUILayout.PropertyField(m_ColorWheels.log);
                WheelSetTitle(GUILayoutUtility.GetLastRect(), "Log Controls");
            }
        }

        private static void WheelSetTitle(Rect position, string label)
        {
            var matrix = GUI.matrix;
            var rect = new Rect(position.x - 10f, position.y, TrackballGroupDrawer.m_Size, TrackballGroupDrawer.m_Size);
            GUIUtility.RotateAroundPivot(-90f, rect.center);
            GUI.Label(rect, label, FxStyles.centeredMiniLabel);
            GUI.matrix = matrix;
        }

        private void ResetVisibleCurves()
        {
            foreach (var curve in m_CurveDict)
            {
                var state = m_CurveEditor.GetCurveState(curve.Key);
                state.visible = false;
                m_CurveEditor.SetCurveState(curve.Key, state);
            }
        }

        private void SetCurveVisible(SerializedProperty prop)
        {
            var state = m_CurveEditor.GetCurveState(prop);
            state.visible = true;
            m_CurveEditor.SetCurveState(prop, state);
        }

        private bool SpecialToggle(bool value, string name, out bool rightClicked)
        {
            var rect = GUILayoutUtility.GetRect(EditorGUIHelper.GetContent(name), EditorStyles.toolbarButton);

            var e = Event.current;
            rightClicked = e.type == EventType.MouseUp && rect.Contains(e.mousePosition) && e.button == 1;

            return GUI.Toggle(rect, value, name, EditorStyles.toolbarButton);
        }

        private void DoCurvesGUI()
        {
            EditorGUILayout.Space();
            EditorGUI.indentLevel -= 2;
            ResetVisibleCurves();

            using (new EditorGUI.DisabledGroupScope(serializedProperty.serializedObject.isEditingMultipleObjects))
            {
                var curveEditingId = 0;

                // Top toolbar
                using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    curveEditingId = EditorGUILayout.Popup(m_Curves.currentEditingCurve.intValue, s_Curves,
                        EditorStyles.toolbarPopup, GUILayout.MaxWidth(150f));
                    bool y = false, r = false, g = false, b = false;

                    if (curveEditingId == 0)
                    {
                        EditorGUILayout.Space();

                        bool rightClickedY, rightClickedR, rightClickedG, rightClickedB;

                        y = SpecialToggle(m_Curves.curveY.boolValue, "Y", out rightClickedY);
                        r = SpecialToggle(m_Curves.curveR.boolValue, "R", out rightClickedR);
                        g = SpecialToggle(m_Curves.curveG.boolValue, "G", out rightClickedG);
                        b = SpecialToggle(m_Curves.curveB.boolValue, "B", out rightClickedB);

                        if (!y && !r && !g && !b)
                        {
                            r = g = b = false;
                            y = true;
                        }

                        if (rightClickedY || rightClickedR || rightClickedG || rightClickedB)
                        {
                            y = rightClickedY;
                            r = rightClickedR;
                            g = rightClickedG;
                            b = rightClickedB;
                        }

                        if (y) SetCurveVisible(m_Curves.master);
                        if (r) SetCurveVisible(m_Curves.red);
                        if (g) SetCurveVisible(m_Curves.green);
                        if (b) SetCurveVisible(m_Curves.blue);

                        m_Curves.curveY.boolValue = y;
                        m_Curves.curveR.boolValue = r;
                        m_Curves.curveG.boolValue = g;
                        m_Curves.curveB.boolValue = b;
                    }
                    else
                    {
                        switch (curveEditingId)
                        {
                            case 1:
                                SetCurveVisible(m_Curves.hueVShue);
                                break;
                            case 2:
                                SetCurveVisible(m_Curves.hueVSsat);
                                break;
                            case 3:
                                SetCurveVisible(m_Curves.satVSsat);
                                break;
                            case 4:
                                SetCurveVisible(m_Curves.lumVSsat);
                                break;
                        }
                    }

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Reset", EditorStyles.toolbarButton))
                        switch (curveEditingId)
                        {
                            case 0:
                                if (y) m_Curves.master.animationCurveValue = AnimationCurve.Linear(0f, 0f, 1f, 1f);
                                if (r) m_Curves.red.animationCurveValue = AnimationCurve.Linear(0f, 0f, 1f, 1f);
                                if (g) m_Curves.green.animationCurveValue = AnimationCurve.Linear(0f, 0f, 1f, 1f);
                                if (b) m_Curves.blue.animationCurveValue = AnimationCurve.Linear(0f, 0f, 1f, 1f);
                                break;
                            case 1:
                                m_Curves.hueVShue.animationCurveValue = new AnimationCurve();
                                break;
                            case 2:
                                m_Curves.hueVSsat.animationCurveValue = new AnimationCurve();
                                break;
                            case 3:
                                m_Curves.satVSsat.animationCurveValue = new AnimationCurve();
                                break;
                            case 4:
                                m_Curves.lumVSsat.animationCurveValue = new AnimationCurve();
                                break;
                        }

                    m_Curves.currentEditingCurve.intValue = curveEditingId;
                }

                // Curve area
                var settings = m_CurveEditor.settings;
                var rect = GUILayoutUtility.GetAspectRect(2f);
                var innerRect = settings.padding.Remove(rect);

                if (Event.current.type == EventType.Repaint)
                {
                    // Background
                    EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f, 1f));

                    if (s_MaterialSpline == null)
                        s_MaterialSpline = new Material(Shader.Find("Hidden/Post FX/UI/Curve Background"))
                            {hideFlags = HideFlags.HideAndDontSave};

                    if (curveEditingId == 1 || curveEditingId == 2)
                        DrawBackgroundTexture(innerRect, 0);
                    else if (curveEditingId == 3 || curveEditingId == 4)
                        DrawBackgroundTexture(innerRect, 1);

                    // Bounds
                    Handles.color = Color.white;
                    Handles.DrawSolidRectangleWithOutline(innerRect, Color.clear, new Color(0.8f, 0.8f, 0.8f, 0.5f));

                    // Grid setup
                    Handles.color = new Color(1f, 1f, 1f, 0.05f);
                    var hLines = (int) Mathf.Sqrt(innerRect.width);
                    var vLines = (int) (hLines / (innerRect.width / innerRect.height));

                    // Vertical grid
                    var gridOffset = Mathf.FloorToInt(innerRect.width / hLines);
                    var gridPadding = (int) innerRect.width % hLines / 2;

                    for (var i = 1; i < hLines; i++)
                    {
                        var offset = i * Vector2.right * gridOffset;
                        offset.x += gridPadding;
                        Handles.DrawLine(innerRect.position + offset,
                            new Vector2(innerRect.x, innerRect.yMax - 1) + offset);
                    }

                    // Horizontal grid
                    gridOffset = Mathf.FloorToInt(innerRect.height / vLines);
                    gridPadding = (int) innerRect.height % vLines / 2;

                    for (var i = 1; i < vLines; i++)
                    {
                        var offset = i * Vector2.up * gridOffset;
                        offset.y += gridPadding;
                        Handles.DrawLine(innerRect.position + offset,
                            new Vector2(innerRect.xMax - 1, innerRect.y) + offset);
                    }
                }

                // Curve editor
                if (m_CurveEditor.OnGUI(rect))
                {
                    Repaint();
                    GUI.changed = true;
                }

                if (Event.current.type == EventType.Repaint)
                {
                    // Borders
                    Handles.color = Color.black;
                    Handles.DrawLine(new Vector2(rect.x, rect.y - 18f), new Vector2(rect.xMax, rect.y - 18f));
                    Handles.DrawLine(new Vector2(rect.x, rect.y - 19f), new Vector2(rect.x, rect.yMax));
                    Handles.DrawLine(new Vector2(rect.x, rect.yMax), new Vector2(rect.xMax, rect.yMax));
                    Handles.DrawLine(new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.y - 18f));

                    // Selection info
                    var selection = m_CurveEditor.GetSelection();

                    if (selection.curve != null && selection.keyframeIndex > -1)
                    {
                        var key = selection.keyframe.Value;
                        var infoRect = innerRect;
                        infoRect.x += 5f;
                        infoRect.width = 100f;
                        infoRect.height = 30f;
                        GUI.Label(infoRect,
                            string.Format("{0}\n{1}", key.time.ToString("F3"), key.value.ToString("F3")),
                            FxStyles.preLabel);
                    }
                }
            }

            /*
            EditorGUILayout.HelpBox(
                @"Curve editor cheat sheet:
- [Del] or [Backspace] to remove a key
- [Ctrl] to break a tangent handle
- [Shift] to align tangent handles
- [Double click] to create a key on the curve(s) at mouse position
- [Alt] + [Double click] to create a key on the curve(s) at a given time",
                MessageType.Info);
            */

            EditorGUILayout.Space();
            EditorGUI.indentLevel += 2;
        }

        private void DrawBackgroundTexture(Rect rect, int pass)
        {
            var scale = EditorGUIUtility.pixelsPerPoint;

            var oldRt = RenderTexture.active;
            var rt = RenderTexture.GetTemporary(Mathf.CeilToInt(rect.width * scale),
                Mathf.CeilToInt(rect.height * scale), 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            s_MaterialSpline.SetFloat("_DisabledState", GUI.enabled ? 1f : 0.5f);
            s_MaterialSpline.SetFloat("_PixelScaling", EditorGUIUtility.pixelsPerPoint);

            Graphics.Blit(null, rt, s_MaterialSpline, pass);
            RenderTexture.active = oldRt;

            GUI.DrawTexture(rect, rt);
            RenderTexture.ReleaseTemporary(rt);
        }

        private struct TonemappingSettings
        {
            public SerializedProperty tonemapper;
            public SerializedProperty neutralBlackIn;
            public SerializedProperty neutralWhiteIn;
            public SerializedProperty neutralBlackOut;
            public SerializedProperty neutralWhiteOut;
            public SerializedProperty neutralWhiteLevel;
            public SerializedProperty neutralWhiteClip;
        }

        private struct BasicSettings
        {
            public SerializedProperty exposure;
            public SerializedProperty temperature;
            public SerializedProperty tint;
            public SerializedProperty hueShift;
            public SerializedProperty saturation;
            public SerializedProperty contrast;
        }

        private struct ChannelMixerSettings
        {
            public SerializedProperty[] channels;
            public SerializedProperty currentEditingChannel;
        }

        private struct ColorWheelsSettings
        {
            public SerializedProperty mode;
            public SerializedProperty log;
            public SerializedProperty linear;
        }

        private struct CurvesSettings
        {
            public SerializedProperty master;
            public SerializedProperty red;
            public SerializedProperty green;
            public SerializedProperty blue;

            public SerializedProperty hueVShue;
            public SerializedProperty hueVSsat;
            public SerializedProperty satVSsat;
            public SerializedProperty lumVSsat;

            public SerializedProperty currentEditingCurve;
            public SerializedProperty curveY;
            public SerializedProperty curveR;
            public SerializedProperty curveG;
            public SerializedProperty curveB;
        }
    }
}